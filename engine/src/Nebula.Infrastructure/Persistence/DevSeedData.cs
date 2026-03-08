using Microsoft.EntityFrameworkCore;
using Nebula.Domain.Entities;
using Nebula.Domain.Workflow;

namespace Nebula.Infrastructure.Persistence;

public static class DevSeedData
{
    private const int BrokerSeedCount = 240;
    private const int SubmissionSeedCount = 320;
    private const int RenewalSeedCount = 320;
    private const int AccountSeedCount = 420;

    private static readonly string[] States = ["CA", "TX", "NY", "FL", "WA", "IL", "GA", "NC", "AZ", "CO", "NJ", "PA"];
    private static readonly string[] Regions = ["West", "Central", "East", "South"];
    private static readonly string[] Industries = ["Manufacturing", "Healthcare", "Construction", "Retail", "Logistics", "Technology", "Hospitality", "Agriculture"];
    private static readonly string[] BrokerRoles = ["Primary", "Producer", "Account Manager", "Service", "Accounting"];
    private static readonly string[] TaskPriorities = ["Low", "Normal", "High", "Urgent"];

    private static readonly Dictionary<string, string[]> SubmissionNextStates = new(StringComparer.Ordinal)
    {
        ["Received"] = ["Triaging"],
        ["Triaging"] = ["WaitingOnBroker", "WaitingOnDocuments", "ReadyForUWReview"],
        ["WaitingOnBroker"] = ["Triaging", "WaitingOnDocuments", "ReadyForUWReview"],
        ["WaitingOnDocuments"] = ["WaitingOnBroker", "ReadyForUWReview"],
        ["ReadyForUWReview"] = ["InReview", "QuotePreparation"],
        ["InReview"] = ["WaitingOnBroker", "WaitingOnDocuments", "QuotePreparation", "Quoted", "RequoteRequested"],
        ["QuotePreparation"] = ["Quoted", "RequoteRequested"],
        ["Quoted"] = ["RequoteRequested", "BindRequested"],
        ["RequoteRequested"] = ["InReview", "QuotePreparation", "Quoted"],
        ["BindRequested"] = ["Binding", "Bound"],
        ["Binding"] = ["Bound"],
    };

    private static readonly Dictionary<string, string[]> RenewalNextStates = new(StringComparer.Ordinal)
    {
        ["Created"] = ["Early", "DataReview"],
        ["Early"] = ["DataReview", "OutreachStarted"],
        ["DataReview"] = ["OutreachStarted", "WaitingOnBroker", "InReview"],
        ["OutreachStarted"] = ["WaitingOnBroker", "InReview", "Quoted"],
        ["WaitingOnBroker"] = ["DataReview", "InReview", "Quoted"],
        ["InReview"] = ["Quoted", "Negotiation"],
        ["Quoted"] = ["Negotiation", "BindRequested", "Bound"],
        ["Negotiation"] = ["Quoted", "BindRequested", "Bound"],
        ["BindRequested"] = ["Bound"],
    };

    public static async Task SeedDevDataAsync(AppDbContext db)
    {
        await EnsureReferenceStatusesAsync(db);

        // F0009: Idempotently ensure the dev broker tenant link exists even when data was
        // already seeded before F0009 migration added the BrokerTenantId column.
        await EnsureDevBrokerTenantIdAsync(db);

        if (await db.Submissions.AnyAsync()) return; // app data already seeded

        var rng = new Random(20260226);
        var now = DateTime.UtcNow;

        var userProfiles = BuildUserProfiles(now);
        db.UserProfiles.AddRange(userProfiles);

        var userIds = userProfiles.Select(u => u.Id).ToArray();
        var userNameById = userProfiles.ToDictionary(u => u.Id, u => u.DisplayName);

        var mgas = new[]
        {
            new MGA { Name = "Acme MGA", ExternalCode = "ACME-001", Status = "Active", CreatedAt = now, UpdatedAt = now, CreatedByUserId = userIds[0], UpdatedByUserId = userIds[0] },
            new MGA { Name = "Orion Specialty MGA", ExternalCode = "ORION-002", Status = "Active", CreatedAt = now, UpdatedAt = now, CreatedByUserId = userIds[1], UpdatedByUserId = userIds[1] },
        };
        db.MGAs.AddRange(mgas);
        await db.SaveChangesAsync();

        var programs = new[]
        {
            new Nebula.Domain.Entities.Program { Name = "General Liability", ProgramCode = "GL-001", MgaId = mgas[0].Id, ManagedByUserId = userIds[0], CreatedAt = now, UpdatedAt = now, CreatedByUserId = userIds[0], UpdatedByUserId = userIds[0] },
            new Nebula.Domain.Entities.Program { Name = "Excess Liability", ProgramCode = "XS-002", MgaId = mgas[0].Id, ManagedByUserId = userIds[2], CreatedAt = now, UpdatedAt = now, CreatedByUserId = userIds[2], UpdatedByUserId = userIds[2] },
            new Nebula.Domain.Entities.Program { Name = "Property CAT", ProgramCode = "PROP-003", MgaId = mgas[1].Id, ManagedByUserId = userIds[1], CreatedAt = now, UpdatedAt = now, CreatedByUserId = userIds[1], UpdatedByUserId = userIds[1] },
        };
        db.Programs.AddRange(programs);
        await db.SaveChangesAsync();

        var accounts = BuildAccounts(AccountSeedCount, now, rng, userIds);
        db.Accounts.AddRange(accounts);
        await db.SaveChangesAsync();

        var brokers = BuildBrokers(BrokerSeedCount, now, rng, userIds, mgas, programs);
        // F0009 §9: Link the first Active broker to BrokerUser dev tenant for test identity validation.
        var devBroker = brokers.FirstOrDefault(b => b.Status == "Active") ?? brokers[0];
        devBroker.BrokerTenantId = BrokerUserDevTenantId;
        db.Brokers.AddRange(brokers);
        await db.SaveChangesAsync();

        db.BrokerRegions.AddRange(BuildBrokerRegions(brokers, rng));
        db.Contacts.AddRange(BuildContacts(brokers, now, rng, userIds));

        var submissions = new List<Submission>(SubmissionSeedCount);
        var renewals = new List<Renewal>(RenewalSeedCount);
        var transitions = new List<WorkflowTransition>(SubmissionSeedCount * 6 + RenewalSeedCount * 5);
        var timelineEvents = new List<ActivityTimelineEvent>(400);

        var boundSubmissionIds = new List<Guid>();

        for (var i = 0; i < SubmissionSeedCount; i++)
        {
            var account = accounts[rng.Next(accounts.Count)];
            var broker = brokers[rng.Next(brokers.Count)];
            var assignedTo = userIds[rng.Next(userIds.Length)];
            var path = GenerateWorkflowPath(
                rng,
                "Received",
                SubmissionNextStates,
                OpportunityStatusCatalog.SubmissionTerminalStatusCodes,
                chooseSubmissionTerminal: true);
            var createdAt = now.AddDays(-rng.Next(Math.Max(path.Count * 10, 21), 365)).AddHours(-rng.Next(0, 24));

            var updatedAt = createdAt;
            for (var step = 1; step < path.Count; step++)
            {
                var occurredAt = updatedAt.AddDays(RandomStepDays(rng, path[step - 1], path[step]))
                    .AddHours(rng.Next(0, 8));
                updatedAt = occurredAt;
                transitions.Add(new WorkflowTransition
                {
                    WorkflowType = "Submission",
                    EntityId = Guid.Empty, // patched after entity instantiation
                    FromState = path[step - 1],
                    ToState = path[step],
                    Reason = BuildTransitionReason(rng, "Submission", path[step - 1], path[step]),
                    ActorUserId = assignedTo,
                    OccurredAt = occurredAt,
                });
            }

            var submission = new Submission
            {
                AccountId = account.Id,
                BrokerId = broker.Id,
                ProgramId = rng.NextDouble() < 0.7 ? programs[rng.Next(programs.Length)].Id : null,
                CurrentStatus = path[^1],
                EffectiveDate = now.AddDays(rng.Next(-30, 180)),
                PremiumEstimate = Math.Round((decimal)(rng.Next(12_000, 250_000) + rng.NextDouble()), 2),
                AssignedToUserId = assignedTo,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                CreatedByUserId = assignedTo,
                UpdatedByUserId = assignedTo,
            };
            submissions.Add(submission);

            if (submission.CurrentStatus == "Bound")
                boundSubmissionIds.Add(submission.Id);

            PatchRecentTransitionsEntityId(transitions, submission.Id);

            if (i < 120 || rng.NextDouble() < 0.25)
            {
                timelineEvents.Add(new ActivityTimelineEvent
                {
                    EntityType = "Submission",
                    EntityId = submission.Id,
                    EventType = "SubmissionCreated",
                    EventDescription = $"Submission created for {account.Name}",
                    ActorUserId = assignedTo,
                    ActorDisplayName = userNameById[assignedTo],
                    OccurredAt = createdAt,
                });
            }
        }

        db.Submissions.AddRange(submissions);
        await db.SaveChangesAsync();

        for (var i = 0; i < RenewalSeedCount; i++)
        {
            var account = accounts[rng.Next(accounts.Count)];
            var broker = brokers[rng.Next(brokers.Count)];
            var assignedTo = userIds[rng.Next(userIds.Length)];
            var path = GenerateWorkflowPath(
                rng,
                "Created",
                RenewalNextStates,
                OpportunityStatusCatalog.RenewalTerminalStatusCodes,
                chooseSubmissionTerminal: false);
            var createdAt = now.AddDays(-rng.Next(Math.Max(path.Count * 10, 30), 365)).AddHours(-rng.Next(0, 24));

            var updatedAt = createdAt;
            for (var step = 1; step < path.Count; step++)
            {
                var occurredAt = updatedAt.AddDays(RandomStepDays(rng, path[step - 1], path[step]))
                    .AddHours(rng.Next(0, 8));
                updatedAt = occurredAt;
                transitions.Add(new WorkflowTransition
                {
                    WorkflowType = "Renewal",
                    EntityId = Guid.Empty,
                    FromState = path[step - 1],
                    ToState = path[step],
                    Reason = BuildTransitionReason(rng, "Renewal", path[step - 1], path[step]),
                    ActorUserId = assignedTo,
                    OccurredAt = occurredAt,
                });
            }

            var renewal = new Renewal
            {
                AccountId = account.Id,
                BrokerId = broker.Id,
                SubmissionId = boundSubmissionIds.Count > 0 && rng.NextDouble() < 0.55 ? boundSubmissionIds[rng.Next(boundSubmissionIds.Count)] : null,
                CurrentStatus = path[^1],
                RenewalDate = now.AddDays(rng.Next(-60, 180)),
                AssignedToUserId = assignedTo,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                CreatedByUserId = assignedTo,
                UpdatedByUserId = assignedTo,
            };
            renewals.Add(renewal);

            PatchRecentTransitionsEntityId(transitions, renewal.Id);

            if (i < 120 || rng.NextDouble() < 0.25)
            {
                timelineEvents.Add(new ActivityTimelineEvent
                {
                    EntityType = "Renewal",
                    EntityId = renewal.Id,
                    EventType = "RenewalCreated",
                    EventDescription = $"Renewal created for {account.Name}",
                    ActorUserId = assignedTo,
                    ActorDisplayName = userNameById[assignedTo],
                    OccurredAt = createdAt,
                });
            }
        }

        db.Renewals.AddRange(renewals);
        db.WorkflowTransitions.AddRange(transitions);

        var tasks = BuildTasks(now, rng, userIds, submissions, renewals, brokers);
        db.Tasks.AddRange(tasks);

        timelineEvents.AddRange(BuildBrokerTimelineEvents(now, rng, userNameById, brokers));
        timelineEvents.AddRange(BuildTransitionTimelineEvents(rng, userNameById, transitions));
        db.ActivityTimelineEvents.AddRange(timelineEvents.OrderByDescending(e => e.OccurredAt).Take(500));

        await db.SaveChangesAsync();
    }

    // Dev seed IdP issuer (authentik local dev)
    private const string DevIdpIssuer = "http://localhost:9000/application/o/nebula/";

    // BrokerUser dev seed tenant ID — must match authentik blueprint broker001 property mapping (F0009 §9).
    internal const string BrokerUserDevTenantId = "broker001-tenant-001";

    // IdpSubject values must match authentik usernames (sub_mode: user_username in blueprint).
    // Each entry here corresponds to a provisioned authentik user; data is seeded to these UserProfile IDs
    // so that logging in as the matching authentik user surfaces seeded submissions, tasks, and renewals.
    private static List<UserProfile> BuildUserProfiles(DateTime now) =>
    [
        new UserProfile { IdpIssuer = DevIdpIssuer, IdpSubject = "akadmin", Email = "akadmin@nebula.local", DisplayName = "Admin User", Department = "Operations", RegionsJson = "[\"West\",\"Central\",\"East\",\"South\"]", RolesJson = "[\"Admin\"]", CreatedAt = now, UpdatedAt = now },
        new UserProfile { IdpIssuer = DevIdpIssuer, IdpSubject = "john.miller", Email = "john.miller@nebula.local", DisplayName = "John Miller", Department = "Underwriting", RegionsJson = "[\"East\"]", RolesJson = "[\"Underwriter\"]", CreatedAt = now, UpdatedAt = now },
        new UserProfile { IdpIssuer = DevIdpIssuer, IdpSubject = "lisa.wong", Email = "lisa.wong@nebula.local", DisplayName = "Lisa Wong", Department = "Distribution", RegionsJson = "[\"West\"]", RolesJson = "[\"DistributionUser\"]", CreatedAt = now, UpdatedAt = now },
        // F0009 §9: BrokerUser test identity — broker_tenant_id claim must match BrokerUserDevTenantId below.
        new UserProfile { IdpIssuer = DevIdpIssuer, IdpSubject = "broker001", Email = "broker001@example.local", DisplayName = "Broker User 001", Department = "External", RegionsJson = "[]", RolesJson = "[\"BrokerUser\"]", CreatedAt = now, UpdatedAt = now },
    ];

    private static List<Account> BuildAccounts(int count, DateTime now, Random rng, Guid[] userIds)
    {
        var accounts = new List<Account>(count);
        for (var i = 1; i <= count; i++)
        {
            var createdBy = userIds[rng.Next(userIds.Length)];
            var region = Regions[rng.Next(Regions.Length)];
            var state = States[rng.Next(States.Length)];
            accounts.Add(new Account
            {
                Name = $"{Pick(rng, CompanyPrefixes)} {Pick(rng, CompanySuffixes)} {i:D3}",
                Industry = Industries[rng.Next(Industries.Length)],
                PrimaryState = state,
                Region = region,
                Status = rng.NextDouble() < 0.9 ? "Active" : "Inactive",
                CreatedAt = now.AddDays(-rng.Next(30, 540)),
                UpdatedAt = now.AddDays(-rng.Next(1, 120)),
                CreatedByUserId = createdBy,
                UpdatedByUserId = createdBy,
            });
        }

        return accounts;
    }

    private static List<Broker> BuildBrokers(
        int count,
        DateTime now,
        Random rng,
        Guid[] userIds,
        MGA[] mgas,
        Nebula.Domain.Entities.Program[] programs)
    {
        var brokers = new List<Broker>(count);
        for (var i = 1; i <= count; i++)
        {
            var createdBy = userIds[rng.Next(userIds.Length)];
            var state = States[rng.Next(States.Length)];
            var status = WeightedPick(rng,
                ("Active", 72),
                ("Pending", 18),
                ("Inactive", 10));

            brokers.Add(new Broker
            {
                LegalName = $"{Pick(rng, BrokerNamePrefixes)} {Pick(rng, BrokerNameSuffixes)} {i:D3}",
                LicenseNumber = $"{state}-{2024 + (i % 3)}-{i:D5}",
                State = state,
                Status = status,
                Email = $"broker{i:D3}@example.local",
                Phone = $"+1-{rng.Next(200, 999)}-{rng.Next(200, 999)}-{rng.Next(1000, 9999)}",
                ManagedByUserId = status == "Inactive" && rng.NextDouble() < 0.5 ? null : userIds[rng.Next(userIds.Length)],
                MgaId = rng.NextDouble() < 0.8 ? mgas[rng.Next(mgas.Length)].Id : null,
                PrimaryProgramId = rng.NextDouble() < 0.75 ? programs[rng.Next(programs.Length)].Id : null,
                CreatedAt = now.AddDays(-rng.Next(20, 720)),
                UpdatedAt = now.AddDays(-rng.Next(0, 120)),
                CreatedByUserId = createdBy,
                UpdatedByUserId = createdBy,
            });
        }

        return brokers;
    }

    private static IEnumerable<BrokerRegion> BuildBrokerRegions(IReadOnlyList<Broker> brokers, Random rng)
    {
        var rows = new List<BrokerRegion>(brokers.Count * 2);
        foreach (var broker in brokers)
        {
            var regionCount = rng.NextDouble() < 0.7 ? 1 : 2;
            var selected = Regions.OrderBy(_ => rng.Next()).Take(regionCount).ToList();
            foreach (var region in selected)
            {
                rows.Add(new BrokerRegion { BrokerId = broker.Id, Region = region });
            }
        }
        return rows;
    }

    private static IEnumerable<Contact> BuildContacts(IReadOnlyList<Broker> brokers, DateTime now, Random rng, Guid[] userIds)
    {
        var contacts = new List<Contact>(brokers.Count * 2);
        for (var i = 0; i < brokers.Count; i++)
        {
            var broker = brokers[i];
            var contactCount = broker.Status == "Inactive" ? 1 : (rng.NextDouble() < 0.5 ? 1 : 2);
            for (var c = 0; c < contactCount; c++)
            {
                var createdBy = userIds[rng.Next(userIds.Length)];
                var first = Pick(rng, FirstNames);
                var last = Pick(rng, LastNames);
                contacts.Add(new Contact
                {
                    BrokerId = broker.Id,
                    FullName = $"{first} {last}",
                    Email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}.{i:D3}{c}@example.local",
                    Phone = $"+1-{rng.Next(200, 999)}-{rng.Next(200, 999)}-{rng.Next(1000, 9999)}",
                    Role = BrokerRoles[rng.Next(BrokerRoles.Length)],
                    CreatedAt = now.AddDays(-rng.Next(10, 600)),
                    UpdatedAt = now.AddDays(-rng.Next(0, 90)),
                    CreatedByUserId = createdBy,
                    UpdatedByUserId = createdBy,
                });
            }
        }

        return contacts;
    }

    private static List<TaskItem> BuildTasks(
        DateTime now,
        Random rng,
        Guid[] userIds,
        IReadOnlyList<Submission> submissions,
        IReadOnlyList<Renewal> renewals,
        IReadOnlyList<Broker> brokers)
    {
        var tasks = new List<TaskItem>(96);
        for (var i = 0; i < 96; i++)
        {
            var assignedTo = userIds[rng.Next(userIds.Length)];
            var status = WeightedPick(rng, ("Open", 48), ("InProgress", 34), ("Done", 18));
            var dueDate = now.Date.AddDays(rng.Next(-10, 21));
            Guid? linkedId = null;
            string? linkedType = null;
            string title;

            switch (rng.Next(3))
            {
                case 0:
                    var sub = submissions[rng.Next(submissions.Count)];
                    linkedId = sub.Id;
                    linkedType = "Submission";
                    title = $"Review submission in {sub.CurrentStatus}";
                    break;
                case 1:
                    var ren = renewals[rng.Next(renewals.Count)];
                    linkedId = ren.Id;
                    linkedType = "Renewal";
                    title = $"Follow renewal {ren.CurrentStatus}";
                    break;
                default:
                    var broker = brokers[rng.Next(brokers.Count)];
                    linkedId = broker.Id;
                    linkedType = "Broker";
                    title = "Broker outreach follow-up";
                    break;
            }

            tasks.Add(new TaskItem
            {
                Title = title,
                Description = rng.NextDouble() < 0.35 ? "Generated dev task for dashboard nudges and task list coverage." : null,
                Status = status,
                Priority = TaskPriorities[rng.Next(TaskPriorities.Length)],
                DueDate = dueDate,
                AssignedToUserId = assignedTo,
                LinkedEntityType = linkedType,
                LinkedEntityId = linkedId,
                CompletedAt = status == "Done" ? dueDate.AddDays(rng.Next(-2, 2)) : null,
                CreatedAt = now.AddDays(-rng.Next(1, 45)),
                UpdatedAt = now.AddDays(-rng.Next(0, 10)),
                CreatedByUserId = assignedTo,
                UpdatedByUserId = assignedTo,
            });
        }

        return tasks;
    }

    private static IEnumerable<ActivityTimelineEvent> BuildBrokerTimelineEvents(
        DateTime now,
        Random rng,
        IReadOnlyDictionary<Guid, string> userNameById,
        IReadOnlyList<Broker> brokers)
    {
        var events = new List<ActivityTimelineEvent>(brokers.Count + 40);
        foreach (var broker in brokers.Take(160))
        {
            var actor = broker.CreatedByUserId;
            events.Add(new ActivityTimelineEvent
            {
                EntityType = "Broker",
                EntityId = broker.Id,
                EventType = "BrokerCreated",
                EventDescription = $"New broker \"{broker.LegalName}\" added",
                ActorUserId = actor,
                ActorDisplayName = userNameById.GetValueOrDefault(actor),
                OccurredAt = broker.CreatedAt,
            });

            if (broker.Status == "Inactive" && rng.NextDouble() < 0.5)
            {
                events.Add(new ActivityTimelineEvent
                {
                    EntityType = "Broker",
                    EntityId = broker.Id,
                    EventType = "BrokerStatusChanged",
                    EventDescription = $"Broker \"{broker.LegalName}\" marked Inactive",
                    ActorUserId = actor,
                    ActorDisplayName = userNameById.GetValueOrDefault(actor),
                    OccurredAt = broker.UpdatedAt,
                });
            }
        }

        return events;
    }

    private static IEnumerable<ActivityTimelineEvent> BuildTransitionTimelineEvents(
        Random rng,
        IReadOnlyDictionary<Guid, string> userNameById,
        IReadOnlyList<WorkflowTransition> transitions)
    {
        return transitions
            .Where(t => rng.NextDouble() < 0.22)
            .OrderByDescending(t => t.OccurredAt)
            .Take(220)
            .Select(t => new ActivityTimelineEvent
            {
                EntityType = t.WorkflowType,
                EntityId = t.EntityId,
                EventType = $"{t.WorkflowType}Transitioned",
                EventDescription = $"{t.WorkflowType} transitioned from {t.FromState} to {t.ToState}",
                ActorUserId = t.ActorUserId,
                ActorDisplayName = userNameById.GetValueOrDefault(t.ActorUserId),
                OccurredAt = t.OccurredAt,
            })
            .ToList();
    }

    private static void PatchRecentTransitionsEntityId(List<WorkflowTransition> transitions, Guid entityId)
    {
        for (var i = transitions.Count - 1; i >= 0 && transitions[i].EntityId == Guid.Empty; i--)
        {
            transitions[i].EntityId = entityId;
        }
    }

    private static List<string> GenerateWorkflowPath(
        Random rng,
        string start,
        IReadOnlyDictionary<string, string[]> nextStateMap,
        IReadOnlySet<string> terminalStatuses,
        bool chooseSubmissionTerminal)
    {
        var path = new List<string> { start };
        var current = start;

        for (var step = 0; step < 9; step++)
        {
            var shouldTerminate = step >= 1 && rng.NextDouble() < (step < 3 ? 0.14 : step < 5 ? 0.28 : 0.52);
            if (shouldTerminate && !terminalStatuses.Contains(current))
            {
                path.Add(chooseSubmissionTerminal
                    ? PickSubmissionTerminal(rng, current)
                    : PickRenewalTerminal(rng, current));
                break;
            }

            if (!nextStateMap.TryGetValue(current, out var nextStates) || nextStates.Length == 0)
                break;

            var next = nextStates[rng.Next(nextStates.Length)];
            path.Add(next);
            current = next;

            if (terminalStatuses.Contains(current))
                break;
        }

        if (!terminalStatuses.Contains(path[^1]) && rng.NextDouble() < 0.38)
        {
            path.Add(chooseSubmissionTerminal
                ? PickSubmissionTerminal(rng, path[^1])
                : PickRenewalTerminal(rng, path[^1]));
        }

        return path;
    }

    private static string PickSubmissionTerminal(Random rng, string current) => current switch
    {
        "Received" or "Triaging" or "WaitingOnBroker" or "WaitingOnDocuments" => WeightedPick(rng,
            ("Withdrawn", 30), ("NotQuoted", 28), ("Lost", 18), ("Declined", 14), ("Expired", 10)),
        "ReadyForUWReview" or "InReview" or "QuotePreparation" => WeightedPick(rng,
            ("Declined", 34), ("NotQuoted", 24), ("Withdrawn", 16), ("Lost", 16), ("Expired", 10)),
        "Quoted" or "RequoteRequested" => WeightedPick(rng,
            ("Lost", 28), ("Withdrawn", 20), ("NotQuoted", 18), ("Declined", 16), ("Bound", 14), ("Expired", 4)),
        "BindRequested" or "Binding" => WeightedPick(rng,
            ("Bound", 56), ("Declined", 14), ("Lost", 12), ("Withdrawn", 10), ("Expired", 8)),
        _ => WeightedPick(rng,
            ("Bound", 36), ("Declined", 20), ("Withdrawn", 16), ("NotQuoted", 14), ("Lost", 10), ("Expired", 4)),
    };

    private static string PickRenewalTerminal(Random rng, string current) => current switch
    {
        "Created" or "Early" or "DataReview" => WeightedPick(rng,
            ("Withdrawn", 28), ("NotRenewed", 24), ("Lost", 16), ("Expired", 12), ("Lapsed", 10), ("Bound", 10)),
        "OutreachStarted" or "WaitingOnBroker" => WeightedPick(rng,
            ("NotRenewed", 26), ("Lost", 22), ("Withdrawn", 18), ("Lapsed", 14), ("Bound", 14), ("Expired", 6)),
        "InReview" or "Quoted" or "Negotiation" => WeightedPick(rng,
            ("Bound", 34), ("Lost", 22), ("NotRenewed", 20), ("Lapsed", 12), ("Withdrawn", 8), ("Expired", 4)),
        "BindRequested" => WeightedPick(rng,
            ("Bound", 62), ("NotRenewed", 14), ("Lost", 10), ("Lapsed", 8), ("Withdrawn", 4), ("Expired", 2)),
        _ => WeightedPick(rng,
            ("Bound", 28), ("NotRenewed", 24), ("Lost", 20), ("Lapsed", 12), ("Withdrawn", 10), ("Expired", 6)),
    };

    private static double RandomStepDays(Random rng, string fromState, string toState)
    {
        if (toState is "WaitingOnBroker" or "WaitingOnDocuments") return rng.Next(2, 14);
        if (toState is "BindRequested" or "Binding") return rng.Next(1, 6);
        if (toState is "Bound" or "Declined" or "Withdrawn" or "Lost" or "NotQuoted" or "NotRenewed" or "Lapsed" or "Expired")
            return rng.Next(1, 10);
        if (fromState == "RequoteRequested") return rng.Next(1, 6);
        return rng.Next(1, 8);
    }

    private static string? BuildTransitionReason(Random rng, string workflowType, string fromState, string toState)
    {
        if (toState == "WaitingOnBroker")
            return WeightedPick(rng, ("Need updated SOV", 35), ("Broker clarification pending", 35), ("Missing loss runs", 30));
        if (toState == "WaitingOnDocuments")
            return WeightedPick(rng, ("Supplemental app requested", 40), ("Loss runs requested", 35), ("Financial statements pending", 25));
        if (toState == "RequoteRequested")
            return WeightedPick(rng, ("Premium target adjustment", 45), ("Limit change request", 30), ("Coverage revision requested", 25));

        if (workflowType == "Submission")
        {
            return toState switch
            {
                "Declined" => WeightedPick(rng, ("Carrier appetite", 35), ("Eligibility", 25), ("Loss history", 20), ("Incomplete information", 20)),
                "Withdrawn" => WeightedPick(rng, ("Broker withdrew", 40), ("Insured withdrew", 35), ("Timing changed", 25)),
                "NotQuoted" => WeightedPick(rng, ("Incomplete submission", 30), ("No broker response", 30), ("Out of appetite", 25), ("Timing expired", 15)),
                "Lost" => WeightedPick(rng, ("Placed elsewhere", 45), ("Price", 30), ("Terms", 15), ("Coverage breadth", 10)),
                "Expired" => WeightedPick(rng, ("No decision before effective date", 60), ("Docs not received", 40)),
                _ => null,
            };
        }

        return toState switch
        {
            "NotRenewed" => WeightedPick(rng, ("Carrier non-renewal", 30), ("Insured declined", 30), ("Program change", 20), ("No response", 20)),
            "Lost" => WeightedPick(rng, ("Lost to competitor", 55), ("Price increase", 25), ("Coverage terms", 20)),
            "Lapsed" => WeightedPick(rng, ("No bind received", 55), ("Late response", 25), ("Payment issue", 20)),
            "Withdrawn" => WeightedPick(rng, ("Insured sold business", 30), ("Broker withdrew", 40), ("Coverage no longer needed", 30)),
            "Expired" => WeightedPick(rng, ("Renewal window elapsed", 70), ("Data collection stalled", 30)),
            _ => null,
        };
    }

    private static async Task EnsureReferenceStatusesAsync(AppDbContext db)
    {
        await UpsertSubmissionReferenceStatusesAsync(db);
        await UpsertRenewalReferenceStatusesAsync(db);
    }

    /// <summary>
    /// Idempotently sets BrokerTenantId on one active broker so the BrokerUser dev identity
    /// (broker001) can resolve their scope. Runs before the early-return so it applies even
    /// when data was seeded before the F0009 migration added the column.
    /// </summary>
    private static async Task EnsureDevBrokerTenantIdAsync(AppDbContext db)
    {
        var alreadyLinked = await db.Brokers
            .AnyAsync(b => b.BrokerTenantId == BrokerUserDevTenantId);
        if (alreadyLinked) return;

        var broker = await db.Brokers.FirstOrDefaultAsync(b => b.Status == "Active")
                     ?? await db.Brokers.FirstOrDefaultAsync();
        if (broker is null) return;

        broker.BrokerTenantId = BrokerUserDevTenantId;
        await db.SaveChangesAsync();
    }

    private static async Task UpsertSubmissionReferenceStatusesAsync(AppDbContext db)
    {
        var existing = await db.ReferenceSubmissionStatuses.ToDictionaryAsync(s => s.Code);
        var desired = OpportunityStatusCatalog.SubmissionStatuses;

        if (existing.Count > 0)
        {
            foreach (var row in existing.Values)
            {
                row.DisplayOrder = (short)(row.DisplayOrder + 100);
            }
            await db.SaveChangesAsync();
        }

        foreach (var status in desired)
        {
            if (!existing.TryGetValue(status.Code, out var row))
            {
                db.ReferenceSubmissionStatuses.Add(new ReferenceSubmissionStatus
                {
                    Code = status.Code,
                    DisplayName = status.DisplayName,
                    Description = status.Description,
                    IsTerminal = status.IsTerminal,
                    DisplayOrder = status.DisplayOrder,
                    ColorGroup = status.ColorGroup,
                });
                continue;
            }

            row.DisplayName = status.DisplayName;
            row.Description = status.Description;
            row.IsTerminal = status.IsTerminal;
            row.DisplayOrder = status.DisplayOrder;
            row.ColorGroup = status.ColorGroup;
        }

        await db.SaveChangesAsync();
    }

    private static async Task UpsertRenewalReferenceStatusesAsync(AppDbContext db)
    {
        var existing = await db.ReferenceRenewalStatuses.ToDictionaryAsync(s => s.Code);
        var desired = OpportunityStatusCatalog.RenewalStatuses;

        if (existing.Count > 0)
        {
            foreach (var row in existing.Values)
            {
                row.DisplayOrder = (short)(row.DisplayOrder + 100);
            }
            await db.SaveChangesAsync();
        }

        foreach (var status in desired)
        {
            if (!existing.TryGetValue(status.Code, out var row))
            {
                db.ReferenceRenewalStatuses.Add(new ReferenceRenewalStatus
                {
                    Code = status.Code,
                    DisplayName = status.DisplayName,
                    Description = status.Description,
                    IsTerminal = status.IsTerminal,
                    DisplayOrder = status.DisplayOrder,
                    ColorGroup = status.ColorGroup,
                });
                continue;
            }

            row.DisplayName = status.DisplayName;
            row.Description = status.Description;
            row.IsTerminal = status.IsTerminal;
            row.DisplayOrder = status.DisplayOrder;
            row.ColorGroup = status.ColorGroup;
        }

        await db.SaveChangesAsync();
    }

    private static T WeightedPick<T>(Random rng, params (T Item, int Weight)[] choices)
    {
        var totalWeight = choices.Sum(c => c.Weight);
        var roll = rng.Next(1, totalWeight + 1);
        var cumulative = 0;
        foreach (var (item, weight) in choices)
        {
            cumulative += weight;
            if (roll <= cumulative) return item;
        }

        return choices[^1].Item;
    }

    private static T Pick<T>(Random rng, IReadOnlyList<T> values) => values[rng.Next(values.Count)];

    private static readonly string[] CompanyPrefixes =
    [
        "Northstar", "Blue Harbor", "Summit", "Granite", "Crescent", "Atlas", "Harbor", "Redwood",
        "Beacon", "Pioneer", "Frontier", "Sterling", "Cobalt", "Highline", "Riverbend", "Oakridge"
    ];

    private static readonly string[] CompanySuffixes =
    [
        "Manufacturing", "Logistics", "Health", "Foods", "Systems", "Transport", "Builders", "Holdings",
        "Services", "Distribution", "Packaging", "Supply", "Retail", "Dynamics", "Materials", "Partners"
    ];

    private static readonly string[] BrokerNamePrefixes =
    [
        "Blue Horizon", "Northline", "Cedar", "Ironwood", "Summit", "Pinnacle", "Harbor", "Compass",
        "Crown", "Meridian", "Anchor", "Evergreen", "Stonegate", "Red River", "Skyline", "Legacy"
    ];

    private static readonly string[] BrokerNameSuffixes =
    [
        "Brokerage", "Risk Partners", "Insurance Group", "Advisors", "Placement Services", "Wholesale", "Markets", "Agency"
    ];

    private static readonly string[] FirstNames =
    [
        "Alex", "Jamie", "Taylor", "Jordan", "Morgan", "Casey", "Riley", "Avery", "Cameron", "Drew",
        "Parker", "Reese", "Quinn", "Sam", "Blake", "Hayden", "Logan", "Kendall", "Rowan", "Sydney"
    ];

    private static readonly string[] LastNames =
    [
        "Anderson", "Brooks", "Carter", "Diaz", "Edwards", "Foster", "Garcia", "Hayes", "Iverson", "Jenkins",
        "Kim", "Lopez", "Mitchell", "Nguyen", "Owens", "Patel", "Reed", "Sanchez", "Turner", "Young"
    ];
}
