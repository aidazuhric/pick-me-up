using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services.Database
{
    public partial class PickMeUpDbContext : DbContext
    {
        private void CreateConfiguration(ModelBuilder modelBuilder)
        {
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
