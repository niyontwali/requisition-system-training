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
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    private readonly IConfiguration _config;


    // constructor
    public AuthController(ApplicationDbContext dbContext, IConfiguration config)
    {
        // assign the _dbContent
        _dbContext = dbContext;
        _passwordHasher = new PasswordHasher<User>();
        _config = config;
    }

    // register method - api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        // step 1 - validate if the user already exists
        bool userExists = await _dbContext.Users.AnyAsync(user => user.Email == request.Email);

        if (userExists)
        {
            return BadRequest(new { ok = false, message = "User alread exists" });
        }
        // step 2: - Validate the role given the user if it exists
        bool roleExists = await _dbContext.Roles.AnyAsync(role => role.Id == request.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { ok = false, message = "Role does not exists" });
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

        // step 7: Return a meaningfull message
        return StatusCode(201, new { ok = true, message = "User registered successfully" });
    }

    // login method
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        // 1. Check the user email login in if it exists
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == request.Email);

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

        return Ok(new { access_token });
    }

    private string GenerateToken(User user)
    {
        // step 1: Payload to be store in the token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,  user.FullName),
        };

        // Step 2: Retrieving the secret key from app setttings
        var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!)
            );
        // step 3: Configuring our token's encryption
        var creds = new SigningCredentials(key, SecurityAlgorithms.Aes256CbcHmacSha512);

        // step 4: Assigning a token to the user
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