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

/*****************************************************************************
 * AUTHENTICATION CONTROLLER
 * Handles user registration and authentication processes
 ****************************************************************************/
[ApiController]
[Route("api/auth")]
public class AuthController(ApplicationDbContext dbContext, IConfiguration config) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
    private readonly IConfiguration _config = config;

    /*************************************************************************
     * REGISTER ENDPOINT - POST api/auth/register
     * Creates a new user account with the provided credentials
     ************************************************************************/
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        /*********************************************************************
         * STEP 1: Validate if user already exists
         ********************************************************************/
        bool userExists = await _dbContext.Users.AnyAsync(user => user.Email == request.Email);
        if (userExists)
        {
            return BadRequest(new { ok = false, message = "User already exists" });
        }

        /*********************************************************************
         * STEP 2: Validate if the specified role exists
         ********************************************************************/
        bool roleExists = await _dbContext.Roles.AnyAsync(role => role.Id == request.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { ok = false, message = "Role does not exist" });
        }

        /*********************************************************************
         * STEP 3: Create new user object
         ********************************************************************/
        var newUser = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            RoleId = request.RoleId,
        };

        /*********************************************************************
         * STEP 4: Hash and append password to user
         ********************************************************************/
        newUser.Password = _passwordHasher.HashPassword(newUser, request.Password);

        /*********************************************************************
         * STEP 5: Add new user to database context
         ********************************************************************/
        await _dbContext.Users.AddAsync(newUser);

        /*********************************************************************
         * STEP 6: Save changes to database
         ********************************************************************/
        await _dbContext.SaveChangesAsync();

        /*********************************************************************
         * STEP 7: Return success response
         ********************************************************************/
        return StatusCode(201, new { ok = true, message = "User registered successfully" });
    }

    /*************************************************************************
     * LOGIN ENDPOINT - POST api/auth/login
     * Authenticates user and returns JWT token if successful
     ************************************************************************/
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        /*********************************************************************
         * STEP 1: Check if user exists by email
         ********************************************************************/
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(user => user.Email == request.Email);

        if (user is null)
        {
            return Unauthorized(new { ok = false, message = "Invalid Credentials" });
        }

        /*********************************************************************
         * STEP 2: Verify provided password against hashed password
         ********************************************************************/
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { ok = false, message = "Invalid Credentials" });
        }

        /*********************************************************************
         * STEP 3: Generate and return JWT token
         ********************************************************************/
        string access_token = GenerateToken(user);
        return Ok(new { ok = true, access_token });
    }

    /*************************************************************************
     * TOKEN GENERATION METHOD
     * Creates a JWT token with user claims and signing credentials
     ************************************************************************/
    private string GenerateToken(User user)
    {
        /*********************************************************************
         * STEP 1: Create token payload with user claims
         ********************************************************************/
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role!.Name),
        };

        /*********************************************************************
         * STEP 2: Retrieve and configure secret key from app settings
         ********************************************************************/
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!)
        );

        /*********************************************************************
         * STEP 3: Configure token signing credentials
         ********************************************************************/
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        /*********************************************************************
         * STEP 4: Create JWT token with all parameters
         ********************************************************************/
        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"])),
            signingCredentials: creds
        );

        /*********************************************************************
         * STEP 5: Serialize and return the token
         ********************************************************************/
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}