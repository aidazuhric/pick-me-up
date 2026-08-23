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

            modelBuilder.Entity<TwoFactorCode>()
                .HasQueryFilter(c => !c.User.IsDeleted);

            modelBuilder.Entity<TwoFactorCode>()
                .HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TwoFactorCode>()
                .HasIndex(c => new { c.UserId, c.ConsumedAt, c.ExpiresAt });

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

            modelBuilder.Entity<DriverLocation>()
                .HasOne(l => l.Driver)
                .WithOne()
                .HasForeignKey<DriverLocation>(l => l.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DriverLocation>()
                .HasQueryFilter(l => !l.Driver.IsDeleted);

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

            modelBuilder.Entity<Payment>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order).WithMany().HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentStatus).WithMany().HasForeignKey(p => p.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>().Property(p => p.AmountCharged).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().HasIndex(p => p.OrderId);
            modelBuilder.Entity<Payment>().HasIndex(p => p.StripePaymentIntentId).IsUnique();

            modelBuilder.Entity<PaymentRefund>()
                .HasOne(r => r.Payment).WithMany(p => p.Refunds).HasForeignKey(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentRefund>().Property(r => r.RefundedAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PaymentRefund>().HasQueryFilter(r => !r.Payment.IsDeleted);

            modelBuilder.Entity<Review>()
                .HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Order).WithMany().HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Passenger).WithMany().HasForeignKey(r => r.PassengerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.TargetCompany).WithMany().HasForeignKey(r => r.TargetCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.TargetDriver).WithMany().HasForeignKey(r => r.TargetDriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.OrderId, r.TargetCompanyId }).IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.OrderId, r.TargetDriverId }).IsUnique();

            modelBuilder.Entity<SupportTicket>()
                .HasQueryFilter(t => !t.IsDeleted);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Submitter).WithMany().HasForeignKey(t => t.SubmitterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.TargetUser).WithMany().HasForeignKey(t => t.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.ResolvedByUser).WithMany().HasForeignKey(t => t.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.SupportTicketStatus).WithMany().HasForeignKey(t => t.SupportTicketStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicket>()
                .HasIndex(t => new { t.SupportTicketStatusId, t.CreatedAt });

            modelBuilder.Entity<SupportTicketReply>()
                .HasOne(r => r.SupportTicket).WithMany(t => t.Replies).HasForeignKey(r => r.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportTicketReply>()
                .HasOne(r => r.Author).WithMany().HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicketReply>()
                .HasQueryFilter(r => !r.SupportTicket.IsDeleted);

            modelBuilder.Entity<Notification>()
                .HasQueryFilter(n => !n.IsDeleted);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Order).WithMany().HasForeignKey(n => n.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

            modelBuilder.Entity<InteractionHistory>()
                .HasQueryFilter(i => !i.User.IsDeleted && !i.TaxiCompany.IsDeleted);

            modelBuilder.Entity<InteractionHistory>()
                .HasOne(i => i.User).WithMany().HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InteractionHistory>()
                .HasOne(i => i.TaxiCompany).WithMany().HasForeignKey(i => i.TaxiCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InteractionHistory>()
                .HasOne(i => i.Order).WithMany().HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InteractionHistory>()
                .Property(i => i.Value).HasPrecision(18, 2);

            modelBuilder.Entity<InteractionHistory>()
                .HasIndex(i => new { i.UserId, i.TaxiCompanyId, i.SignalTypeId });

            modelBuilder.Entity<InteractionHistory>()
                .HasIndex(i => new { i.TaxiCompanyId, i.SignalTypeId });

            modelBuilder
                .HasDbFunction(typeof(PostgresFunctions).GetMethod(nameof(PostgresFunctions.DateTrunc))!)
                .HasName("date_trunc");
        }
    }
}
