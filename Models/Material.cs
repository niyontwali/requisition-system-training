
using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.Models;

public class Material : BaseEntity
{
    [StringLength(200, ErrorMessage = "Material name should not exceed 200 characters")]
    public required string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description name should not exceed 500 characters")]
    public string? Description { get; set; }

    public required string Unit { get; set; }
}

