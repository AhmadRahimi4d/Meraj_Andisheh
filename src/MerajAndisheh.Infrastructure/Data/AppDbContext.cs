using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Infrastructure.Data;

/// <summary>
/// نقطه‌ی دسترسی به دیتابیس (مثل RoomDatabase در اندروید).
/// در مرحله‌ی ۱ عمداً خالی است؛ DbSetها در مرحله‌ی ۳ اضافه می‌شوند.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
