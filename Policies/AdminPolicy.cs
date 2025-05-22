namespace RequisitionSystem.Policies;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class AdminPolicy : IAuthorizationRequirement
{
  public string RequiredRole { get; } = "admin";
}

public class AdminPolicyHandler : AuthorizationHandler<AdminPolicy>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      AdminPolicy requirement)
  {
    // Check if user has the admin role
    if (context.User.HasClaim(ClaimTypes.Role, requirement.RequiredRole))
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}