using FluentValidation;
using Nebula.Api.Helpers;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class RenewalEndpoints
{
    public static IEndpointRouteBuilder MapRenewalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/renewals")
            .WithTags("Renewals")
            .RequireAuthorization();

        group.MapGet("/{renewalId:guid}", GetRenewal);
        group.MapGet("/{renewalId:guid}/transitions", GetTransitions);
        group.MapPost("/{renewalId:guid}/transitions", PostTransition);

        return app;
    }

    private static async Task<IResult> GetRenewal(
        Guid renewalId, RenewalService svc, CancellationToken ct)
    {
        var result = await svc.GetByIdAsync(renewalId, ct);
        return result is null ? ProblemDetailsHelper.NotFound("Renewal", renewalId) : Results.Ok(result);
    }

    private static async Task<IResult> GetTransitions(
        Guid renewalId, RenewalService svc, CancellationToken ct) =>
        Results.Ok(await svc.GetTransitionsAsync(renewalId, ct));

    private static async Task<IResult> PostTransition(
        Guid renewalId,
        WorkflowTransitionRequestDto dto,
        IValidator<WorkflowTransitionRequestDto> validator,
        RenewalService svc,
        ICurrentUserService user,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var (result, error) = await svc.TransitionAsync(renewalId, dto, user, ct);
        return error switch
        {
            "not_found" => ProblemDetailsHelper.NotFound("Renewal", renewalId),
            "invalid_transition" => ProblemDetailsHelper.InvalidTransition("current", dto.ToState),
            _ => Results.Created($"/renewals/{renewalId}/transitions", result),
        };
    }
}
