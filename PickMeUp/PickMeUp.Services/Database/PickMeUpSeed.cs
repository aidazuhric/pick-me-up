using PickMeUp.Model.Enums;
using RoleNames = PickMeUp.Model.Constants.Roles;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services.Database
{
    public partial class PickMeUpDbContext : DbContext
    {
        private static readonly DateTime SeedTimestamp = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        private void CreateSeed(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUserRoles(modelBuilder);
            SeedCities(modelBuilder);
            SeedVehicleCategories(modelBuilder);
            SeedOrderStatuses(modelBuilder);
            SeedSupportTicketStatuses(modelBuilder);
            SeedPaymentStatuses(modelBuilder);
            SeedTaxiCompanies(modelBuilder);
            SeedDrivers(modelBuilder);
            SeedCars(modelBuilder);
        }

        private void SeedCities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().HasData(
                new { Id = 1, Name = "Mostar", PostalCode = (string?)"88000", IsActive = true },
                new { Id = 2, Name = "Sarajevo", PostalCode = (string?)"71000", IsActive = true },
                new { Id = 3, Name = "Banja Luka", PostalCode = (string?)"78000", IsActive = true },
                new { Id = 4, Name = "Tuzla", PostalCode = (string?)"75000", IsActive = true },
                new { Id = 5, Name = "Zenica", PostalCode = (string?)"72000", IsActive = true }
            );
        }

        private void SeedVehicleCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleCategory>().HasData(
                new { Id = 1, Name = "Standard", Description = (string?)"Everyday four-seat saloon", IsActive = true },
                new { Id = 2, Name = "Comfort", Description = (string?)"Higher class vehicle with extra legroom", IsActive = true },
                new { Id = 3, Name = "Van", Description = (string?)"Six to eight seats for groups and luggage", IsActive = true },
                new { Id = 4, Name = "Premium", Description = (string?)"Executive vehicle for business rides", IsActive = true }
            );
        }

        private void SeedOrderStatuses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderStatus>().HasData(
                new { Id = (int)OrderStatusEnum.Pending, Name = nameof(OrderStatusEnum.Pending), Description = (string?)"Waiting for a driver to accept", SortOrder = 1, IsActive = true },
                new { Id = (int)OrderStatusEnum.Confirmed, Name = nameof(OrderStatusEnum.Confirmed), Description = (string?)"A driver accepted and is on the way", SortOrder = 2, IsActive = true },
                new { Id = (int)OrderStatusEnum.InProgress, Name = nameof(OrderStatusEnum.InProgress), Description = (string?)"Passenger picked up, ride underway", SortOrder = 3, IsActive = true },
                new { Id = (int)OrderStatusEnum.Completed, Name = nameof(OrderStatusEnum.Completed), Description = (string?)"Ride finished, payment follows", SortOrder = 4, IsActive = true },
                new { Id = (int)OrderStatusEnum.Cancelled, Name = nameof(OrderStatusEnum.Cancelled), Description = (string?)"Cancelled with a mandatory reason", SortOrder = 5, IsActive = true }
            );
        }

        private void SeedSupportTicketStatuses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupportTicketStatus>().HasData(
                new { Id = (int)SupportTicketStatusEnum.Open, Name = nameof(SupportTicketStatusEnum.Open), Description = (string?)"Submitted and awaiting review", SortOrder = 1, IsActive = true },
                new { Id = (int)SupportTicketStatusEnum.InProgress, Name = nameof(SupportTicketStatusEnum.InProgress), Description = (string?)"Being handled by an administrator", SortOrder = 2, IsActive = true },
                new { Id = (int)SupportTicketStatusEnum.Resolved, Name = nameof(SupportTicketStatusEnum.Resolved), Description = (string?)"Closed with a reply", SortOrder = 3, IsActive = true }
            );
        }

        private void SeedPaymentStatuses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentStatus>().HasData(
                new { Id = (int)PaymentStatusEnum.Unpaid, Name = nameof(PaymentStatusEnum.Unpaid), Description = (string?)"No successful charge recorded", SortOrder = 1, IsActive = true },
                new { Id = (int)PaymentStatusEnum.Paid, Name = nameof(PaymentStatusEnum.Paid), Description = (string?)"Confirmed by the payment provider webhook", SortOrder = 2, IsActive = true },
                new { Id = (int)PaymentStatusEnum.PartiallyRefunded, Name = nameof(PaymentStatusEnum.PartiallyRefunded), Description = (string?)"Part of the charged amount returned", SortOrder = 3, IsActive = true },
                new { Id = (int)PaymentStatusEnum.Refunded, Name = nameof(PaymentStatusEnum.Refunded), Description = (string?)"Full charged amount returned", SortOrder = 4, IsActive = true }
            );
        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new
                {
                    Id = 1,
                    Name = RoleNames.Admin,
                    Description = "Platform administrator",
                    IsActive = true,
                    CreatedAt = SeedTimestamp
                },
                new
                {
                    Id = 2,
                    Name = RoleNames.Company,
                    Description = "Taxi company operating drivers and vehicles",
                    IsActive = true,
                    CreatedAt = SeedTimestamp
                },
                new
                {
                    Id = 3,
                    Name = RoleNames.Driver,
                    Description = "Driver working for a taxi company",
                    IsActive = true,
                    CreatedAt = SeedTimestamp
                },
                new
                {
                    Id = 4,
                    Name = RoleNames.Passenger,
                    Description = "End user ordering rides",
                    IsActive = true,
                    CreatedAt = SeedTimestamp
                }
            );
        }

        private void SeedUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Admin",
                    Email = "admin1@pickmeup.local",
                    Username = "admin1",
                    PasswordHash = "w2ka8ahYEDcLTXF5tlMLcMfIjIA=",
                    PasswordSalt = "nlQ9pafmLDDgNQjCdCHDBA==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 2,
                    FirstName = "Bella",
                    LastName = "Company",
                    Email = "company1@pickmeup.local",
                    Username = "company1",
                    PasswordHash = "AwJYxEEx8ikPN8F8vgy2oKAcjvE=",
                    PasswordSalt = "Li1IhzglTwqEbHNNdr/PuQ==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 3,
                    FirstName = "Damir",
                    LastName = "Driver",
                    Email = "driver1@pickmeup.local",
                    Username = "driver1",
                    PasswordHash = "C2PEEH/Xup8N70SBm2JUe+0ZOgw=",
                    PasswordSalt = "JEexDUo3l4SMx/4mz4etmQ==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 4,
                    FirstName = "Petra",
                    LastName = "Passenger",
                    Email = "passenger1@pickmeup.local",
                    Username = "passenger1",
                    PasswordHash = "052xFoGxDl/+S7j6ADYmEJRJj1k=",
                    PasswordSalt = "QtT/dkd9xeip5+bSeaA+4A==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 5,
                    FirstName = "Nedim",
                    LastName = "Neretva",
                    Email = "company2@pickmeup.local",
                    Username = "company2",
                    PasswordHash = "AwJYxEEx8ikPN8F8vgy2oKAcjvE=",
                    PasswordSalt = "Li1IhzglTwqEbHNNdr/PuQ==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 6,
                    FirstName = "Selma",
                    LastName = "Express",
                    Email = "company3@pickmeup.local",
                    Username = "company3",
                    PasswordHash = "AwJYxEEx8ikPN8F8vgy2oKAcjvE=",
                    PasswordSalt = "Li1IhzglTwqEbHNNdr/PuQ==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 7,
                    FirstName = "Haris",
                    LastName = "Hodzic",
                    Email = "driver2@pickmeup.local",
                    Username = "driver2",
                    PasswordHash = "AwJYxEEx8ikPN8F8vgy2oKAcjvE=",
                    PasswordSalt = "Li1IhzglTwqEbHNNdr/PuQ==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 8,
                    FirstName = "Lejla",
                    LastName = "Kovac",
                    Email = "driver3@pickmeup.local",
                    Username = "driver3",
                    PasswordHash = "AwJYxEEx8ikPN8F8vgy2oKAcjvE=",
                    PasswordSalt = "Li1IhzglTwqEbHNNdr/PuQ==",
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                }
            );
        }

        private void SeedTaxiCompanies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaxiCompany>().HasData(
                new
                {
                    Id = 1,
                    Name = "Mostar Taxi",
                    HqAddress = "Bulevar 12",
                    CityId = 1,
                    ContactEmail = "info@mostartaxi.local",
                    ContactPhone = "+38736111222",
                    OwnerName = "Bella Company",
                    StartingPrice = 2.50m,
                    PricePerKm = 1.20m,
                    LogoImagePath = (string?)null,
                    UserId = 2,
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 2,
                    Name = "Neretva Prevoz",
                    HqAddress = "Kralja Tomislava 4",
                    CityId = 1,
                    ContactEmail = "info@neretvaprevoz.local",
                    ContactPhone = "+38736333444",
                    OwnerName = "Nedim Neretva",
                    StartingPrice = 2.00m,
                    PricePerKm = 1.35m,
                    LogoImagePath = (string?)null,
                    UserId = 5,
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 3,
                    Name = "Sarajevo Express",
                    HqAddress = "Zmaja od Bosne 88",
                    CityId = 2,
                    ContactEmail = "info@sarajevoexpress.local",
                    ContactPhone = "+38733555666",
                    OwnerName = "Selma Express",
                    StartingPrice = 3.00m,
                    PricePerKm = 1.10m,
                    LogoImagePath = (string?)null,
                    UserId = 6,
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                }
            );
        }

        private void SeedUserRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasData(
                new
                {
                    Id = 1,
                    UserId = 1,
                    RoleId = 1,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 2,
                    UserId = 2,
                    RoleId = 2,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 3,
                    UserId = 3,
                    RoleId = 3,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 4,
                    UserId = 4,
                    RoleId = 4,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 5,
                    UserId = 5,
                    RoleId = 2,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 6,
                    UserId = 6,
                    RoleId = 2,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 7,
                    UserId = 7,
                    RoleId = 3,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 8,
                    UserId = 8,
                    RoleId = 3,
                    DateAssigned = SeedTimestamp
                }
            );
        }
        private void SeedDrivers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Driver>().HasData(
                new { Id = 1, UserId = 3, TaxiCompanyId = 1, HireDate = SeedTimestamp, ResignedAt = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 2, UserId = 7, TaxiCompanyId = 1, HireDate = SeedTimestamp, ResignedAt = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 3, UserId = 8, TaxiCompanyId = 3, HireDate = SeedTimestamp, ResignedAt = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null }
            );
        }

        private void SeedCars(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>().HasData(
                new { Id = 1, Make = "Skoda", Model = "Octavia", Plate = "A12-T-345", Year = 2019, Seats = 4, VehicleCategoryId = 1, TaxiCompanyId = 1, PhotoImagePath = (string?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 2, Make = "Volkswagen", Model = "Passat", Plate = "A34-K-567", Year = 2021, Seats = 4, VehicleCategoryId = 2, TaxiCompanyId = 1, PhotoImagePath = (string?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 3, Make = "Ford", Model = "Tourneo", Plate = "A56-M-789", Year = 2020, Seats = 8, VehicleCategoryId = 3, TaxiCompanyId = 1, PhotoImagePath = (string?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 4, Make = "Skoda", Model = "Superb", Plate = "E56-M-789", Year = 2022, Seats = 4, VehicleCategoryId = 2, TaxiCompanyId = 2, PhotoImagePath = (string?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 5, Make = "Toyota", Model = "Corolla", Plate = "K78-J-012", Year = 2018, Seats = 4, VehicleCategoryId = 3, TaxiCompanyId = 3, PhotoImagePath = (string?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null }
            );
        }
    }
}
