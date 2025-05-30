namespace RequisitionSystem.Policies;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

/*****************************************************************************
 * ADMIN POLICY REQUIREMENT
 * Defines the authorization requirement for admin-level access
 * Requires users to have the "admin" role claim
 ****************************************************************************/
public class AdminPolicy : IAuthorizationRequirement
{
  public string RequiredRole { get; } = "admin";
}

/*****************************************************************************
 * ADMIN POLICY HANDLER
 * Implements the authorization logic for the AdminPolicy requirement
 * Validates whether the current user has the required admin role
 ****************************************************************************/
public class AdminPolicyHandler : AuthorizationHandler<AdminPolicy>
{
  /*************************************************************************
   * HANDLE REQUIREMENT
   * Determines if the current user meets the admin policy requirements
   * 
   * Parameters:
   *   context - The authorization context containing user information
   *   requirement - The admin policy requirement being evaluated
   * 
   * Returns: Task.CompletedTask
   ************************************************************************/
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      AdminPolicy requirement)
  {
    /*********************************************************************
     * STEP 1: Verify user has the required admin role claim
     ********************************************************************/
    if (context.User.HasClaim(ClaimTypes.Role, requirement.RequiredRole))
    {
      /*****************************************************************
       * STEP 2: Mark requirement as satisfied if role exists
       ****************************************************************/
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}