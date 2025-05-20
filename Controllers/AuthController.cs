using Microsoft.AspNetCore.Mvc;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;

namespace RequisitionSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    // constructor
    public AuthController (ApplicationDbContext dbContext ): base(dbContext) {

    }

    // register method
    [HttpPost]
    public async Task<IActionResult> Register (RegisterDto registerDto) 
    {
        // create your register logic
        return Ok();
    }

    // login method
    [HttpPost]
    public async Task<IActionResult> Login (RegisterDto registerDto) 
    {
        // create your login logic
        return Ok();
    }
}