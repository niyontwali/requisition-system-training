namespace RequisitionSystem.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

[ApiController]
[Route("api/requisitions")]
public class RequisitionsController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;


    // Get All Requisitions
    [HttpGet]
    public async Task<IActionResult> GetRequisitions()
    {
        var requisitions = await _dbContext.Requisitions
            .Include(r => r.RequestedUser)
            .Include(r => r.RequisitionItems)
                .ThenInclude(ri => ri.Material)
            .Include(r => r.RequisitionRemarks)
                .ThenInclude(rr => rr.Author)
            .Select(r => new RequisitionResponseDto
            {
                Id = r.Id,
                RequestedUser = new UserBasicDto
                {
                    Name = r.RequestedUser!.FullName,
                    Email = r.RequestedUser.Email,
                },
                Status = r.Status,
                Description = r.Description,
                RequisitionItems = r.RequisitionItems.Select(ri => new RequisitionItemResponseDto
                {
                    Material = ri.Material != null ? new MaterialDetailDto
                    {
                        Name = ri.Material.Name,
                        Description = ri.Material.Description ?? "No description available",
                        Unit = ri.Material.Unit,
                    } : null,
                    Quantity = ri.Quantity,
                }).ToList(),
                RequisitionRemarks = r.RequisitionRemarks.Select(rr => new RequisitionRemarkResponseDto
                {
                    Content = rr.Content,
                    AuthorId = rr.AuthorId,
                    Author = new UserBasicDto
                    {
                        Name = rr.Author.FullName,
                        Email = rr.Author.Email,

                    },

                }).ToList(),
            })
            .ToListAsync();

        return Ok(new { ok = true, data = requisitions });
    }

    // Get a single requisition
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequisition(Guid id)
    {
        var requisition = await _dbContext.Requisitions
            .Include(r => r.RequestedUser)
            .Include(r => r.RequisitionItems)
                .ThenInclude(ri => ri.Material)
            .Include(r => r.RequisitionRemarks)
                .ThenInclude(rr => rr.Author)
            .Where(r => r.Id == id)
            .Select(r => new RequisitionResponseDto
            {
                Id = r.Id,
                RequestedUser = new UserBasicDto
                {
                    Name = r.RequestedUser!.FullName,
                    Email = r.RequestedUser.Email,
                },
                Status = r.Status,
                Description = r.Description,
                RequisitionItems = r.RequisitionItems.Select(ri => new RequisitionItemResponseDto
                {
                    Material = ri.Material != null ? new MaterialDetailDto
                    {
                        Name = ri.Material.Name,
                        Description = ri.Material.Description ?? "No description available",
                        Unit = ri.Material.Unit,
                    } : null,
                    Quantity = ri.Quantity,
                }).ToList(),
                RequisitionRemarks = r.RequisitionRemarks.Select(rr => new RequisitionRemarkResponseDto
                {
                    Content = rr.Content,
                    AuthorId = rr.AuthorId,
                    Author = rr.Author != null ? new UserBasicDto
                    {
                        Name = rr.Author.FullName,
                        Email = rr.Author.Email,
                    } : null,

                }).ToList(),
            })
            .FirstOrDefaultAsync();

        if (requisition is null)
        {
            return NotFound(new { ok = false, message = $"Requisition with id: {id} not found" });
        }

        return Ok(new { ok = true, data = requisition });
    }

    [Authorize(Policy = "AdminOnly")]
    // Create Requisition
    [HttpPost]
    public async Task<IActionResult> CreateRequisition(CreateRequisitionDt0 request)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return Unauthorized(new { ok = false, message = "User not authenticated" });
        }

        // step 2. Check if the items are not empty
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { ok = false, message = "Items should not be null or empty" });
        }

        // step 3. Validate that all materials exist
        var materialIds = request.Items.Select(i => i.MaterialId).ToList();
        var existingMaterials = await _dbContext.Materials
            .Where(m => materialIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync();

        var missingMaterials = materialIds.Except(existingMaterials).ToList();
        if (missingMaterials.Count != 0)
        {
            return BadRequest(new { ok = false, message = $"Materials not found: {string.Join(", ", missingMaterials)}" });
        }

        // step 4. Creation of requisition
        var newRequisition = new Requisition
        {
            RequestedUserId = request.RequestedUserId,
            Status = request.Status,
            Description = request.Description
        };

        // step 5. Add new requisition to the context and save changes
        _dbContext.Requisitions.Add(newRequisition);
        await _dbContext.SaveChangesAsync();

        // step 6. Loop through the items from the request
        var requisitionItems = request.Items.Select(item => new RequisitionItem
        {
            MaterialId = item.MaterialId,
            Quantity = item.Quantity,
            RequisitionId = newRequisition.Id
        }).ToList();

        // step 7. Add bulk items save the changes
        _dbContext.RequisitionItems.AddRange(requisitionItems);
        await _dbContext.SaveChangesAsync();

        // step 8: Return a meaningful message with the created requisition ID
        return StatusCode(201, new { ok = true, message = "Requisition submitted successfully", requisitionId = newRequisition.Id });
    }

    // Update Requisition
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRequisition(Guid id, UpdateRequisitionDto requisitionDto)
    {
        var requisition = await _dbContext.Requisitions
            .Include(r => r.RequisitionItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (requisition is null)
        {
            return NotFound(new { ok = false, message = $"Requisition with id: {id} not found" });
        }

        // Only allow updates if status is Pending or NeedsModification
        if (requisition.Status == "Approved" || requisition.Status == "Rejected")
        {
            return BadRequest(new { ok = false, message = "Cannot update approved or rejected requisitions" });
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(requisitionDto.Description))
        {
            requisition.Description = requisitionDto.Description;
        }

        if (!string.IsNullOrWhiteSpace(requisitionDto.Status))
        {
            requisition.Status = requisitionDto.Status;
        }

        // Update items if provided
        if (requisitionDto.Items != null && requisitionDto.Items.Any())
        {
            // Remove existing items
            _dbContext.RequisitionItems.RemoveRange(requisition.RequisitionItems);

            // Validate that all materials exist
            var materialIds = requisitionDto.Items.Select(i => i.MaterialId).ToList();
            var existingMaterials = await _dbContext.Materials
                .Where(m => materialIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            var missingMaterials = materialIds.Except(existingMaterials).ToList();
            if (missingMaterials.Any())
            {
                return BadRequest(new { ok = false, message = $"Materials not found: {string.Join(", ", missingMaterials)}" });
            }

            // Add new items
            var newItems = requisitionDto.Items.Select(item => new RequisitionItem
            {
                MaterialId = item.MaterialId,
                Quantity = item.Quantity,
                RequisitionId = requisition.Id
            }).ToList();

            _dbContext.RequisitionItems.AddRange(newItems);
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new { ok = true, message = "Requisition updated successfully" });
    }

    // Update Requisition Status (for approvers)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateRequisitionStatus(Guid id, UpdateRequisitionStatusDto statusDto)
    {
        var requisition = await _dbContext.Requisitions.FindAsync(id);
        if (requisition is null)
        {
            return NotFound(new { ok = false, message = $"Requisition with id: {id} not found" });
        }

        requisition.Status = statusDto.Status;
        await _dbContext.SaveChangesAsync();

        return Ok(new { ok = true, message = "Requisition status updated successfully" });
    }

    // Delete Requisition
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRequisition(Guid id)
    {
        var requisition = await _dbContext.Requisitions
            .Include(r => r.RequisitionItems)
            .Include(r => r.RequisitionRemarks)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (requisition is null)
        {
            return NotFound(new { ok = false, message = $"Requisition with id: {id} not found" });
        }

        // Only allow deletion if status is Pending
        if (requisition.Status != "Pending")
        {
            return BadRequest(new { ok = false, message = "Only pending requisitions can be deleted" });
        }

        // Remove related items and remarks first
        _dbContext.RequisitionItems.RemoveRange(requisition.RequisitionItems);
        _dbContext.RequisitionRemarks.RemoveRange(requisition.RequisitionRemarks);
        _dbContext.Requisitions.Remove(requisition);

        await _dbContext.SaveChangesAsync();
        return Ok(new { ok = true, message = "Requisition deleted successfully" });
    }

    // Get Requisitions by User
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetRequisitionsByUser(Guid userId)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return NotFound(new { ok = false, message = "User not found" });
        }

        var requisitions = await _dbContext.Requisitions
            .Include(r => r.RequestedUser)
            .Include(r => r.RequisitionItems)
                .ThenInclude(ri => ri.Material)
            .Include(r => r.RequisitionRemarks)
                .ThenInclude(rr => rr.Author)
            .Where(r => r.RequestedUserId == userId)
            .Select(r => new RequisitionResponseDto
            {
                Id = r.Id,
                RequestedUser = new UserBasicDto
                {
                    Name = r.RequestedUser!.FullName,
                    Email = r.RequestedUser.Email,
                },
                Status = r.Status,
                Description = r.Description,
                RequisitionItems = r.RequisitionItems.Select(ri => new RequisitionItemResponseDto
                {
                    Material = new MaterialDetailDto
                    {
                        Name = ri.Material!.Name,
                        Description = ri.Material.Description!,
                        Unit = ri.Material.Unit,

                    },
                    Quantity = ri.Quantity,
                }).ToList(),
                RequisitionRemarks = r.RequisitionRemarks.Select(rr => new RequisitionRemarkResponseDto
                {
                    Content = rr.Content,
                    AuthorId = rr.AuthorId,
                    Author = new UserBasicDto
                    {
                        Name = rr.Author.FullName,
                        Email = rr.Author.Email,

                    },

                }).ToList(),
            })
            .ToListAsync();

        return Ok(new { ok = true, data = requisitions });
    }

    // Get Requisitions by Status
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetRequisitionsByStatus(string status)
    {
        var validStatuses = new[] { "Pending", "Approved", "Rejected", "NeedsModification" };
        if (!validStatuses.Contains(status))
        {
            return BadRequest(new { ok = false, message = "Invalid status. Valid statuses are: Pending, Approved, Rejected, NeedsModification" });
        }

        var requisitions = await _dbContext.Requisitions
            .Include(r => r.RequestedUser)
            .Include(r => r.RequisitionItems)
                .ThenInclude(ri => ri.Material)
            .Include(r => r.RequisitionRemarks)
                .ThenInclude(rr => rr.Author)
            .Where(r => r.Status == status)
            .Select(r => new RequisitionResponseDto
            {
                Id = r.Id,
                RequestedUser = new UserBasicDto
                {
                    Name = r.RequestedUser!.FullName,
                    Email = r.RequestedUser.Email,

                },
                Status = r.Status,
                Description = r.Description,
                RequisitionItems = r.RequisitionItems.Select(ri => new RequisitionItemResponseDto
                {
                    Material = new MaterialDetailDto
                    {
                        Name = ri.Material!.Name,
                        Description = ri.Material.Description ?? "No Description available",
                        Unit = ri.Material.Unit,

                    },
                    Quantity = ri.Quantity,
                }).ToList(),
                RequisitionRemarks = r.RequisitionRemarks.Select(rr => new RequisitionRemarkResponseDto
                {
                    Content = rr.Content,
                    AuthorId = rr.AuthorId,
                    Author = new UserBasicDto
                    {
                        Name = rr.Author.FullName,
                        Email = rr.Author.Email,

                    },
                }).ToList(),
            })
            .ToListAsync();

        return Ok(new { ok = true, data = requisitions });
    }
}