using PickMeUp.Common.Services.Configuration;
using PickMeUp.Model.Exceptions;
using Microsoft.Extensions.Options;

namespace PickMeUp.Services.Storage
{
    public class ImageFileStore : IImageFileStore
    {
        private const long MaxBytes = 2 * 1024 * 1024;

        private static readonly Dictionary<string, ImageFormat> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = new ImageFormat(".jpg", new byte[] { 0xFF, 0xD8, 0xFF }),
            ["image/png"] = new ImageFormat(".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })
        };

        private readonly string _rootPath;

        public ImageFileStore(IOptions<FileStorageOptions> options)
        {
            _rootPath = options.Value.RootPath;
        }

        public async Task<string> SaveAsync(string area, int ownerId, Stream content, string fileName, string contentType)
        {
            if (!SupportedFormats.TryGetValue(contentType ?? string.Empty, out var format))
            {
                throw new BusinessException("Only JPEG and PNG images are accepted.");
            }

            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer);

            if (buffer.Length == 0)
            {
                throw new BusinessException("The uploaded file is empty.");
            }

            if (buffer.Length > MaxBytes)
            {
                throw new BusinessException("The image must be 2 MB or smaller.");
            }

            var bytes = buffer.ToArray();

            if (!StartsWithSignature(bytes, format.Signature))
            {
                throw new BusinessException(
                    "The file contents do not match the declared image type. Upload a genuine JPEG or PNG image.");
            }

            var directory = Path.Combine(_rootPath, area);
            Directory.CreateDirectory(directory);

            var storedName = $"{ownerId}{format.Extension}";
            var absolutePath = Path.Combine(directory, storedName);

            await File.WriteAllBytesAsync(absolutePath, bytes);

            return Path.Combine(area, storedName);
        }

        public (Stream Content, string ContentType, string FileName) Open(string storedPath)
        {
            var absolutePath = Path.Combine(_rootPath, storedPath);

            if (!IsInsideRoot(absolutePath) || !File.Exists(absolutePath))
            {
                throw new KeyNotFoundException("The stored image could not be found.");
            }

            var extension = Path.GetExtension(absolutePath);
            var contentType = SupportedFormats
                .FirstOrDefault(f => string.Equals(f.Value.Extension, extension, StringComparison.OrdinalIgnoreCase))
                .Key ?? "application/octet-stream";

            Stream stream = File.OpenRead(absolutePath);

            return (stream, contentType, Path.GetFileName(absolutePath));
        }

        private bool IsInsideRoot(string absolutePath)
        {
            var root = Path.GetFullPath(_rootPath);
            var target = Path.GetFullPath(absolutePath);

            return target.StartsWith(root, StringComparison.Ordinal);
        }

        private static bool StartsWithSignature(byte[] bytes, byte[] signature)
        {
            if (bytes.Length < signature.Length)
            {
                return false;
            }

            for (var i = 0; i < signature.Length; i++)
            {
                if (bytes[i] != signature[i])
                {
                    return false;
                }
            }

            return true;
        }

        private sealed record ImageFormat(string Extension, byte[] Signature);
    }
}
