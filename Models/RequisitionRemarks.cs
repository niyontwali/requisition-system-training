
using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.Models;

public class RequisitionRemark : BaseEntity
{
    public required Guid RequisitionId { get; set; }

    [StringLength(500, MinimumLength = 20, ErrorMessage = "Remarks must be between 20 characters and 500 characters")]
    public required string Content { get; set; }

    public required Guid AuthorId { get; set; }
}

