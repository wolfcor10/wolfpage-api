using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<PageGenerationRequest> PageGenerationRequests => Set<PageGenerationRequest>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageAsset> PageAssets => Set<PageAsset>();
    public DbSet<DomainBinding> DomainBindings => Set<DomainBinding>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenant");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasMany(x => x.PageGenerationRequests)
                .WithOne(x => x.Tenant)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Pages)
                .WithOne(x => x.Tenant)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Users)
                .WithOne(x => x.Tenant)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.ToTable("template");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();

            entity.HasMany(x => x.Versions)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TemplateVersion>(entity =>
        {
            entity.ToTable("template_version");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.VersionNumber).IsRequired();
            entity.Property(x => x.Engine).HasMaxLength(50);
            entity.Property(x => x.HtmlTemplate).IsRequired();
            entity.Property(x => x.CssTemplate);
            entity.Property(x => x.JsTemplate);
            entity.Property(x => x.SchemaJson);
            entity.Property(x => x.IsPublished).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasIndex(x => new { x.TemplateId, x.VersionNumber }).IsUnique();
        });

        modelBuilder.Entity<PageGenerationRequest>(entity =>
        {
            entity.ToTable("page_generation_request");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PageName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ContentJson);
            entity.Property(x => x.ErrorMessage);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ProcessedAt);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(x => x.CorrelationId).IsUnique();

            entity.HasOne(x => x.TemplateVersion)
                .WithMany(x => x.PageGenerationRequests)
                .HasForeignKey(x => x.TemplateVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("page");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RoutePath).HasMaxLength(250).IsRequired();
            entity.Property(x => x.HtmlContent).IsRequired();
            entity.Property(x => x.CssContent);
            entity.Property(x => x.JsContent);
            entity.Property(x => x.PublishedUrl).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.RequestId).IsUnique();

            entity.HasOne(x => x.TemplateVersion)
                .WithMany(x => x.Pages)
                .HasForeignKey(x => x.TemplateVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Request)
                .WithOne(x => x.Page)
                .HasForeignKey<Page>(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PageAsset>(entity =>
        {
            entity.ToTable("page_asset");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.AssetType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(100);
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasOne(x => x.Page)
                .WithMany(x => x.Assets)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DomainBinding>(entity =>
        {
            entity.ToTable("domain_binding");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.DomainName).HasMaxLength(255);
            entity.Property(x => x.Subdomain).HasMaxLength(150);
            entity.Property(x => x.IsPrimary).IsRequired();
            entity.Property(x => x.SslStatus).HasMaxLength(50);
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasOne(x => x.Page)
                .WithMany(x => x.DomainBindings)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.LastLoginAt);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

            entity.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();

            entity.HasMany(x => x.UserRoles)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_role");
            entity.HasKey(x => new { x.UserId, x.RoleId });

            entity.Property(x => x.AssignedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
