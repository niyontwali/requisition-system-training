namespace RequisitionSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Data;
using RequisitionSystem.DTOs;
using RequisitionSystem.Models;

[ApiController]
[Route("api/materials")]
public class MaterialsController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    // Get Materials
    [HttpGet]
    public async Task<IActionResult> GetMaterials()
    {
        var materials = await _dbContext.Materials.ToListAsync();
        return Ok(new { ok = true, data = materials });
    }

    // Get a single material
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

    // Create material
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