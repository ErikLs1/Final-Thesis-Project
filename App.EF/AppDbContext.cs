using App.Domain;
using App.Domain.Common;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.EF;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Languages> Languages { get; set; } = null!;
    public DbSet<UserLanguages> UserLanguages { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = "Some User";
                    entry.Entity.UpdatedAt = null;
                    entry.Entity.UpdatedBy = null;
                    break;
                
                case EntityState.Modified:
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = "Some User";
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        
        // CATEGORY
        b.Entity<Category>(e =>
        {
            e.ToTable("category");

            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            e.Property(p => p.Name)
                .HasMaxLength(255)
                .IsRequired();

            e.Property(p => p.Description)
                .HasMaxLength(512);

            e.HasIndex(p => p.Name)
                .IsUnique();
        });

        // PRODUCT
        b.Entity<Product>(e =>
        {
            e.ToTable("products");

            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            e.Property(p => p.Name)
                .HasMaxLength(255)
                .IsRequired();
            
            e.Property(p => p.Description)
                .HasMaxLength(1024)
                .IsRequired();

            e.Property(p => p.Price)
                .HasColumnType("double precision");
            
            e.Property(p => p.Quantity)
                .HasDefaultValue(0);
            
            e.Property(p => p.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
            
            e.Property(p => p.UpdatedBy)
                .HasMaxLength(100);
            
            e.HasOne(p => p.Category)
                .WithMany(u => u.Products)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LANGUAGES
        b.Entity<Languages>(e =>
        {
            // TODO: IMPLEMENT LATER
            e.ToTable("languages");

            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
        });

        // USER_LANGUAGES
        b.Entity<UserLanguages>(e =>
        {
            e.ToTable("user_languages");
            
            e.HasIndex(p => new { p.UserId, p.LanguageId }).IsUnique();
            e.HasIndex(p => p.UserId);
            e.HasIndex(p => p.LanguageId);

            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            
            e.HasOne(p => p.User)
                .WithMany(u => u.UserLanguages)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.HasOne(p => p.Language)
                .WithMany(u => u.UserLanguages)
                .HasForeignKey(r => r.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}