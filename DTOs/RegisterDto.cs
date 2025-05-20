using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.DTOs;

public class RegisterDto
{
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 255 characters")]
    public required string FullName { get; set; }

    [EmailAddress(ErrorMessage ="Invalid email")]
    public required string Email { get; set; }

    [MinLength(8, ErrorMessage ="Your password should be at least 8 character")]
    public required string Password { get; set; }

   public required Guid RoleId {get;set;} = Guid.NewGuid();
}