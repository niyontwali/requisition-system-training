
using System.ComponentModel.DataAnnotations;

namespace RequisitionSystem.DTOs;

public class UpdateMaterialDto
{
    [StringLength(200, ErrorMessage = "Material name should not exceed 200 characters")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "Description name should not exceed 500 characters")]
    public string? Description { get; set; }

    public string? Unit { get; set; }
}



