namespace RequisitionSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

/*****************************************************************************
 * MATERIALS CONTROLLER
 * Handles CRUD operations for material resources
 ****************************************************************************/
[ApiController]
[Route("api/materials")]
public class MaterialsController(ApplicationDbContext dbContext, ILogger<MaterialsController> logger) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<MaterialsController> _logger = logger;

    /*************************************************************************
     * GET ALL MATERIALS - GET api/materials
     * Retrieves a list of all materials
     ************************************************************************/
    [HttpGet]
    public async Task<IActionResult> GetMaterials()
    {
        var materials = await _dbContext.Materials.ToListAsync();
        _logger.LogInformation("Got all materials");
        return Ok(new { ok = true, data = materials });
    }

    /*************************************************************************
     * GET SINGLE MATERIAL - GET api/materials/{id}
     * Retrieves a specific material by ID
     ************************************************************************/
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMaterial(Guid id)
    {
        var material = await _dbContext.Materials.FindAsync(id);

        if (material is null)
        {
            return NotFound(new { ok = false, message = $"Material with id: {id} not found" });
        }

        return Ok(new { ok = true, data = material });
    }

    /*************************************************************************
     * CREATE MATERIAL - POST api/materials
     * Creates a new material record
     ************************************************************************/
    [HttpPost]
    public async Task<IActionResult> CreateMaterial(CreateMaterialDto materialDto)
    {
        var newMaterial = new Material
        {
            Name = materialDto.Name,
            Description = materialDto.Description,
            Unit = materialDto.Unit
        };

        _dbContext.Materials.Add(newMaterial);
        await _dbContext.SaveChangesAsync();

        return Ok(new { ok = true, message = "Material created successfully" });
    }

    /*************************************************************************
     * UPDATE MATERIAL - PUT api/materials/{id}
     * Updates an existing material record
     ************************************************************************/
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(Guid id, UpdateMaterialDto materialDto)
    {
        var material = await _dbContext.Materials.FindAsync(id);
        if (material is null)
        {
            return NotFound(new { ok = false, message = $"Material with {id} not found" });
        }

        /*********************************************************************
         * Apply partial updates only for provided fields
         ********************************************************************/
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

    /*************************************************************************
     * DELETE MATERIAL - DELETE api/materials/{id}
     * Removes a material record
     ************************************************************************/
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