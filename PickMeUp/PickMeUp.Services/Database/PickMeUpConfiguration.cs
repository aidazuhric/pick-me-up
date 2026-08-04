using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services.Database
{
    public partial class PickMeUpDbContext : DbContext
    {
        private void CreateConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.IsDeleted);

            modelBuilder.Entity<UserRole>()
                .HasQueryFilter(ur => !ur.User.IsDeleted);

            modelBuilder.Entity<RefreshToken>()
                .HasQueryFilter(rt => !rt.User.IsDeleted);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(ur => ur.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(ur => ur.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
