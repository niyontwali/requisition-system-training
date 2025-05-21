namespace RequisitionSystem.Data;

using Microsoft.EntityFrameworkCore;
using RequisitionSystem.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // properties for creating our tables
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Material> Materials { get; set; }

    public DbSet<Requisition> Requisitions { get; set; }
    public DbSet<RequisitionItem> RequisitionItems { get; set; }
    
     public DbSet<RequisitionRemark> RequisitionRemarks { get; set; }
     
}