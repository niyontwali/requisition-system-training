namespace RequisitionSystem.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

[ApiController]
[Route("api/users")]
public class UserController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    [Authorize]
    // Get Users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dbContext.Users
            .Include(user => user.Role)
            .ToListAsync();
        return Ok(new { ok = true, data = users });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

        if (user is null) return NotFound(new { ok = false, message = "User not found" });
   

        return NotFound(new { ok = true, data = user });
    }

}