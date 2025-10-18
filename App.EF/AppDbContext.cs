using App.Domain;
using App.Domain.AB;
using App.Domain.Common;
using App.Domain.Identity;
using App.Domain.UITranslationEntities;
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
    
    // UI TRANSLATIONS
    public DbSet<UIExperiment> UIExperiments { get; set; } = null!;
    public DbSet<UIResourceKeys> UIResourceKeys { get; set; } = null!;
    public DbSet<UITranslationAuditLog> UITranslation { get; set; } = null!;
    public DbSet<UITranslations> UITranslations { get; set; } = null!;
    public DbSet<UITranslationVersions> UITranslationVersions { get; set; } = null!;

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
            e.ToTable("languages");

            e.HasIndex(p => p.LanguageTag).IsUnique();
                
            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            
            e.Property(p => p.LanguageTag)
                .IsRequired();
            
            e.Property(p => p.LanguageName)
                .IsRequired();
            
            e.Property(p => p.IsDefaultLanguage)
                .IsRequired();
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
        
        // UI_RESOURCE_KEYS
        b.Entity<UIResourceKeys>(e =>
        {
            e.ToTable("ui_resource_keys");
            
            e.HasIndex(p => p.ResourceKey).IsUnique();
            
            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            e.Property(p => p.ResourceKey)
                .IsRequired();
        });
        
        
        // UI_TRANSLATION_VERSIONS
        b.Entity<UITranslationVersions>(e =>
        {
            e.ToTable("ui_translation_versions");

            //e.HasIndex(p => new { p.LanguageId, p.ResourceKeyId });
            
            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            e.Property(p => p.Content)
                .IsRequired();
            
            e.Property(p => p.TranslationState)
                .IsRequired();
            
            e.Property(p => p.CreatedAt)
                .IsRequired();
            
            e.Property(p => p.CreatedBy)
                .IsRequired();

            e.HasOne(p => p.UIResourceKeys)
                .WithMany(p => p.UITranslationVersions)
                .HasForeignKey(p => p.ResourceKeyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.HasOne(p => p.Language)
                .WithMany(p => p.UITranslationVersions)
                .HasForeignKey(p => p.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // UI_TRANSLATIONS
        b.Entity<UITranslations>(e =>
        {
            e.ToTable("ui_translations");
            
            e.HasIndex(p => new { p.LanguageId, p.ResourceKeyId }).IsUnique();
            
            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            
            e.Property(p => p.PublishedAt)
                .IsRequired();
            
            e.Property(p => p.PublishedBy)
                .IsRequired();
            
            e.HasOne(p => p.Language)
                .WithOne(l => l.UITranslations)
                .HasForeignKey<UITranslations>(p => p.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);
            
            e.HasOne(p => p.UIResourceKeys)
                .WithOne(rk => rk.UITranslations)
                .HasForeignKey<UITranslations>(p => p.ResourceKeyId)
                .OnDelete(DeleteBehavior.Restrict);
            
            e.HasOne(p => p.UITranslationVersions)
                .WithMany()
                .HasForeignKey(p => new { p.LanguageId, p.ResourceKeyId, p.TranslationVersionId })
                .HasPrincipalKey(v => new { v.LanguageId, v.ResourceKeyId, v.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // UI_TRANSLATION_AUDIT_LOG
        b.Entity<UITranslationAuditLog>(e =>
        {
            e.ToTable("ui_translation_audit_log");
            
            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            
            e.Property(p => p.ActivatedAt)
                .IsRequired();
            
            e.Property(p => p.ActivatedBy)
                .IsRequired();
            
            e.Property(p => p.DeactivatedAt)
                .IsRequired();
            
            e.Property(p => p.DeactivatedBy)
                .IsRequired();

            e.HasOne(p => p.Language)
                .WithMany(l => l.UITranslationAuditLogs)
                .HasForeignKey(p => p.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.UIResourceKeys)
                .WithMany(rk => rk.UITranslationAuditLogs)
                .HasForeignKey(p => p.ResourceKeyId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.UITranslationVersions)
                .WithMany(v => v.UITranslationAuditLogs)
                .HasForeignKey(p => p.TranslationVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // UI_EXPERIMENTS
        b.Entity<UIExperiment>(e =>
        {
            e.ToTable("ui_experiments");
            
            e.HasIndex(p => new { p.LanguageId, p.ResourceKeyId }).IsUnique();
            
            e.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            
            e.Property(p => p.ExperimentName)
                .IsRequired();
            
            e.Property(p => p.Option)
                .IsRequired();
            
            e.HasOne(p => p.Language)
                .WithMany(l => l.UIExperiments)
                .HasForeignKey(p => p.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.UIResourceKeys)
                .WithMany(rk => rk.UIExperiments)
                .HasForeignKey(p => p.ResourceKeyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.HasOne(p => p.UITranslationVersions)
                .WithMany()
                .HasForeignKey(p => new { p.LanguageId, p.ResourceKeyId, p.TranslationVersionId })
                .HasPrincipalKey(v => new { v.LanguageId, v.ResourceKeyId, v.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}