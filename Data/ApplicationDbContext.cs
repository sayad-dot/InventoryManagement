using InventoryManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<InventoryAccess> InventoryAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Inventory>()
                .HasOne(i => i.Creator)
                .WithMany(u => u.OwnedInventories)
                .HasForeignKey(i => i.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Item>()
                .HasOne(i => i.Inventory)
                .WithMany(inv => inv.Items)
                .HasForeignKey(i => i.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryAccess>()
                .HasOne(ia => ia.Inventory)
                .WithMany(i => i.AccessUsers)
                .HasForeignKey(ia => ia.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryAccess>()
                .HasOne(ia => ia.User)
                .WithMany()
                .HasForeignKey(ia => ia.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}