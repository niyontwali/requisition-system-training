namespace RequisitionSystem.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

/*****************************************************************************
 * REQUISITIONS CONTROLLER
 * Handles all operations related to requisition management including
 * creation, retrieval, updating, and status management
 ****************************************************************************/
[ApiController]
[Route("api/requisitions")]
public class RequisitionsController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    /*************************************************************************
     * GET ALL REQUISITIONS - GET api/requisitions
     * Retrieves complete list of requisitions with all related data
     ************************************************************************/
    [Authorize]
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

    /*************************************************************************
     * GET SINGLE REQUISITION - GET api/requisitions/{id}
     * Retrieves detailed information for a specific requisition
     ************************************************************************/
    [Authorize]
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

    /*************************************************************************
     * CREATE REQUISITION - POST api/requisitions
     * Creates a new requisition with associated items (Admin only)
     ************************************************************************/
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateRequisition(CreateRequisitionDt0 request)
    {
        /*********************************************************************
         * STEP 1: Validate authenticated user
         ********************************************************************/
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Unauthorized(new { ok = false, message = "User not authenticated" });
        }

        /*********************************************************************
         * STEP 2: Validate requisition items
         ********************************************************************/
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { ok = false, message = "Items should not be null or empty" });
        }

        /*********************************************************************
         * STEP 3: Validate material existence
         ********************************************************************/
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

        /*********************************************************************
         * STEP 4: Create new requisition
         ********************************************************************/
        var newRequisition = new Requisition
        {
            RequestedUserId = request.RequestedUserId,
            Status = request.Status,
            Description = request.Description
        };

        /*********************************************************************
         * STEP 5: Save requisition to database
         ********************************************************************/
        _dbContext.Requisitions.Add(newRequisition);
        await _dbContext.SaveChangesAsync();

        /*********************************************************************
         * STEP 6: Create requisition items
         ********************************************************************/
        var requisitionItems = request.Items.Select(item => new RequisitionItem
        {
            MaterialId = item.MaterialId,
            Quantity = item.Quantity,
            RequisitionId = newRequisition.Id
        }).ToList();

        /*********************************************************************
         * STEP 7: Save items to database
         ********************************************************************/
        _dbContext.RequisitionItems.AddRange(requisitionItems);
        await _dbContext.SaveChangesAsync();

        /*********************************************************************
         * STEP 8: Return success response
         ********************************************************************/
        return StatusCode(201, new { ok = true, message = "Requisition submitted successfully", requisitionId = newRequisition.Id });
    }

    /*************************************************************************
     * UPDATE REQUISITION - PUT api/requisitions/{id}
     * Modifies an existing requisition (limited to certain statuses)
     ************************************************************************/
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRequisition(Guid id, UpdateRequisitionDto requisitionDto)
    {
        /*********************************************************************
         * STEP 1: Retrieve and validate requisition
         ********************************************************************/
        var requisition = await _dbContext.Requisitions
            .Include(r => r.RequisitionItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (requisition is null)
        {
            return NotFound(new { ok = false, message = $"Requisition with id: {id} not found" });
        }

        /*********************************************************************
         * STEP 2: Validate requisition status
         ********************************************************************/
        if (requisition.Status == "Approved" || requisition.Status == "Rejected")
        {
            return BadRequest(new { ok = false, message = "Cannot update approved or rejected requisitions" });
        }

        /*********************************************************************
         * STEP 3: Update basic fields
         ********************************************************************/
        if (!string.IsNullOrWhiteSpace(requisitionDto.Description))
        {
            requisition.Description = requisitionDto.Description;
        }

        if (!string.IsNullOrWhiteSpace(requisitionDto.Status))
        {
            requisition.Status = requisitionDto.Status;
        }

        /*********************************************************************
         * STEP 4: Handle items update if provided
         ********************************************************************/
        if (requisitionDto.Items != null && requisitionDto.Items.Any())
        {
            // Remove existing items
            _dbContext.RequisitionItems.RemoveRange(requisition.RequisitionItems);

            // Validate new materials
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

    /*************************************************************************
     * UPDATE REQUISITION STATUS - PATCH api/requisitions/{id}/status
     * Allows approvers to change requisition status
     ************************************************************************/
    [Authorize(Policy = "AdminOnly")]
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

    /*************************************************************************
     * DELETE REQUISITION - DELETE api/requisitions/{id}
     * Removes a requisition and all related data (only for pending status)
     ************************************************************************/
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRequisition(Guid id)
    {
        /*********************************************************************
         * STEP 1: Retrieve and validate requisition
         ********************************************************************/
        var requisition = await _dbContext.Requisitions
            .Include(r => r.RequisitionItems)
            .Include(r => r.RequisitionRemarks)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (requisition is null)
        {
            return NotFound(new { ok = false, message = $"Requisition with id: {id} not found" });
        }

        /*********************************************************************
         * STEP 2: Validate requisition status
         ********************************************************************/
        if (requisition.Status != "Pending")
        {
            return BadRequest(new { ok = false, message = "Only pending requisitions can be deleted" });
        }

        /*********************************************************************
         * STEP 3: Remove all related data
         ********************************************************************/
        _dbContext.RequisitionItems.RemoveRange(requisition.RequisitionItems);
        _dbContext.RequisitionRemarks.RemoveRange(requisition.RequisitionRemarks);
        _dbContext.Requisitions.Remove(requisition);

        await _dbContext.SaveChangesAsync();
        return Ok(new { ok = true, message = "Requisition deleted successfully" });
    }

    /*************************************************************************
     * GET REQUISITIONS BY USER - GET api/requisitions/user/{userId}
     * Retrieves all requisitions for a specific user
     ************************************************************************/
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetRequisitionsByUser(Guid userId)
    {
        /*********************************************************************
         * STEP 1: Validate user existence
         ********************************************************************/
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return NotFound(new { ok = false, message = "User not found" });
        }

        /*********************************************************************
         * STEP 2: Retrieve user requisitions
         ********************************************************************/
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

    /*************************************************************************
     * GET REQUISITIONS BY STATUS - GET api/requisitions/status/{status}
     * Retrieves all requisitions with a specific status
     ************************************************************************/
    [Authorize]
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetRequisitionsByStatus(string status)
    {
        /*********************************************************************
         * STEP 1: Validate status
         ********************************************************************/
        var validStatuses = new[] { "Pending", "Approved", "Rejected", "NeedsModification" };
        if (!validStatuses.Contains(status))
        {
            return BadRequest(new { ok = false, message = "Invalid status. Valid statuses are: Pending, Approved, Rejected, NeedsModification" });
        }

        /*********************************************************************
         * STEP 2: Retrieve filtered requisitions
         ********************************************************************/
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