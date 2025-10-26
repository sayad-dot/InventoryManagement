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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<InventoryTag> InventoryTags { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<ItemLike> ItemLikes { get; set; }

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

            // Fix: Change InventoryAccess UserId to string to match IdentityUser
            builder.Entity<InventoryAccess>()
                .HasOne(ia => ia.Inventory)
                .WithMany(i => i.AccessUsers)
                .HasForeignKey(ia => ia.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryAccess>()
                .HasOne(ia => ia.User)
                .WithMany(u => u.AccessibleInventories)
                .HasForeignKey(ia => ia.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many for Inventory-Tag
            builder.Entity<InventoryTag>()
                .HasKey(it => new { it.InventoryId, it.TagId });

            builder.Entity<InventoryTag>()
                .HasOne(it => it.Inventory)
                .WithMany(i => i.InventoryTags)
                .HasForeignKey(it => it.InventoryId);

            builder.Entity<InventoryTag>()
                .HasOne(it => it.Tag)
                .WithMany(t => t.InventoryTags)
                .HasForeignKey(it => it.TagId);

            // Seed initial categories and tags
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Equipment", Description = "Office and technical equipment" },
                new Category { Id = 2, Name = "Furniture", Description = "Office furniture and fixtures" },
                new Category { Id = 3, Name = "Books", Description = "Books and publications" },
                new Category { Id = 4, Name = "Electronics", Description = "Electronic devices and components" },
                new Category { Id = 5, Name = "Vehicles", Description = "Company vehicles and transportation" },
                new Category { Id = 6, Name = "Tools", Description = "Tools and workshop equipment" },
                new Category { Id = 7, Name = "Other", Description = "Other categories" }
            );

            builder.Entity<Tag>().HasData(
                new Tag { Id = 1, Name = "office" },
                new Tag { Id = 2, Name = "technology" },
                new Tag { Id = 3, Name = "furniture" },
                new Tag { Id = 4, Name = "books" },
                new Tag { Id = 5, Name = "electronics" },
                new Tag { Id = 6, Name = "tools" },
                new Tag { Id = 7, Name = "vehicles" },
                new Tag { Id = 8, Name = "supplies" }
            );

            // Configure Discussion relationships
            builder.Entity<Discussion>()
                .HasOne(d => d.Inventory)
                .WithMany()
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Discussion>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ItemLike relationships
            builder.Entity<ItemLike>()
                .HasOne(il => il.Item)
                .WithMany(i => i.Likes)
                .HasForeignKey(il => il.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItemLike>()
                .HasOne(il => il.User)
                .WithMany()
                .HasForeignKey(il => il.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique likes per user per item
            builder.Entity<ItemLike>()
                .HasIndex(il => new { il.ItemId, il.UserId })
                .IsUnique();
        }
    }
}