using PickMeUp.Model.Constants;
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
            SeedCarAssignments(modelBuilder);
            SeedSupportTickets(modelBuilder);
            SeedSupportTicketReplies(modelBuilder);
            SeedOrders(modelBuilder);
            SeedOrderStatusHistories(modelBuilder);
            SeedReviews(modelBuilder);
            SeedInteractionHistories(modelBuilder);
            SeedShifts(modelBuilder);
            SeedShiftBreaks(modelBuilder);
            SeedPayments(modelBuilder);
            SeedPaymentRefunds(modelBuilder);
            SeedNotifications(modelBuilder);
        }

        private void SeedCities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().HasData(
                new { Id = 1, Name = "Mostar", PostalCode = (string?)"88000", IsActive = true, Latitude = 43.3438, Longitude = 17.8078 },
                new { Id = 2, Name = "Sarajevo", PostalCode = (string?)"71000", IsActive = true, Latitude = 43.8563, Longitude = 18.4131 },
                new { Id = 3, Name = "Banja Luka", PostalCode = (string?)"78000", IsActive = true, Latitude = 44.7722, Longitude = 17.1910 },
                new { Id = 4, Name = "Tuzla", PostalCode = (string?)"75000", IsActive = true, Latitude = 44.5384, Longitude = 18.6739 },
                new { Id = 5, Name = "Zenica", PostalCode = (string?)"72000", IsActive = true, Latitude = 44.2039, Longitude = 17.9066 }
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
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/1.png",
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
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/2.png",
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
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/3.jpg",
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
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/4.jpg",
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
                    TwoFactorMode = TwoFactorModeEnum.ConfirmOnce,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/5.png",
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
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/6.jpg",
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
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/7.jpg",
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
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)null,
                    ProfileImagePath = (string?)"user-photos/8.jpg",
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 9,
                    FirstName = "Amar",
                    LastName = "Selimovic",
                    Email = "passenger2@pickmeup.local",
                    Username = "passenger2",
                    PasswordHash = "56VoVY84Uv3DwxLzx4fwlU3IzrM=",
                    PasswordSalt = "Rk9wYmJ2MnhLd0RlTHRRUQ==",
                    IsActive = true,
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)"+38761234567",
                    ProfileImagePath = (string?)"user-photos/9.jpg",
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 10,
                    FirstName = "Ines",
                    LastName = "Maric",
                    Email = "passenger3@pickmeup.local",
                    Username = "passenger3",
                    PasswordHash = "pG3YU6itVrSMloQBg/eTd0NOfNo=",
                    PasswordSalt = "c1RtWnFYaDdOdkxrRWJVdA==",
                    IsActive = true,
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)"+38761765432",
                    ProfileImagePath = (string?)"user-photos/10.jpg",
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 11,
                    FirstName = "Kenan",
                    LastName = "Bosna",
                    Email = "company4@pickmeup.local",
                    Username = "company4",
                    PasswordHash = "VryTRHSCv/ouQq2T4UhDu7VG8ow=",
                    PasswordSalt = "WGpQdzRzTmhCcTJmVmRLYQ==",
                    IsActive = true,
                    TwoFactorMode = TwoFactorModeEnum.EveryLogin,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)"+38733555666",
                    ProfileImagePath = (string?)"user-photos/11.png",
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 12,
                    FirstName = "Tarik",
                    LastName = "Zovko",
                    Email = "driver4@pickmeup.local",
                    Username = "driver4",
                    PasswordHash = "NVrK8OXfEs43X/SeZ5ylkNrLeMg=",
                    PasswordSalt = "THBjNXZLZ1J3TjhtWXNFag==",
                    IsActive = true,
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)"+38736777888",
                    ProfileImagePath = (string?)"user-photos/12.jpg",
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 13,
                    FirstName = "Adna",
                    LastName = "Hadzic",
                    Email = "driver5@pickmeup.local",
                    Username = "driver5",
                    PasswordHash = "Yk+hY2PFplbc+iJLzo9GXPyQCXo=",
                    PasswordSalt = "QjdrTndSdFhoTTJ2Q3BZZA==",
                    IsActive = true,
                    TwoFactorMode = TwoFactorModeEnum.Off,
                    TwoFactorConfirmedAt = (DateTime?)null,
                    MustChangePassword = false,
                    CreatedAt = SeedTimestamp,
                    LastLoginAt = (DateTime?)null,
                    PhoneNumber = (string?)"+38733999000",
                    ProfileImagePath = (string?)"user-photos/13.png",
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
                    LogoImagePath = (string?)"company-logos/1.png",
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
                    LogoImagePath = (string?)"company-logos/2.png",
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
                    LogoImagePath = (string?)"company-logos/3.png",
                    UserId = 6,
                    IsActive = true,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 4,
                    Name = "Bascarsija Taxi",
                    HqAddress = "Mula Mustafe Baseskije 18",
                    CityId = 2,
                    ContactEmail = "info@bascarsijataxi.local",
                    ContactPhone = "+38733777999",
                    OwnerName = "Kenan Bosna",
                    StartingPrice = 2.80m,
                    PricePerKm = 1.15m,
                    LogoImagePath = (string?)"company-logos/4.jpg",
                    UserId = 11,
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
                },
                new
                {
                    Id = 9,
                    UserId = 9,
                    RoleId = 4,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 10,
                    UserId = 10,
                    RoleId = 4,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 11,
                    UserId = 11,
                    RoleId = 2,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 12,
                    UserId = 12,
                    RoleId = 3,
                    DateAssigned = SeedTimestamp
                },
                new
                {
                    Id = 13,
                    UserId = 13,
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
                new { Id = 3, UserId = 8, TaxiCompanyId = 3, HireDate = SeedTimestamp, ResignedAt = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 4, UserId = 12, TaxiCompanyId = 2, HireDate = SeedTimestamp, ResignedAt = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 5, UserId = 13, TaxiCompanyId = 4, HireDate = SeedTimestamp, ResignedAt = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null }
            );
        }

        private void SeedCars(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>().HasData(
                new { Id = 1, Make = "Skoda", Model = "Octavia", Plate = "A12-T-345", Year = 2019, Seats = 4, VehicleCategoryId = 1, TaxiCompanyId = 1, PhotoImagePath = (string?)"car-photos/1.jpg", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 2, Make = "Volkswagen", Model = "Passat", Plate = "A34-K-567", Year = 2021, Seats = 4, VehicleCategoryId = 2, TaxiCompanyId = 1, PhotoImagePath = (string?)"car-photos/2.jpg", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 3, Make = "Ford", Model = "Tourneo", Plate = "A56-M-789", Year = 2020, Seats = 8, VehicleCategoryId = 3, TaxiCompanyId = 1, PhotoImagePath = (string?)"car-photos/3.jpg", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 4, Make = "Skoda", Model = "Superb", Plate = "E56-M-789", Year = 2022, Seats = 4, VehicleCategoryId = 2, TaxiCompanyId = 2, PhotoImagePath = (string?)"car-photos/4.jpg", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 5, Make = "Toyota", Model = "Corolla", Plate = "K78-J-012", Year = 2018, Seats = 4, VehicleCategoryId = 3, TaxiCompanyId = 3, PhotoImagePath = (string?)"car-photos/5.jpg", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 6, Make = "Renault", Model = "Megane", Plate = "J12-A-678", Year = 2021, Seats = 4, VehicleCategoryId = 1, TaxiCompanyId = 4, PhotoImagePath = (string?)"car-photos/6.jpg", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null }
            );
        }

        private void SeedCarAssignments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CarAssignment>().HasData(
                new { Id = 1, CarId = 1, DriverId = 1, AssignedFrom = SeedTimestamp, AssignedTo = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 2, CarId = 2, DriverId = 2, AssignedFrom = SeedTimestamp, AssignedTo = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 3, CarId = 5, DriverId = 3, AssignedFrom = SeedTimestamp, AssignedTo = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 4, CarId = 4, DriverId = 4, AssignedFrom = SeedTimestamp, AssignedTo = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 5, CarId = 6, DriverId = 5, AssignedFrom = SeedTimestamp, AssignedTo = (DateTime?)null, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = (DateTime?)null, IsDeleted = false, DeletedAt = (DateTime?)null }
            );
        }

        private void SeedSupportTickets(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupportTicket>().HasData(
                new
                {
                    Id = 1,
                    SubmitterId = 4,
                    Subject = "The app signs me out during a ride",
                    Body = "Twice this week the app returned me to the login screen while my ride was still in progress.",
                    SupportTicketStatusId = (int)SupportTicketStatusEnum.Open,
                    TargetUserId = (int?)null,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)null,
                    ResolvedAt = (DateTime?)null,
                    ResolvedByUserId = (int?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 2,
                    SubmitterId = 3,
                    Subject = "I cannot start my shift",
                    Body = "Starting a shift fails with an error even though a car is assigned to me.",
                    SupportTicketStatusId = (int)SupportTicketStatusEnum.InProgress,
                    TargetUserId = (int?)null,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)SeedTimestamp,
                    ResolvedAt = (DateTime?)null,
                    ResolvedByUserId = (int?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 3,
                    SubmitterId = 4,
                    Subject = "Driver took a much longer route",
                    Body = "The driver drove across town for a trip that normally takes ten minutes and the fare was far higher than usual.",
                    SupportTicketStatusId = (int)SupportTicketStatusEnum.Resolved,
                    TargetUserId = (int?)3,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)SeedTimestamp,
                    ResolvedAt = (DateTime?)SeedTimestamp,
                    ResolvedByUserId = (int?)1,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 4,
                    SubmitterId = 2,
                    Subject = "Our company logo does not show in the app",
                    Body = "We uploaded a logo from the desktop app but passengers still see an empty placeholder.",
                    SupportTicketStatusId = (int)SupportTicketStatusEnum.Open,
                    TargetUserId = (int?)null,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = (DateTime?)null,
                    ResolvedAt = (DateTime?)null,
                    ResolvedByUserId = (int?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                }
            );
        }

        private void SeedSupportTicketReplies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SupportTicketReply>().HasData(
                new { Id = 1, SupportTicketId = 2, AuthorId = 1, Body = "Thanks for reporting this. We are checking your car assignment now.", CreatedAt = SeedTimestamp },
                new { Id = 2, SupportTicketId = 3, AuthorId = 1, Body = "We have pulled the route for this ride and are reviewing it with the company.", CreatedAt = SeedTimestamp },
                new { Id = 3, SupportTicketId = 3, AuthorId = 1, Body = "The difference has been refunded and the company has been notified.", CreatedAt = SeedTimestamp }
            );
        }

        private void SeedNotifications(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>().HasData(
                new { Id = 1, UserId = 4, Title = "Welcome to PickMeUp", Body = "Order your first ride straight from the home screen.", Category = NotificationCategories.OrderStatus, OrderId = (int?)null, IsRead = false, ReadAt = (DateTime?)null, CreatedAt = SeedTimestamp, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 2, UserId = 4, Title = "Payment method saved", Body = "Your card is ready, so paying for a ride now takes one tap.", Category = NotificationCategories.Payment, OrderId = (int?)null, IsRead = true, ReadAt = (DateTime?)SeedTimestamp, CreatedAt = SeedTimestamp, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 3, UserId = 4, Title = "A ride was cancelled", Body = "Reason: the driver could not reach the pick-up point.", Category = NotificationCategories.OrderCancelled, OrderId = (int?)null, IsRead = true, ReadAt = (DateTime?)SeedTimestamp, CreatedAt = SeedTimestamp, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 4, UserId = 3, Title = "New ride available", Body = "Pick-up at Kralja Tomislava 1.", Category = NotificationCategories.OrderAvailable, OrderId = (int?)null, IsRead = false, ReadAt = (DateTime?)null, CreatedAt = SeedTimestamp, IsDeleted = false, DeletedAt = (DateTime?)null },
                new { Id = 5, UserId = 3, Title = "Your shift summary is ready", Body = "You completed four rides on your last shift.", Category = NotificationCategories.OrderStatus, OrderId = (int?)null, IsRead = false, ReadAt = (DateTime?)null, CreatedAt = SeedTimestamp, IsDeleted = false, DeletedAt = (DateTime?)null }
            );
        }

        private void SeedOrders(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasData(
                new
                {
                    Id = 1,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)1,
                    CarId = (int?)1,
                    PickupLat = 43.3438,
                    PickupLng = 17.8078,
                    PickupAddress = "Kralja Tomislava 3",
                    DestinationLat = (double?)43.3502,
                    DestinationLng = (double?)17.814,
                    DestinationAddress = (string?)"Bulevar 22",
                    DistanceKm = (decimal?)3.2m,
                    Fare = (decimal?)6.34m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 6, 11, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 6, 11, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 6, 11, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 6, 11, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 11, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 2,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)1,
                    CarId = (int?)1,
                    PickupLat = 43.3401,
                    PickupLng = 17.8135,
                    PickupAddress = "Marsala Tita 40",
                    DestinationLat = (double?)43.3489,
                    DestinationLng = (double?)17.7995,
                    DestinationAddress = (string?)"Rondo 1",
                    DistanceKm = (decimal?)5.6m,
                    Fare = (decimal?)9.22m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 6, 18, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 6, 18, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 6, 18, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 6, 18, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 18, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 3,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)2,
                    CarId = (int?)2,
                    PickupLat = 43.335,
                    PickupLng = 17.8021,
                    PickupAddress = "Splitska 5",
                    DestinationLat = (double?)43.2829,
                    DestinationLng = (double?)17.8459,
                    DestinationAddress = (string?)"Aerodrom Mostar",
                    DistanceKm = (decimal?)8.4m,
                    Fare = (decimal?)12.58m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 6, 25, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 6, 25, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 6, 25, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 6, 25, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 25, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 4,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)1,
                    CarId = (int?)1,
                    PickupLat = 43.3502,
                    PickupLng = 17.814,
                    PickupAddress = "Bulevar 22",
                    DestinationLat = (double?)43.3374,
                    DestinationLng = (double?)17.8151,
                    DestinationAddress = (string?)"Stari Most",
                    DistanceKm = (decimal?)4.1m,
                    Fare = (decimal?)7.42m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 7, 2, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 7, 2, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 7, 2, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 2, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 5,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)2,
                    CarId = (int?)2,
                    PickupLat = 43.3477,
                    PickupLng = 17.7961,
                    PickupAddress = "Kneza Domagoja 12",
                    DestinationLat = (double?)43.3419,
                    DestinationLng = (double?)17.8043,
                    DestinationAddress = (string?)"Kampus Sveucilista",
                    DistanceKm = (decimal?)6.9m,
                    Fare = (decimal?)10.78m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 7, 9, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 7, 9, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 7, 9, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 9, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 6,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)2,
                    DriverId = (int?)4,
                    CarId = (int?)4,
                    PickupLat = 43.339,
                    PickupLng = 17.8158,
                    PickupAddress = "Fejiceva 9",
                    DestinationLat = (double?)43.348,
                    DestinationLng = (double?)17.8085,
                    DestinationAddress = (string?)"Zeljeznicka stanica",
                    DistanceKm = (decimal?)5.0m,
                    Fare = (decimal?)8.75m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 6, 14, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 6, 14, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 6, 14, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 6, 14, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 14, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 7,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)2,
                    DriverId = (int?)4,
                    CarId = (int?)4,
                    PickupLat = 43.3489,
                    PickupLng = 17.7995,
                    PickupAddress = "Rondo 1",
                    DestinationLat = (double?)43.257,
                    DestinationLng = (double?)17.889,
                    DestinationAddress = (string?)"Blagaj",
                    DistanceKm = (decimal?)7.3m,
                    Fare = (decimal?)11.86m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 7, 16, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 7, 16, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 7, 16, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 16, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 8,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)3,
                    DriverId = (int?)3,
                    CarId = (int?)5,
                    PickupLat = 43.859,
                    PickupLng = 18.421,
                    PickupAddress = "Ferhadija 10",
                    DestinationLat = (double?)43.856,
                    DestinationLng = (double?)18.411,
                    DestinationAddress = (string?)"Skenderija",
                    DistanceKm = (decimal?)4.5m,
                    Fare = (decimal?)7.95m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 6, 12, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 6, 12, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 6, 12, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 6, 12, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 12, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 9,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)3,
                    DriverId = (int?)3,
                    CarId = (int?)5,
                    PickupLat = 43.8595,
                    PickupLng = 18.431,
                    PickupAddress = "Bascarsija",
                    DestinationLat = (double?)43.828,
                    DestinationLng = (double?)18.31,
                    DestinationAddress = (string?)"Ilidza",
                    DistanceKm = (decimal?)9.2m,
                    Fare = (decimal?)13.12m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 6, 20, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 6, 20, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 6, 20, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 6, 20, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 20, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 10,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)3,
                    DriverId = (int?)3,
                    CarId = (int?)5,
                    PickupLat = 43.856,
                    PickupLng = 18.403,
                    PickupAddress = "Marijin Dvor",
                    DestinationLat = (double?)43.9,
                    DestinationLng = (double?)18.343,
                    DestinationAddress = (string?)"Vogosca",
                    DistanceKm = (decimal?)6.1m,
                    Fare = (decimal?)9.71m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 7, 4, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 7, 4, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 7, 4, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 7, 4, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 4, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 11,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)3,
                    DriverId = (int?)3,
                    CarId = (int?)5,
                    PickupLat = 43.848,
                    PickupLng = 18.395,
                    PickupAddress = "Grbavica",
                    DestinationLat = (double?)43.868,
                    DestinationLng = (double?)18.413,
                    DestinationAddress = (string?)"Kosevo",
                    DistanceKm = (decimal?)3.8m,
                    Fare = (decimal?)7.18m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 7, 11, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 7, 11, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 7, 11, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 11, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 12,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)4,
                    DriverId = (int?)5,
                    CarId = (int?)6,
                    PickupLat = 43.8615,
                    PickupLng = 18.4055,
                    PickupAddress = "Alipasina 14",
                    DestinationLat = (double?)43.823,
                    DestinationLng = (double?)18.339,
                    DestinationAddress = (string?)"Dobrinja",
                    DistanceKm = (decimal?)5.4m,
                    Fare = (decimal?)9.01m,
                    OrderStatusId = (int)OrderStatusEnum.Completed,
                    CreatedAt = new DateTime(2026, 6, 28, 9, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 6, 28, 9, 5, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 6, 28, 9, 12, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)new DateTime(2026, 6, 28, 9, 35, 0, DateTimeKind.Utc),
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 28, 9, 35, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 13,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)null,
                    CarId = (int?)null,
                    PickupLat = 43.3489,
                    PickupLng = 17.7995,
                    PickupAddress = "Rondo 1",
                    DestinationLat = (double?)43.3419,
                    DestinationLng = (double?)17.8043,
                    DestinationAddress = (string?)"Kampus Sveucilista",
                    DistanceKm = (decimal?)5.50m,
                    Fare = (decimal?)null,
                    OrderStatusId = (int)OrderStatusEnum.Pending,
                    CreatedAt = new DateTime(2026, 8, 3, 8, 30, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)null,
                    PickedUpAt = (DateTime?)null,
                    CompletedAt = (DateTime?)null,
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 14,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)3,
                    DriverId = (int?)null,
                    CarId = (int?)null,
                    PickupLat = 43.859,
                    PickupLng = 18.421,
                    PickupAddress = "Ferhadija 10",
                    DestinationLat = (double?)43.856,
                    DestinationLng = (double?)18.403,
                    DestinationAddress = (string?)"Marijin Dvor",
                    DistanceKm = (decimal?)3.90m,
                    Fare = (decimal?)null,
                    OrderStatusId = (int)OrderStatusEnum.Pending,
                    CreatedAt = new DateTime(2026, 8, 4, 17, 10, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)null,
                    PickedUpAt = (DateTime?)null,
                    CompletedAt = (DateTime?)null,
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 15,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)1,
                    CarId = (int?)1,
                    PickupLat = 43.335,
                    PickupLng = 17.8021,
                    PickupAddress = "Splitska 5",
                    DestinationLat = (double?)43.3374,
                    DestinationLng = (double?)17.8151,
                    DestinationAddress = (string?)"Stari Most",
                    DistanceKm = (decimal?)6.70m,
                    Fare = (decimal?)null,
                    OrderStatusId = (int)OrderStatusEnum.Confirmed,
                    CreatedAt = new DateTime(2026, 8, 5, 7, 45, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 8, 5, 7, 50, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)null,
                    CompletedAt = (DateTime?)null,
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 8, 5, 7, 50, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 16,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)3,
                    DriverId = (int?)3,
                    CarId = (int?)5,
                    PickupLat = 43.8595,
                    PickupLng = 18.431,
                    PickupAddress = "Bascarsija",
                    DestinationLat = (double?)43.828,
                    DestinationLng = (double?)18.31,
                    DestinationAddress = (string?)"Ilidza",
                    DistanceKm = (decimal?)8.20m,
                    Fare = (decimal?)null,
                    OrderStatusId = (int)OrderStatusEnum.InProgress,
                    CreatedAt = new DateTime(2026, 8, 5, 18, 5, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 8, 5, 18, 9, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)new DateTime(2026, 8, 5, 18, 16, 0, DateTimeKind.Utc),
                    CompletedAt = (DateTime?)null,
                    CancelledAt = (DateTime?)null,
                    CancellationReason = (string?)null,
                    UpdatedAt = (DateTime?)new DateTime(2026, 8, 5, 18, 16, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 17,
                    PassengerId = 4,
                    TaxiCompanyId = (int?)1,
                    DriverId = (int?)null,
                    CarId = (int?)null,
                    PickupLat = 43.3401,
                    PickupLng = 17.8135,
                    PickupAddress = "Marsala Tita 40",
                    DestinationLat = (double?)43.348,
                    DestinationLng = (double?)17.8085,
                    DestinationAddress = (string?)"Zeljeznicka stanica",
                    DistanceKm = (decimal?)4.40m,
                    Fare = (decimal?)null,
                    OrderStatusId = (int)OrderStatusEnum.Cancelled,
                    CreatedAt = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)null,
                    PickedUpAt = (DateTime?)null,
                    CompletedAt = (DateTime?)null,
                    CancelledAt = (DateTime?)new DateTime(2026, 7, 20, 12, 6, 0, DateTimeKind.Utc),
                    CancellationReason = (string?)"Passenger found another ride before a driver accepted.",
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 20, 12, 6, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 18,
                    PassengerId = 9,
                    TaxiCompanyId = (int?)3,
                    DriverId = (int?)3,
                    CarId = (int?)5,
                    PickupLat = 43.848,
                    PickupLng = 18.395,
                    PickupAddress = "Grbavica",
                    DestinationLat = (double?)43.9,
                    DestinationLng = (double?)18.343,
                    DestinationAddress = (string?)"Vogosca",
                    DistanceKm = (decimal?)6.10m,
                    Fare = (decimal?)null,
                    OrderStatusId = (int)OrderStatusEnum.Cancelled,
                    CreatedAt = new DateTime(2026, 7, 22, 9, 15, 0, DateTimeKind.Utc),
                    AcceptedAt = (DateTime?)new DateTime(2026, 7, 22, 9, 19, 0, DateTimeKind.Utc),
                    PickedUpAt = (DateTime?)null,
                    CompletedAt = (DateTime?)null,
                    CancelledAt = (DateTime?)new DateTime(2026, 7, 22, 9, 31, 0, DateTimeKind.Utc),
                    CancellationReason = (string?)"Driver could not reach the pick-up point because of a closed road.",
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 22, 9, 31, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                }
            );
        }

        private void SeedOrderStatusHistories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderStatusHistory>().HasData(
                new { Id = 1, OrderId = 1, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 6, 11, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 2, OrderId = 1, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 6, 11, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 3, OrderId = 1, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 6, 11, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 4, OrderId = 1, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 6, 11, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 5, OrderId = 2, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 6, 18, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 6, OrderId = 2, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 6, 18, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 7, OrderId = 2, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 6, 18, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 8, OrderId = 2, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 6, 18, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 9, OrderId = 3, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 6, 25, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 10, OrderId = 3, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 7, ChangedAt = new DateTime(2026, 6, 25, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 11, OrderId = 3, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 7, ChangedAt = new DateTime(2026, 6, 25, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 12, OrderId = 3, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 7, ChangedAt = new DateTime(2026, 6, 25, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 13, OrderId = 4, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 14, OrderId = 4, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 7, 2, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 15, OrderId = 4, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 7, 2, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 16, OrderId = 4, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 7, 2, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 17, OrderId = 5, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 18, OrderId = 5, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 7, ChangedAt = new DateTime(2026, 7, 9, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 19, OrderId = 5, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 7, ChangedAt = new DateTime(2026, 7, 9, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 20, OrderId = 5, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 7, ChangedAt = new DateTime(2026, 7, 9, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 21, OrderId = 6, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 6, 14, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 22, OrderId = 6, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 12, ChangedAt = new DateTime(2026, 6, 14, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 23, OrderId = 6, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 12, ChangedAt = new DateTime(2026, 6, 14, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 24, OrderId = 6, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 12, ChangedAt = new DateTime(2026, 6, 14, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 25, OrderId = 7, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 26, OrderId = 7, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 12, ChangedAt = new DateTime(2026, 7, 16, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 27, OrderId = 7, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 12, ChangedAt = new DateTime(2026, 7, 16, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 28, OrderId = 7, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 12, ChangedAt = new DateTime(2026, 7, 16, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 29, OrderId = 8, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 6, 12, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 30, OrderId = 8, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 6, 12, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 31, OrderId = 8, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 6, 12, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 32, OrderId = 8, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 6, 12, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 33, OrderId = 9, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 6, 20, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 34, OrderId = 9, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 6, 20, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 35, OrderId = 9, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 6, 20, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 36, OrderId = 9, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 6, 20, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 37, OrderId = 10, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 7, 4, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 38, OrderId = 10, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 4, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 39, OrderId = 10, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 4, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 40, OrderId = 10, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 4, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 41, OrderId = 11, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 42, OrderId = 11, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 11, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 43, OrderId = 11, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 11, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 44, OrderId = 11, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 11, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 45, OrderId = 12, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 6, 28, 9, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 46, OrderId = 12, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 13, ChangedAt = new DateTime(2026, 6, 28, 9, 5, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 47, OrderId = 12, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 13, ChangedAt = new DateTime(2026, 6, 28, 9, 12, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 48, OrderId = 12, FromStatusId = (int?)(int)OrderStatusEnum.InProgress, ToStatusId = (int)OrderStatusEnum.Completed, ChangedByUserId = 13, ChangedAt = new DateTime(2026, 6, 28, 9, 35, 0, DateTimeKind.Utc), Reason = (string?)"Ride completed" },
                new { Id = 49, OrderId = 13, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 8, 3, 8, 30, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 50, OrderId = 14, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 8, 4, 17, 10, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 51, OrderId = 15, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 8, 5, 7, 45, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 52, OrderId = 15, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 3, ChangedAt = new DateTime(2026, 8, 5, 7, 50, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 53, OrderId = 16, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 8, 5, 18, 5, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 54, OrderId = 16, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 8, 5, 18, 9, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 55, OrderId = 16, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.InProgress, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 8, 5, 18, 16, 0, DateTimeKind.Utc), Reason = (string?)"Passenger picked up" },
                new { Id = 56, OrderId = 17, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 57, OrderId = 17, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Cancelled, ChangedByUserId = 4, ChangedAt = new DateTime(2026, 7, 20, 12, 6, 0, DateTimeKind.Utc), Reason = (string?)"Passenger found another ride before a driver accepted." },
                new { Id = 58, OrderId = 18, FromStatusId = (int?)null, ToStatusId = (int)OrderStatusEnum.Pending, ChangedByUserId = 9, ChangedAt = new DateTime(2026, 7, 22, 9, 15, 0, DateTimeKind.Utc), Reason = (string?)"Order placed" },
                new { Id = 59, OrderId = 18, FromStatusId = (int?)(int)OrderStatusEnum.Pending, ToStatusId = (int)OrderStatusEnum.Confirmed, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 22, 9, 19, 0, DateTimeKind.Utc), Reason = (string?)"Accepted by driver" },
                new { Id = 60, OrderId = 18, FromStatusId = (int?)(int)OrderStatusEnum.Confirmed, ToStatusId = (int)OrderStatusEnum.Cancelled, ChangedByUserId = 8, ChangedAt = new DateTime(2026, 7, 22, 9, 31, 0, DateTimeKind.Utc), Reason = (string?)"Driver could not reach the pick-up point because of a closed road." }
            );
        }

        private void SeedReviews(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review>().HasData(
                new
                {
                    Id = 1,
                    OrderId = 1,
                    PassengerId = 4,
                    TargetCompanyId = (int?)1,
                    TargetDriverId = (int?)1,
                    Rating = 5,
                    Comment = (string?)"Quick pickup and a friendly driver.",
                    CreatedAt = new DateTime(2026, 6, 11, 10, 35, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 2,
                    OrderId = 2,
                    PassengerId = 4,
                    TargetCompanyId = (int?)1,
                    TargetDriverId = (int?)1,
                    Rating = 5,
                    Comment = (string?)"Always on time, never had a problem.",
                    CreatedAt = new DateTime(2026, 6, 18, 10, 35, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 3,
                    OrderId = 3,
                    PassengerId = 4,
                    TargetCompanyId = (int?)1,
                    TargetDriverId = (int?)2,
                    Rating = 4,
                    Comment = (string?)"Comfortable ride, arrived a few minutes late.",
                    CreatedAt = new DateTime(2026, 6, 25, 10, 35, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 4,
                    OrderId = 6,
                    PassengerId = 4,
                    TargetCompanyId = (int?)2,
                    TargetDriverId = (int?)4,
                    Rating = 2,
                    Comment = (string?)"Waited far too long for the car to show up.",
                    CreatedAt = new DateTime(2026, 6, 14, 10, 35, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 5,
                    OrderId = 8,
                    PassengerId = 9,
                    TargetCompanyId = (int?)3,
                    TargetDriverId = (int?)3,
                    Rating = 4,
                    Comment = (string?)"Good driver and a clean car.",
                    CreatedAt = new DateTime(2026, 6, 12, 10, 35, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 6,
                    OrderId = 9,
                    PassengerId = 9,
                    TargetCompanyId = (int?)3,
                    TargetDriverId = (int?)3,
                    Rating = 5,
                    Comment = (string?)"Excellent from start to finish.",
                    CreatedAt = new DateTime(2026, 6, 20, 10, 35, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 7,
                    OrderId = 12,
                    PassengerId = 9,
                    TargetCompanyId = (int?)4,
                    TargetDriverId = (int?)5,
                    Rating = 3,
                    Comment = (string?)"Fine ride, but the fare felt high for the distance.",
                    CreatedAt = new DateTime(2026, 6, 28, 10, 35, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                }
            );
        }

        private void SeedInteractionHistories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InteractionHistory>().HasData(
                new { Id = 1, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)1, CreatedAt = new DateTime(2026, 6, 11, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 2, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)1, CreatedAt = new DateTime(2026, 6, 11, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 3, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)2, CreatedAt = new DateTime(2026, 6, 18, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 4, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)2, CreatedAt = new DateTime(2026, 6, 18, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 5, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)3, CreatedAt = new DateTime(2026, 6, 25, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 6, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)3, CreatedAt = new DateTime(2026, 6, 25, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 7, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)4, CreatedAt = new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 8, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)4, CreatedAt = new DateTime(2026, 7, 2, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 9, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)5, CreatedAt = new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 10, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)5, CreatedAt = new DateTime(2026, 7, 9, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 11, UserId = 4, TaxiCompanyId = 2, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)6, CreatedAt = new DateTime(2026, 6, 14, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 12, UserId = 4, TaxiCompanyId = 2, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)6, CreatedAt = new DateTime(2026, 6, 14, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 13, UserId = 4, TaxiCompanyId = 2, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)7, CreatedAt = new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 14, UserId = 4, TaxiCompanyId = 2, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)7, CreatedAt = new DateTime(2026, 7, 16, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 15, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)8, CreatedAt = new DateTime(2026, 6, 12, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 16, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)8, CreatedAt = new DateTime(2026, 6, 12, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 17, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)9, CreatedAt = new DateTime(2026, 6, 20, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 18, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)9, CreatedAt = new DateTime(2026, 6, 20, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 19, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)10, CreatedAt = new DateTime(2026, 7, 4, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 20, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)10, CreatedAt = new DateTime(2026, 7, 4, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 21, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)11, CreatedAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 22, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)11, CreatedAt = new DateTime(2026, 7, 11, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 23, UserId = 9, TaxiCompanyId = 4, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)12, CreatedAt = new DateTime(2026, 6, 28, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = 24, UserId = 9, TaxiCompanyId = 4, SignalTypeId = (int)InteractionSignalType.Completed, Value = 1m, OrderId = (int?)12, CreatedAt = new DateTime(2026, 6, 28, 9, 35, 0, DateTimeKind.Utc) },
                new { Id = 25, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Reviewed, Value = 5m, OrderId = (int?)1, CreatedAt = new DateTime(2026, 6, 11, 10, 35, 0, DateTimeKind.Utc) },
                new { Id = 26, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Reviewed, Value = 5m, OrderId = (int?)2, CreatedAt = new DateTime(2026, 6, 18, 10, 35, 0, DateTimeKind.Utc) },
                new { Id = 27, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Reviewed, Value = 4m, OrderId = (int?)3, CreatedAt = new DateTime(2026, 6, 25, 10, 35, 0, DateTimeKind.Utc) },
                new { Id = 28, UserId = 4, TaxiCompanyId = 2, SignalTypeId = (int)InteractionSignalType.Reviewed, Value = 2m, OrderId = (int?)6, CreatedAt = new DateTime(2026, 6, 14, 10, 35, 0, DateTimeKind.Utc) },
                new { Id = 29, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Reviewed, Value = 4m, OrderId = (int?)8, CreatedAt = new DateTime(2026, 6, 12, 10, 35, 0, DateTimeKind.Utc) },
                new { Id = 30, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Reviewed, Value = 5m, OrderId = (int?)9, CreatedAt = new DateTime(2026, 6, 20, 10, 35, 0, DateTimeKind.Utc) },
                new { Id = 31, UserId = 9, TaxiCompanyId = 4, SignalTypeId = (int)InteractionSignalType.Reviewed, Value = 3m, OrderId = (int?)12, CreatedAt = new DateTime(2026, 6, 28, 10, 35, 0, DateTimeKind.Utc) },
                new { Id = 32, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)13, CreatedAt = new DateTime(2026, 8, 3, 8, 30, 0, DateTimeKind.Utc) },
                new { Id = 33, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)14, CreatedAt = new DateTime(2026, 8, 4, 17, 10, 0, DateTimeKind.Utc) },
                new { Id = 34, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)15, CreatedAt = new DateTime(2026, 8, 5, 7, 45, 0, DateTimeKind.Utc) },
                new { Id = 35, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)16, CreatedAt = new DateTime(2026, 8, 5, 18, 5, 0, DateTimeKind.Utc) },
                new { Id = 36, UserId = 4, TaxiCompanyId = 1, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)17, CreatedAt = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc) },
                new { Id = 37, UserId = 9, TaxiCompanyId = 3, SignalTypeId = (int)InteractionSignalType.Ordered, Value = 1m, OrderId = (int?)18, CreatedAt = new DateTime(2026, 7, 22, 9, 15, 0, DateTimeKind.Utc) }
            );
        }

        private void SeedPayments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>().HasData(
                new
                {
                    Id = 1,
                    OrderId = 1,
                    AmountCharged = 6.34m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Paid,
                    StripePaymentIntentId = (string?)"pi_seed_001",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 6, 11, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 6, 11, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 2,
                    OrderId = 2,
                    AmountCharged = 9.22m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Paid,
                    StripePaymentIntentId = (string?)"pi_seed_002",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 6, 18, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 6, 18, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 3,
                    OrderId = 3,
                    AmountCharged = 12.58m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.PartiallyRefunded,
                    StripePaymentIntentId = (string?)"pi_seed_003",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 6, 25, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 6, 25, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 4,
                    OrderId = 4,
                    AmountCharged = 7.42m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Refunded,
                    StripePaymentIntentId = (string?)"pi_seed_004",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 7, 2, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 7, 2, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 5,
                    OrderId = 5,
                    AmountCharged = 10.78m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Unpaid,
                    StripePaymentIntentId = (string?)"pi_seed_005",
                    IsPaid = false,
                    CreatedAt = new DateTime(2026, 7, 9, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)null,
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 6,
                    OrderId = 6,
                    AmountCharged = 8.75m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Paid,
                    StripePaymentIntentId = (string?)"pi_seed_006",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 6, 14, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 6, 14, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 7,
                    OrderId = 8,
                    AmountCharged = 7.95m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Paid,
                    StripePaymentIntentId = (string?)"pi_seed_008",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 6, 12, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 6, 12, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 8,
                    OrderId = 9,
                    AmountCharged = 13.12m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Paid,
                    StripePaymentIntentId = (string?)"pi_seed_009",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 6, 20, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 6, 20, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 9,
                    OrderId = 10,
                    AmountCharged = 9.71m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Paid,
                    StripePaymentIntentId = (string?)"pi_seed_010",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 7, 4, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 7, 4, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 10,
                    OrderId = 11,
                    AmountCharged = 7.18m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.PartiallyRefunded,
                    StripePaymentIntentId = (string?)"pi_seed_011",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 7, 11, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 7, 11, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 11,
                    OrderId = 12,
                    AmountCharged = 9.01m,
                    Currency = "BAM",
                    PaymentStatusId = (int)PaymentStatusEnum.Refunded,
                    StripePaymentIntentId = (string?)"pi_seed_012",
                    IsPaid = true,
                    CreatedAt = new DateTime(2026, 6, 28, 9, 40, 0, DateTimeKind.Utc),
                    PaidAt = (DateTime?)new DateTime(2026, 6, 28, 10, 5, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                }
            );
        }

        private void SeedPaymentRefunds(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentRefund>().HasData(
                new { Id = 1, PaymentId = 10, RefundedAmount = 2.18m, StripeRefundId = (string?)"re_seed_010", Reason = (string?)"Waiting time was charged in error and returned to the passenger.", RefundedAt = new DateTime(2026, 7, 12, 8, 20, 0, DateTimeKind.Utc) },
                new { Id = 2, PaymentId = 11, RefundedAmount = 9.01m, StripeRefundId = (string?)"re_seed_011", Reason = (string?)"The ride was billed twice, so the duplicate charge was returned in full.", RefundedAt = new DateTime(2026, 6, 29, 8, 20, 0, DateTimeKind.Utc) },
                new { Id = 3, PaymentId = 3, RefundedAmount = 3.58m, StripeRefundId = (string?)"re_seed_003", Reason = (string?)"The passenger was charged for a longer route than the one driven.", RefundedAt = new DateTime(2026, 6, 26, 9, 15, 0, DateTimeKind.Utc) },
                new { Id = 4, PaymentId = 4, RefundedAmount = 7.42m, StripeRefundId = (string?)"re_seed_004", Reason = (string?)"The ride was cut short by a breakdown, so the full fare was returned.", RefundedAt = new DateTime(2026, 7, 3, 8, 40, 0, DateTimeKind.Utc) }
            );
        }

        private void SeedShifts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Shift>().HasData(
                new
                {
                    Id = 1,
                    DriverId = 1,
                    StartedAt = new DateTime(2026, 6, 11, 7, 30, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 6, 11, 15, 30, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 6, 11, 7, 30, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 11, 15, 30, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 2,
                    DriverId = 1,
                    StartedAt = new DateTime(2026, 6, 18, 7, 30, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 6, 18, 15, 30, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 6, 18, 7, 30, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 18, 15, 30, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 3,
                    DriverId = 1,
                    StartedAt = new DateTime(2026, 7, 2, 7, 30, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 7, 2, 16, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 7, 2, 7, 30, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 2, 16, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 4,
                    DriverId = 2,
                    StartedAt = new DateTime(2026, 6, 25, 8, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 6, 25, 16, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 6, 25, 8, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 25, 16, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 5,
                    DriverId = 2,
                    StartedAt = new DateTime(2026, 7, 9, 8, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 7, 9, 16, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 7, 9, 8, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 9, 16, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 6,
                    DriverId = 4,
                    StartedAt = new DateTime(2026, 6, 14, 9, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 6, 14, 17, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 6, 14, 9, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 14, 17, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 7,
                    DriverId = 4,
                    StartedAt = new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 7, 16, 17, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 16, 17, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 8,
                    DriverId = 3,
                    StartedAt = new DateTime(2026, 6, 12, 7, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 6, 12, 15, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 6, 12, 7, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 12, 15, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 9,
                    DriverId = 3,
                    StartedAt = new DateTime(2026, 6, 20, 7, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 6, 20, 15, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 6, 20, 7, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 20, 15, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 10,
                    DriverId = 3,
                    StartedAt = new DateTime(2026, 7, 4, 7, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 7, 4, 15, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 7, 4, 7, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 4, 15, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 11,
                    DriverId = 3,
                    StartedAt = new DateTime(2026, 7, 11, 7, 0, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 7, 11, 15, 0, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 7, 11, 7, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 7, 11, 15, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 12,
                    DriverId = 5,
                    StartedAt = new DateTime(2026, 6, 28, 8, 30, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)new DateTime(2026, 6, 28, 16, 30, 0, DateTimeKind.Utc),
                    OrdersHandled = 1,
                    CreatedAt = new DateTime(2026, 6, 28, 8, 30, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)new DateTime(2026, 6, 28, 16, 30, 0, DateTimeKind.Utc),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                },
                new
                {
                    Id = 13,
                    DriverId = 1,
                    StartedAt = new DateTime(2026, 8, 6, 6, 45, 0, DateTimeKind.Utc),
                    EndedAt = (DateTime?)null,
                    OrdersHandled = 0,
                    CreatedAt = new DateTime(2026, 8, 6, 6, 45, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null,
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null
                }
            );
        }

        private void SeedShiftBreaks(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShiftBreak>().HasData(
                new { Id = 1, ShiftId = 2, StartedAt = new DateTime(2026, 6, 18, 11, 0, 0, DateTimeKind.Utc), EndedAt = (DateTime?)new DateTime(2026, 6, 18, 11, 30, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2026, 6, 18, 11, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)new DateTime(2026, 6, 18, 11, 30, 0, DateTimeKind.Utc) },
                new { Id = 2, ShiftId = 4, StartedAt = new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc), EndedAt = (DateTime?)new DateTime(2026, 6, 25, 12, 45, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)new DateTime(2026, 6, 25, 12, 45, 0, DateTimeKind.Utc) },
                new { Id = 3, ShiftId = 8, StartedAt = new DateTime(2026, 6, 12, 10, 30, 0, DateTimeKind.Utc), EndedAt = (DateTime?)new DateTime(2026, 6, 12, 11, 0, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2026, 6, 12, 10, 30, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)new DateTime(2026, 6, 12, 11, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
