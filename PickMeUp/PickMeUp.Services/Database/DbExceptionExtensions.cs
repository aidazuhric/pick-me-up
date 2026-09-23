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

        public static string? ConstraintName(this DbUpdateException exception)
        {
            var inner = exception.InnerException;

            while (inner != null)
            {
                if (inner.GetType().GetProperty("ConstraintName")?.GetValue(inner) is string name
                    && !string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }

                inner = inner.InnerException;
            }

            return null;
        }

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
