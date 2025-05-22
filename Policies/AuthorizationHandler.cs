
namespace RequisitionSystem.Policies;

using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
  public async Task HandleAsync(
      RequestDelegate next,
      HttpContext context,
      AuthorizationPolicy policy,
      PolicyAuthorizationResult authorizeResult)
  {
    if (authorizeResult.Succeeded)
    {
      await next(context);
      return;
    }

    // Handle all authorization failures in one place
    var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
    
    var message = isAuthenticated
        ? "Forbidden: You don't have permission to access this resource"
        : "Unauthorized: Please provide a valid token";

   
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
      ok = false,
      message,
    }));
  }
}