namespace PickMeUp.Services.Database
{
    public static class PostgresFunctions
    {
        public static DateTime DateTrunc(string field, DateTime source) =>
            throw new NotSupportedException("date_trunc is only available inside a database query.");
    }
}
