using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services.Database
{
    public static class DbExceptionExtensions
    {
        private const string ForeignKeyViolation = "23503";
        private const string UniqueViolation = "23505";

        public static bool IsForeignKeyViolation(this DbUpdateException exception) =>
            HasSqlState(exception, ForeignKeyViolation);

        public static bool IsUniqueViolation(this DbUpdateException exception) =>
            HasSqlState(exception, UniqueViolation);

        private static bool HasSqlState(DbUpdateException exception, string sqlState)
        {
            var inner = exception.InnerException;

            while (inner != null)
            {
                if (inner.GetType().GetProperty("SqlState")?.GetValue(inner) as string == sqlState)
                {
                    return true;
                }

                inner = inner.InnerException;
            }

            return false;
        }
    }
}
