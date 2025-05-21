namespace RequisitionSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

[ApiController]
[Route("api/requisitions")]
public class RequsitionsController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    // Get Requisitions
    [HttpGet]
    public async Task<IActionResult> GetRequisition()
    {
        var requisitions = await _dbContext.Requisitions
        .Include(r => r.RequisitionItems)
        .FirstOrDefaultAsync();

        return Ok(new { ok = true, data = requisitions });
    }

    // Get a single requisition
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequisition(Guid id)
    {
        var requisition = await _dbContext.Requisitions
        .Include(r => r.RequestedUser)
        .Include(r => r.RequisitionItems)
        .Include(r => r.RequisitionRemarks)
        .FirstOrDefaultAsync(r => r.Id == id);

        if (requisition is null)
        {
            return NotFound(new { ok = false, message = $"Requisition with id: {id} not found" });
        }

        return Ok(new { ok = true, data = requisition });
    }

    // Create Requisition
    [HttpPost]
    public async Task<IActionResult> CreateRequisition(CreateRequisitionDt0 request)
    {

        // step 1. Validate the user requesting requistion
        bool userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.RequestedUserId);
        if (!userExists)
        {
            return NotFound(new { ok = false, message = "User not found" });
        }

        // check if the items are not empty
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { ok = false, message = "Items should not be null or empty" });
        }

        // step 2. Creation of requition
        // Description (From the body), status will default to Pending
        var newRequisition = new Requisition
        {
            RequestedUserId = request.RequestedUserId,
            Status = request.Status,
            Description = request.Description
        };

        // add new user to the context and save chnages
        _dbContext.Requisitions.Add(newRequisition);
        await _dbContext.SaveChangesAsync();

        // step 3. Loop through the items from the request
        var requisitionItems = request.Items.Select(item => new RequisitionItem
        {
            MaterialId = item.MaterialId,
            Quantity = item.Quantity,
            RequisitionId = newRequisition.Id
        }).ToList();

        // add bulky items
        _dbContext.RequisitionItems.AddRange(requisitionItems);

        // save the changes
        await _dbContext.SaveChangesAsync();

        // step 4: Return a meaningful message
        return Ok(new { ok = true, message = "Requisition submitted successfully" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(Guid id, UpdateMaterialDto materialDto)
    {
        var material = await _dbContext.Materials.FindAsync(id);
        if (material is null)
        {
            return NotFound(new { ok = false, message = $"Material with {id} not found" });
        }

        if (!string.IsNullOrWhiteSpace(materialDto.Name))
        {
            material.Name = materialDto.Name;
        }

        if (!string.IsNullOrWhiteSpace(materialDto.Description))
        {
            material.Description = materialDto.Description;
        }
        if (!string.IsNullOrWhiteSpace(materialDto.Unit))
        {
            material.Unit = materialDto.Unit;
        }


        await _dbContext.SaveChangesAsync();

        return Ok(new { ok = true, message = "Material updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaterial(Guid id)
    {
        var material = await _dbContext.Materials.FindAsync(id);
        if (material is null)
        {
            return NotFound(new { ok = true, message = $"Material with {id} not found" });
        }

        _dbContext.Materials.Remove(material);
        await _dbContext.SaveChangesAsync();
        return Ok(new { ok = true, message = "Material deleted successfully" });
    }
}