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
                }
            );
        }
    }
}
