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

            modelBuilder.Entity<City>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<VehicleCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<OrderStatus>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<SupportTicketStatus>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<PaymentStatus>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<TaxiCompany>()
                .HasQueryFilter(t => !t.IsDeleted);

            modelBuilder.Entity<TaxiCompany>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<TaxiCompany>()
                .HasOne(t => t.City)
                .WithMany()
                .HasForeignKey(t => t.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaxiCompany>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaxiCompany>()
                .Property(t => t.StartingPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TaxiCompany>()
                .Property(t => t.PricePerKm)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Driver>()
                .HasQueryFilter(d => !d.IsDeleted);

            modelBuilder.Entity<Driver>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Driver>()
                .HasOne(d => d.TaxiCompany)
                .WithMany()
                .HasForeignKey(d => d.TaxiCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.UserId)
                .IsUnique();

            modelBuilder.Entity<Car>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.Plate)
                .IsUnique();

            modelBuilder.Entity<Car>()
                .HasOne(c => c.VehicleCategory)
                .WithMany()
                .HasForeignKey(c => c.VehicleCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Car>()
                .HasOne(c => c.TaxiCompany)
                .WithMany()
                .HasForeignKey(c => c.TaxiCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarAssignment>()
                .HasQueryFilter(a => !a.IsDeleted);

            modelBuilder.Entity<CarAssignment>()
                .HasOne(a => a.Car)
                .WithMany()
                .HasForeignKey(a => a.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarAssignment>()
                .HasOne(a => a.Driver)
                .WithMany()
                .HasForeignKey(a => a.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarAssignment>()
                .HasIndex(a => new { a.CarId, a.AssignedFrom });

            modelBuilder.Entity<CarAssignment>()
                .HasIndex(a => new { a.DriverId, a.AssignedFrom });
        }
    }
}
