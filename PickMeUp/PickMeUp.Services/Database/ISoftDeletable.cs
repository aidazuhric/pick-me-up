namespace PickMeUp.Services.Database
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }

        DateTime? DeletedAt { get; set; }
    }
}
