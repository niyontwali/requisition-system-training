namespace RequisitionSystem.DTOs;

public class RequisitionItemDto
{
    public required Guid MaterialId { get; set; }
    public required int Quantity { get; set; }
}