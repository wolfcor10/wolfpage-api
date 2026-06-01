using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<PageGenerationRequest> PageGenerationRequests => Set<PageGenerationRequest>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageItem> PageItems => Set<PageItem>();
    public DbSet<PageAsset> PageAssets => Set<PageAsset>();
    public DbSet<DomainBinding> DomainBindings => Set<DomainBinding>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.ToTable("workspace");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.WorkspaceType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.ProfileType).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasMany(x => x.PageGenerationRequests)
                .WithOne(x => x.Workspace)
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Pages)
                .WithOne(x => x.Workspace)
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Members)
                .WithOne(x => x.Workspace)
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(x => x.SelectedTemplateId).HasMaxLength(100).IsRequired();
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
            entity.HasIndex(x => x.PageId);

            entity.HasOne(x => x.TemplateVersion)
                .WithMany(x => x.PageGenerationRequests)
                .HasForeignKey(x => x.TemplateVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Page)
                .WithOne(x => x.Request)
                .HasForeignKey<PageGenerationRequest>(x => x.PageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("page");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.SelectedTemplateId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RoutePath).HasMaxLength(250).IsRequired();
            entity.Property(x => x.HtmlContent);
            entity.Property(x => x.CssContent);
            entity.Property(x => x.JsContent);
            entity.Property(x => x.BusinessName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.BusinessCategory).HasMaxLength(100);
            entity.Property(x => x.BusinessDescription).IsRequired();
            entity.Property(x => x.LogoUrl).HasMaxLength(500);
            entity.Property(x => x.HeroTitle).HasMaxLength(200).IsRequired();
            entity.Property(x => x.HeroSubtitle).HasMaxLength(500);
            entity.Property(x => x.HeroImageUrl).HasMaxLength(500);
            entity.Property(x => x.Phone).HasMaxLength(80);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.WhatsApp).HasMaxLength(80);
            entity.Property(x => x.OpeningHours).HasMaxLength(300);
            entity.Property(x => x.SocialLinksJson);
            entity.Property(x => x.GeneratedFilePath).HasMaxLength(800);
            entity.Property(x => x.PublishedUrl).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(x => new { x.WorkspaceId, x.Slug }).IsUnique();
            entity.HasIndex(x => x.RequestId).IsUnique().HasFilter("[RequestId] IS NOT NULL");

            entity.HasOne(x => x.TemplateVersion)
                .WithMany(x => x.Pages)
                .HasForeignKey(x => x.TemplateVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Page)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PageItem>(entity =>
        {
            entity.ToTable("page_item");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(800).IsRequired();
            entity.Property(x => x.Price).HasMaxLength(80);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.Property(x => x.Enabled).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();

            entity.HasIndex(x => new { x.PageId, x.SortOrder });
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
            entity.Property(x => x.PasswordHash).HasMaxLength(500);
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.EmailConfirmed).IsRequired();
            entity.Property(x => x.EmailConfirmedAt);
            entity.Property(x => x.LastLoginAt);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasMany(x => x.WorkspaceMemberships)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.SentWorkspaceInvitations)
                .WithOne(x => x.InvitedByUser)
                .HasForeignKey(x => x.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.ExternalLogins)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Tokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserExternalLogin>(entity =>
        {
            entity.ToTable("user_external_login");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Provider).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ProviderUserId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
            entity.HasIndex(x => new { x.Provider, x.Email });
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.ToTable("user_token");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Purpose).HasMaxLength(80).IsRequired();
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExpiresAt).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UsedAt);

            entity.HasIndex(x => new { x.Purpose, x.TokenHash }).IsUnique();
            entity.HasIndex(x => x.UserId);
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

            entity.HasMany(x => x.WorkspaceMembers)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkspaceMember>(entity =>
        {
            entity.ToTable("workspace_member");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.JoinedAt);
            entity.Property(x => x.InvitedAt);
            entity.Property(x => x.RemovedAt);

            entity.HasIndex(x => new { x.WorkspaceId, x.UserId, x.RoleId }).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.RoleId);
            entity.HasIndex(x => x.InvitedByUserId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
