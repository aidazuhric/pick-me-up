namespace PickMeUp.Services.Storage
{
    public interface IImageFileStore
    {
        Task<string> SaveAsync(string area, int ownerId, Stream content, string fileName, string contentType);

        (Stream Content, string ContentType, string FileName) Open(string storedPath);
    }
}
