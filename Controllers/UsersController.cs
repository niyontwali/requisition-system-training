namespace RequisitionSystem.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;

/*****************************************************************************
 * USER CONTROLLER
 * Handles user-related operations (currently read-only)
 * All endpoints require Admin privileges
 ****************************************************************************/
[ApiController]
[Route("api/users")]
public class UserController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    /*************************************************************************
     * GET ALL USERS - GET api/users
     * Retrieves complete list of all users with their role information
     * Access: Admin only
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dbContext.Users
            .Include(user => user.Role)
            .ToListAsync();

        return Ok(new { ok = true, data = users });
    }

    /*************************************************************************
     * GET SINGLE USER - GET api/users/{id}
     * Retrieves details for a specific user by ID including role information
     * Access: Admin only
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        /*********************************************************************
         * STEP 1: Find user by ID with role information
         ********************************************************************/
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        /*********************************************************************
         * STEP 2: Validate user existence
         ********************************************************************/
        if (user is null)
        {
            return NotFound(new { ok = false, message = "User not found" });
        }

        /*********************************************************************
         * STEP 3: Return user data
         ********************************************************************/
        return Ok(new { ok = true, data = user });
    }
}