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

            modelBuilder.Entity<Shift>()
                .HasQueryFilter(s => !s.IsDeleted);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Driver)
                .WithMany()
                .HasForeignKey(s => s.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shift>()
                .HasIndex(s => new { s.DriverId, s.EndedAt });

            modelBuilder.Entity<ShiftBreak>()
                .HasOne(b => b.Shift)
                .WithMany(s => s.Breaks)
                .HasForeignKey(b => b.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShiftBreak>()
                .HasQueryFilter(b => !b.Shift.IsDeleted);

            modelBuilder.Entity<ShiftBreak>()
                .HasIndex(b => new { b.ShiftId, b.EndedAt });

            modelBuilder.Entity<Order>()
                .HasQueryFilter(o => !o.IsDeleted);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Passenger).WithMany().HasForeignKey(o => o.PassengerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.TaxiCompany).WithMany().HasForeignKey(o => o.TaxiCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Driver).WithMany().HasForeignKey(o => o.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Car).WithMany().HasForeignKey(o => o.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderStatus).WithMany().HasForeignKey(o => o.OrderStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>().Property(o => o.DistanceKm).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.Fare).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().HasIndex(o => new { o.OrderStatusId, o.TaxiCompanyId });

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.Order).WithMany(o => o.StatusHistory).HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.FromStatus).WithMany().HasForeignKey(h => h.FromStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.ToStatus).WithMany().HasForeignKey(h => h.ToStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.ChangedByUser).WithMany().HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasQueryFilter(h => !h.Order.IsDeleted);

            modelBuilder.Entity<OrderRejection>()
                .HasOne(r => r.Order).WithMany(o => o.Rejections).HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderRejection>()
                .HasOne(r => r.Driver).WithMany().HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderRejection>()
                .HasQueryFilter(r => !r.Order.IsDeleted);

            modelBuilder.Entity<OrderRejection>()
                .HasIndex(r => new { r.OrderId, r.DriverId }).IsUnique();
        }
    }
}
