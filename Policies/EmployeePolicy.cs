namespace RequisitionSystem.Policies;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


public class EmployeePolicy : IAuthorizationRequirement
{
  public string RequiredRole { get; } = "employee";
}

public class EmployeePolicyHandler : AuthorizationHandler<EmployeePolicy>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      EmployeePolicy requirement)
  {
    // Check if user has the employee role
    if (context.User.HasClaim(ClaimTypes.Role, requirement.RequiredRole))
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}

