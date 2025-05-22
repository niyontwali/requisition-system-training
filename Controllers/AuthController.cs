using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

namespace RequisitionSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ApplicationDbContext dbContext, IConfiguration config) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
    private readonly IConfiguration _config = config;

  // register method - api/auth/register
  [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        // step 1 - validate if the user already exists
        bool userExists = await _dbContext.Users.AnyAsync(user => user.Email == request.Email);

        if (userExists)
        {
            return BadRequest(new { ok = false, message = "User already exists" });
        }
        // step 2: - Validate the role given the user if it exists
        bool roleExists = await _dbContext.Roles.AnyAsync(role => role.Id == request.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { ok = false, message = "Role does not exist" });
        }

        // step 3 - Creating our user
        var newUser = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            RoleId = request.RoleId,
        };

        // Step 4:  append the password to the user
        newUser.Password = _passwordHasher.HashPassword(newUser, request.Password);

        // Step 5:  add the new user to the context
        await _dbContext.Users.AddAsync(newUser);

        // Step 6: Save the changes 
        await _dbContext.SaveChangesAsync();

        // step 7: Return a meaningful message
        return StatusCode(201, new { ok = true, message = "User registered successfully" });
    }

    // login method
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        // 1. Check the user email login in if it exists
        var user = await _dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(user => user.Email == request.Email);

        if (user is null)
        {
            return Unauthorized(new { ok = false, message = "Invalid Credentials" });
        }

        // 2. Validate Password
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { ok = false, message = "Invalid Credentials" });
        }

        // step 3: Generate Token 
        string access_token = GenerateToken(user);

        return Ok(new { ok = true, access_token });
    }

    private string GenerateToken(User user)
    {
        // step 1: Payload to be stored in the token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role!.Name),

        };

        // Step 2: Retrieve the secret key from app settings
        var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!)
            );

        // Step 3: Configure token's signing credentials - Use HMACSHA256 instead of Aes256CbcHmacSha512
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // step 4: Assign a token to the user
        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"])),
            signingCredentials: creds
        );

        // step 5: Return the token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}