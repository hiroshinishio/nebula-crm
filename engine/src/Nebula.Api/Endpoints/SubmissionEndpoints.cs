using FluentValidation;
using Nebula.Api.Helpers;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class SubmissionEndpoints
{
    public static IEndpointRouteBuilder MapSubmissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/submissions")
            .WithTags("Submissions")
            .RequireAuthorization();

        group.MapGet("/{submissionId:guid}", GetSubmission);
        group.MapGet("/{submissionId:guid}/transitions", GetTransitions);
        group.MapPost("/{submissionId:guid}/transitions", PostTransition);

        return app;
    }

    private static async Task<IResult> GetSubmission(
        Guid submissionId, SubmissionService svc, CancellationToken ct)
    {
        var result = await svc.GetByIdAsync(submissionId, ct);
        return result is null ? ProblemDetailsHelper.NotFound("Submission", submissionId) : Results.Ok(result);
    }

    private static async Task<IResult> GetTransitions(
        Guid submissionId, SubmissionService svc, CancellationToken ct) =>
        Results.Ok(await svc.GetTransitionsAsync(submissionId, ct));

    private static async Task<IResult> PostTransition(
        Guid submissionId,
        WorkflowTransitionRequestDto dto,
        IValidator<WorkflowTransitionRequestDto> validator,
        SubmissionService svc,
        ICurrentUserService user,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var (result, error) = await svc.TransitionAsync(submissionId, dto, user, ct);
        return error switch
        {
            "not_found" => ProblemDetailsHelper.NotFound("Submission", submissionId),
            "invalid_transition" => ProblemDetailsHelper.InvalidTransition("current", dto.ToState),
            _ => Results.Created($"/submissions/{submissionId}/transitions", result),
        };
    }
}
