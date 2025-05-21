namespace RequisitionSystem.Models;

public class RequisitionItem : BaseEntity
{
    public required Guid RequisitionId { get; set; }
    public required Guid MaterialId { get; set; }
    public Material? Material { get; set; }
    public required int Quantity { get; set; }
}