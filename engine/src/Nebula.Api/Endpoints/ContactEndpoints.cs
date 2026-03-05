using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nebula.Api.Helpers;
using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Services;

namespace Nebula.Api.Endpoints;

public static class ContactEndpoints
{
    public static RouteGroupBuilder MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/contacts")
            .WithTags("Contacts")
            .RequireAuthorization();

        group.MapGet("/", ListContacts);
        group.MapPost("/", CreateContact);
        group.MapGet("/{contactId:guid}", GetContact);
        group.MapPut("/{contactId:guid}", UpdateContact);
        group.MapDelete("/{contactId:guid}", DeleteContact);

        return group;
    }

    private static async Task<IResult> ListContacts(
        Guid? brokerId, int? page, int? pageSize,
        ContactService svc, ICurrentUserService user, CancellationToken ct)
    {
        var result = await svc.ListAsync(brokerId, page ?? 1, pageSize ?? 20, user, ct);
        return Results.Ok(new { data = result.Data, page = result.Page, pageSize = result.PageSize, totalCount = result.TotalCount, totalPages = result.TotalPages });
    }

    private static async Task<IResult> CreateContact(
        ContactCreateDto dto,
        IValidator<ContactCreateDto> validator,
        ContactService svc,
        ICurrentUserService user,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var (result, error) = await svc.CreateAsync(dto, user, ct);
        return error switch
        {
            "not_found" => ProblemDetailsHelper.NotFound("Broker", dto.BrokerId),
            _ => Results.Created($"/contacts/{result!.Id}", result),
        };
    }

    private static async Task<IResult> GetContact(
        Guid contactId, ContactService svc, CancellationToken ct)
    {
        var result = await svc.GetByIdAsync(contactId, ct);
        return result is null ? ProblemDetailsHelper.NotFound("Contact", contactId) : Results.Ok(result);
    }

    private static async Task<IResult> UpdateContact(
        Guid contactId,
        ContactUpdateDto dto,
        IValidator<ContactUpdateDto> validator,
        ContactService svc,
        ICurrentUserService user,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ProblemDetailsHelper.ValidationError(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var ifMatch = httpContext.Request.Headers.IfMatch.FirstOrDefault();
        if (string.IsNullOrEmpty(ifMatch) || !uint.TryParse(ifMatch.Trim('"'), out var rowVersion))
            return Results.Problem(title: "If-Match header required", statusCode: 428);

        try
        {
            var (result, error) = await svc.UpdateAsync(contactId, dto, rowVersion, user, ct);
            return error switch
            {
                "not_found" => ProblemDetailsHelper.NotFound("Contact", contactId),
                _ => Results.Ok(result),
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return ProblemDetailsHelper.ConcurrencyConflict();
        }
    }

    private static async Task<IResult> DeleteContact(
        Guid contactId, ContactService svc, ICurrentUserService user, CancellationToken ct)
    {
        var error = await svc.DeleteAsync(contactId, user, ct);
        return error switch
        {
            "not_found" => ProblemDetailsHelper.NotFound("Contact", contactId),
            _ => Results.NoContent(),
        };
    }
}
