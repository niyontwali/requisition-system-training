namespace RequisitionSystem.Data;

using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // properties for creating our tables
    public DbSet<Role> Roles { get; set; }
    public DbSet<Material> Materials {get; set;}
    
}