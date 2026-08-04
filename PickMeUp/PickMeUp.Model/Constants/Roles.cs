namespace PickMeUp.Model.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Company = "Company";
        public const string Driver = "Driver";
        public const string Passenger = "Passenger";

        public static readonly string[] All = { Admin, Company, Driver, Passenger };
    }
}
