using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

namespace RequisitionSystem.Controllers;

/*****************************************************************************
 * ROLES CONTROLLER
 * Handles all role management operations (CRUD) for the system
 * All endpoints require Admin privileges
 ****************************************************************************/
[ApiController]
[Route("api/roles")]
public class RolesController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    /*************************************************************************
     * GET ALL ROLES - GET api/roles
     * Retrieves complete list of all roles in the system
     * Access: Admin only
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _dbContext.Roles.ToListAsync();
        return Ok(new { ok = true, data = roles });
    }

    /*************************************************************************
     * GET SINGLE ROLE - GET api/roles/{id}
     * Retrieves details for a specific role by ID
     * Access: Admin only
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        /*********************************************************************
         * STEP 1: Find role by ID
         ********************************************************************/
        var role = await _dbContext.Roles.FindAsync(id);

        /*********************************************************************
         * STEP 2: Validate role existence
         ********************************************************************/
        if (role is null)
        {
            return NotFound(new { ok = false, message = "Role not found" });
        }

        /*********************************************************************
         * STEP 3: Return role data
         ********************************************************************/
        return Ok(new { ok = true, data = role });
    }

    /*************************************************************************
     * CREATE ROLE - POST api/roles
     * Creates a new role in the system
     * Access: Admin only
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleDto roleDto)
    {
        /*********************************************************************
         * STEP 1: Create new role object
         ********************************************************************/
        var role = new Role
        {
            Name = roleDto.Name,
            Description = roleDto.Description
        };

        /*********************************************************************
         * STEP 2: Add role to database context
         ********************************************************************/
        _dbContext.Roles.Add(role);

        /*********************************************************************
         * STEP 3: Save changes to database
         ********************************************************************/
        await _dbContext.SaveChangesAsync();

        /*********************************************************************
         * STEP 4: Return success response
         ********************************************************************/
        return StatusCode(201, new { ok = true, message = "Role created successfully" });
    }

    /*************************************************************************
     * UPDATE ROLE - PUT api/roles/{id}
     * Modifies an existing role's details
     * Access: Admin only
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleDto payload)
    {
        /*********************************************************************
         * STEP 1: Find role by ID
         ********************************************************************/
        var role = await _dbContext.Roles.FindAsync(id);

        /*********************************************************************
         * STEP 2: Validate role existence
         ********************************************************************/
        if (role is null)
        {
            return NotFound(new { ok = false, message = $"Role with id: {id} not found" });
        }

        /*********************************************************************
         * STEP 3: Apply partial updates to role
         ********************************************************************/
        if (!string.IsNullOrWhiteSpace(payload.Name))
        {
            role.Name = payload.Name;
        }

        if (!string.IsNullOrWhiteSpace(payload.Description))
        {
            role.Description = payload.Description;
        }

        /*********************************************************************
         * STEP 4: Update timestamp
         ********************************************************************/
        role.UpdatedAt = DateTime.UtcNow;

        /*********************************************************************
         * STEP 5: Save changes to database
         ********************************************************************/
        await _dbContext.SaveChangesAsync();

        /*********************************************************************
         * STEP 6: Return success response
         ********************************************************************/
        return Ok(new { ok = true, message = "Role updated successfully" });
    }

    /*************************************************************************
     * DELETE ROLE - DELETE api/roles/{id}
     * Removes a role from the system
     * Access: Admin only
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        /*********************************************************************
         * STEP 1: Find role by ID
         ********************************************************************/
        var role = await _dbContext.Roles.FindAsync(id);

        /*********************************************************************
         * STEP 2: Validate role existence
         ********************************************************************/
        if (role is null)
        {
            return NotFound(new { ok = false, message = $"Role with id: {id} not found" });
        }

        /*********************************************************************
         * STEP 3: Remove role from database context
         ********************************************************************/
        _dbContext.Roles.Remove(role);

        /*********************************************************************
         * STEP 4: Save changes to database
         ********************************************************************/
        await _dbContext.SaveChangesAsync();

        /*********************************************************************
         * STEP 5: Return success response
         ********************************************************************/
        return Ok(new { ok = true, message = "Role deleted successfully" });
    }
}