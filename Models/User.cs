using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RequisitionSystem.Models;

public class User : BaseEntity
{
    public required Guid RoleId { get; set; } = Guid.NewGuid();

    // specify that a user has a role for easy querying using EF
    // Used on one-to-one or many-to-one relations
    public Role? Role { get; set; }

    [StringLength(255, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 255 characters")]
    public required string FullName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email")]
    public required string Email { get; set; }

    [JsonIgnore]
    [MinLength(8, ErrorMessage = "Your password should be at least 8 character")]
    public string Password { get; set; } = string.Empty;

    public ICollection<Requisition> Requisitions { get; set; } = [];

}