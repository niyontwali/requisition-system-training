using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.DTOs;

public class CreateRequisitionDt0
{
    public required Guid RequestedUserId { get; set; }

    public string Status { get; set; } = "Pending";

    [StringLength(500, ErrorMessage = "Description name should not exceed 500 characters")]
    public required string Description { get; set; }

    public List<RequisitionItemDto>? Items {get; set;}
}