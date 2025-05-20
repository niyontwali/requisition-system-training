using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.DTOs;

public class LoginDto 
{
    [EmailAddress(ErrorMessage ="Invalid email")]
    public required string Email { get; set; }

    [MinLength(8, ErrorMessage ="Your password should be at least 8 character")]
    public required string Password { get; set; }

}