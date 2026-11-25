using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Infrastructure.Data;

/// <summary>
/// نقطه‌ی دسترسی به دیتابیس (مثل RoomDatabase در اندروید) + جدول‌های کاربران و نقش‌ها (Identity).
/// تنظیمات هر جدول در یک فایل جدا در پوشه‌ی Configurations است.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, int>(options)
{
    // طبقه‌بندی
    public DbSet<AgeGroup> AgeGroups => Set<AgeGroup>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<ImpactLevel> ImpactLevels => Set<ImpactLevel>();

    // محتوا
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<ContentSource> ContentSources => Set<ContentSource>();
    public DbSet<ContentFile> ContentFiles => Set<ContentFile>();
    public DbSet<ContentTopic> ContentTopics => Set<ContentTopic>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<ContentPerson> ContentPeople => Set<ContentPerson>();
    public DbSet<ContentAgeGroup> ContentAgeGroups => Set<ContentAgeGroup>();

    // تنظیمات
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // نام جدول‌های Identity (به‌جای AspNet...)
        modelBuilder.Entity<AppUser>().ToTable("AppUsers");
        modelBuilder.Entity<AppRole>().ToTable("AppRoles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AppUserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AppUserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AppUserLogins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AppUserTokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AppRoleClaims");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
