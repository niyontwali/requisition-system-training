using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.Models;

public class Requisition : BaseEntity
{
    public required Guid RequestedUserId { get; set; }
    public User? RequestedUser { get; set; }

    [RegularExpression("^(Pending|Approved|Rejected|NeedsModification)$", ErrorMessage = "Invalid status. You must provide either Pending, Approved, Rejected or NeedsModification ")]
    public required string Status { get; set; }

    [StringLength(500, ErrorMessage = "Description name should not exceed 500 characters")]
    public required string Description { get; set; }

    public ICollection<RequisitionRemark> RequisitionRemarks { get; set; } = [];
    public ICollection<RequisitionItem> RequisitionItems { get; set; } = [];
}