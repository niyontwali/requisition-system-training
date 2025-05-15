using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

namespace RequisitionSystem.Controllers;

[ApiController]
[Route("api/roles")] // controller base route i.e api/Roles
public class RolesController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    // All methods for CRUD Operation
    // get roles
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _dbContext.Roles.ToListAsync();

        /*
        Ok, NotFound
        */
        return Ok(new { ok = true, data = roles }); // ok == status code of 200
    }

    // get a single role by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole (Guid id) {
        // find role with that id
        var role = await _dbContext.Roles.FindAsync(id);

        // check if the role is found
        if (role is null) {
            return NotFound(new {ok=false, message="Role not found"});
        }

        // return response
        return Ok(new {ok=true, data=role});
    }

    // create role
    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleDto roleDto)
    {
        var role = new Role
        {
            Name = roleDto.Name,
            Description = roleDto.Description
        };

        // add the new object to the db context
        _dbContext.Roles.Add(role);

        // save the changes
        await _dbContext.SaveChangesAsync();

        // return a response
        return StatusCode(201, new { ok = true, message = "Role created successfully" });
    }

    // update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleDto payload)
    {
        // find a role with the params id
        var role = await _dbContext.Roles.FindAsync(id);
        if (role is null)
        {
            return NotFound(new {ok=false, message=$"Role with id: {id} not found"}); // NotFound == status code of 404
        }
        
        // validate the payload with dto
        if (!string.IsNullOrWhiteSpace(payload.Name)) {
            role.Name = payload.Name;
        }
        if (!string.IsNullOrWhiteSpace(payload.Description)) {
            role.Description = payload.Description;
        }
        // update the time in the database - ef and sql do not handle this by default
        role.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new {ok=true, message="Role updated successfully"});
    }

    // delete role
    [HttpDelete("{id}")] // HTTP Method 
    // IActionResult is an ASP.NET Interface that allows or enable us to return our responses with status
    public async Task<IActionResult> DeleteRole (Guid id) {
        // find the role to delete
        var role = await _dbContext.Roles.FindAsync(id);

        // condition: check if the role was found
        if (role is null) {
            return NotFound(new {ok = false, message=$"Role with id: ${id} not found" });
        }

        // delete the role from your db
        _dbContext.Roles.Remove(role);

        // save the delete changes
        await _dbContext.SaveChangesAsync();

        return Ok(new {ok=true, message="Role deleted successfully"});
    }

}



// IActionResult, this is the return type that allows you to use methods like
// - OK -> 200
// - NotFound -> 404
// - StatusCode - > here you specify the status code
// - Unauthorized -> 401
// - Created -> 201 (but this takes in two arguments)
// - NoContent -> 204
// - BadRequest -> 400
// - Forbid - 403