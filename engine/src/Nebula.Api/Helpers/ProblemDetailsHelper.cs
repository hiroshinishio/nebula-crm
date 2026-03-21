using System.Diagnostics;

namespace Nebula.Api.Helpers;

public static class ProblemDetailsHelper
{
    public static IResult DuplicateLicense() => Results.Problem(
        title: "Duplicate license number",
        detail: "A broker with the given license number already exists.",
        statusCode: 409,
        extensions: Ext("duplicate_license"));

    public static IResult ActiveDependenciesExist() => Results.Problem(
        title: "Active submissions or renewals exist",
        detail: "Cannot deactivate a broker with active (non-terminal) submissions or renewals.",
        statusCode: 409,
        extensions: Ext("active_dependencies_exist"));

    public static IResult AlreadyActive() => Results.Problem(
        title: "Broker is already active",
        detail: "The broker is currently Active and cannot be reactivated.",
        statusCode: 409,
        extensions: Ext("already_active"));

    public static IResult InvalidTransition(string from, string to) => Results.Problem(
        title: "Invalid workflow transition",
        detail: $"Transition from '{from}' to '{to}' is not allowed.",
        statusCode: 409,
        extensions: Ext("invalid_transition"));

    public static IResult InvalidStatusTransition(string from, string to) => Results.Problem(
        title: "Invalid status transition",
        detail: $"Transition from '{from}' to '{to}' is not allowed.",
        statusCode: 409,
        extensions: Ext("invalid_status_transition"));

    public static IResult ConcurrencyConflict() => Results.Problem(
        title: "Concurrency conflict",
        detail: "The resource was modified by another user. Please refresh and retry.",
        statusCode: 409,
        extensions: Ext("concurrency_conflict"));

    public static IResult NotFound(string resource, Guid id) => Results.Problem(
        title: $"{resource} not found",
        detail: $"{resource} with ID {id} does not exist.",
        statusCode: 404,
        extensions: Ext("not_found"));

    public static IResult Forbidden() => Results.Problem(
        title: "Forbidden",
        detail: "You do not have permission to perform this action.",
        statusCode: 403,
        extensions: Ext("forbidden"));

    public static IResult ValidationError(IDictionary<string, string[]> errors) => Results.Problem(
        title: "Validation error",
        detail: "One or more validation errors occurred.",
        statusCode: 400,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = "validation_error",
            ["errors"] = errors,
            ["traceId"] = Activity.Current?.Id,
        });

    private static Dictionary<string, object?> Ext(string code) => new()
    {
        ["code"] = code,
        ["traceId"] = Activity.Current?.Id,
    };
}
