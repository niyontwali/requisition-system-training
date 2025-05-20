using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.DTOs;

public class LoginDto 
{
    [EmailAddress(ErrorMessage ="Invalid email")]
    public required string Email { get; set; }
    
    public required string Password { get; set; }

}