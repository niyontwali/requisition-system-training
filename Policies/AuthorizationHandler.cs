namespace RequisitionSystem.Policies;

using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

/*****************************************************************************
 * CUSTOM AUTHORIZATION MIDDLEWARE RESULT HANDLER
 * Provides centralized handling of authorization results with consistent
 * error responses for both authentication and authorization failures
 ****************************************************************************/
public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    /*************************************************************************
     * HANDLE AUTHORIZATION RESULT
     * Processes authorization results and generates appropriate responses
     * 
     * Parameters:
     *   next          - The next middleware in the pipeline
     *   context       - The current HTTP context
     *   policy        - The authorization policy being evaluated
     *   authorizeResult - The result of the authorization check
     ************************************************************************/
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        /*********************************************************************
         * STEP 1: Handle successful authorization
         ********************************************************************/
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        /*********************************************************************
         * STEP 2: Determine failure type (authentication vs authorization)
         ********************************************************************/
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        
        /*********************************************************************
         * STEP 3: Prepare appropriate error message
         ********************************************************************/
        var message = isAuthenticated
            ? "Forbidden: You don't have permission to access this resource"
            : "Unauthorized: Please provide a valid token";

        /*********************************************************************
         * STEP 4: Configure and send error response
         ********************************************************************/
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            ok = false,
            message,
        }));
    }
}