using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.DTOs;

public class UpdateRequisitionDto
{
    [StringLength(500, ErrorMessage = "Description name should not exceed 500 characters")]
    public string? Description { get; set; }

    [RegularExpression("^(Pending|Approved|Rejected|NeedsModification)$", ErrorMessage = "Invalid status. You must provide either Pending, Approved, Rejected or NeedsModification")]
    public string? Status { get; set; }

    public List<RequisitionItemDto>? Items { get; set; }
}

public class UpdateRequisitionStatusDto
{
    [Required]
    [RegularExpression("^(Pending|Approved|Rejected|NeedsModification)$", ErrorMessage = "Invalid status. You must provide either Pending, Approved, Rejected or NeedsModification")]
    public required string Status { get; set; }
}