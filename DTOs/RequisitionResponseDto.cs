namespace RequisitionSystem.DTOs;

public class RequisitionResponseDto
{
  public Guid Id { get; set; }
  public UserBasicDto? RequestedUser { get; set; }
  public string Status { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public List<RequisitionItemResponseDto> RequisitionItems { get; set; } = [];
  public List<RequisitionRemarkResponseDto> RequisitionRemarks { get; set; } = [];

}

public class UserBasicDto
{
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
}

public class RequisitionItemResponseDto
{
  public MaterialDetailDto? Material { get; set; }
  public int Quantity { get; set; }
}

public class MaterialDetailDto
{
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Unit { get; set; } = string.Empty;
}

public class RequisitionRemarkResponseDto
{
  public string Content { get; set; } = string.Empty;
  public Guid AuthorId { get; set; }
  public UserBasicDto? Author { get; set; }
}