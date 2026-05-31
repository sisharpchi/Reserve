using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ReserveFlow.Common.Application.Abstractions;

namespace ReserveFlow.Common.Presentation.Endpoints;

public sealed class TenantAccessEndpointFilter(ITenantAccessGuard tenantAccessGuard) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (!TryResolveTenantId(context, out Guid tenantId))
        {
            return TypedResults.Problem("Tenant id is required for this endpoint.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (!tenantAccessGuard.CanAccessTenant(tenantId))
        {
            return TypedResults.Forbid();
        }

        return await next(context);
    }

    private static bool TryResolveTenantId(EndpointFilterInvocationContext context, out Guid tenantId)
    {
        if (TryResolveFromRoute(context.HttpContext, out tenantId) ||
            TryResolveFromQuery(context.HttpContext, out tenantId))
        {
            return true;
        }

        foreach (object? argument in context.Arguments)
        {
            if (TryResolveFromArgument(argument, out tenantId))
            {
                return true;
            }
        }

        tenantId = Guid.Empty;
        return false;
    }

    private static bool TryResolveFromRoute(HttpContext httpContext, out Guid tenantId)
    {
        if (!httpContext.Request.RouteValues.TryGetValue("tenantId", out object? value))
        {
            tenantId = Guid.Empty;
            return false;
        }

        return TryParseTenantId(value, out tenantId);
    }

    private static bool TryResolveFromQuery(HttpContext httpContext, out Guid tenantId)
    {
        if (!httpContext.Request.Query.TryGetValue("tenantId", out StringValues values))
        {
            tenantId = Guid.Empty;
            return false;
        }

        return TryParseTenantId(values.FirstOrDefault(), out tenantId);
    }

    private static bool TryResolveFromArgument(object? argument, out Guid tenantId)
    {
        if (argument is null)
        {
            tenantId = Guid.Empty;
            return false;
        }

        if (argument is Guid guid)
        {
            tenantId = guid;
            return true;
        }

        PropertyInfo? tenantIdProperty = argument
            .GetType()
            .GetProperty("TenantId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (tenantIdProperty is null)
        {
            tenantId = Guid.Empty;
            return false;
        }

        return TryParseTenantId(tenantIdProperty.GetValue(argument), out tenantId);
    }

    private static bool TryParseTenantId(object? value, out Guid tenantId)
    {
        if (value is Guid guid)
        {
            tenantId = guid;
            return true;
        }

        string? stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);

        return Guid.TryParse(stringValue, out tenantId);
    }
}

public static class TenantAccessEndpointFilterExtensions
{
    public static RouteHandlerBuilder RequireTenantAccess(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<TenantAccessEndpointFilter>();
    }
}
