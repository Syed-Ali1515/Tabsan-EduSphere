namespace Tabsan.EduSphere.API.Services;

/// <summary>
/// Validates uploaded files by checking size, extension, MIME type, and magic bytes.
/// Prevents malicious file uploads (OWASP A04 — Insecure Design).
/// </summary>
public static class FileUploadValidator
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"
    };

    private static readonly Dictionary<string, string[]> AllowedMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"]  = ["application/pdf"],
            [".jpg"]  = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"]  = ["image/png"],
            [".doc"]  = ["application/msword"],
            [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"]
        };

    // Magic bytes: extension → expected header bytes (null = wildcard byte)
    private static readonly Dictionary<string, byte[][]> MagicBytes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"]  = [[0x25, 0x50, 0x44, 0x46]],                                     // %PDF
            [".jpg"]  = [[0xFF, 0xD8, 0xFF]],                                            // JPEG SOI
            [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
            [".png"]  = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],            // PNG
            [".doc"]  = [[0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]],            // OLE CFB
            [".docx"] = [[0x50, 0x4B, 0x03, 0x04]]                                       // ZIP (OOXML)
        };

    /// <summary>
    /// Validates the uploaded file. Returns null on success, or an error message on failure.
    /// </summary>
    public static async Task<string?> ValidateAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return "No file uploaded.";

        if (file.Length > MaxFileSizeBytes)
            return $"File exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.";

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return $"File type '{ext}' is not permitted. Allowed: {string.Join(", ", AllowedExtensions)}.";

        if (AllowedMimeTypes.TryGetValue(ext, out var mimes) &&
            !mimes.Any(m => file.ContentType.StartsWith(m, StringComparison.OrdinalIgnoreCase)))
        {
            return $"MIME type '{file.ContentType}' does not match the file extension '{ext}'.";
        }

        if (MagicBytes.TryGetValue(ext, out var magicOptions))
        {
            var headerLength = magicOptions.Max(m => m.Length);
            var header       = new byte[headerLength];

            await using var stream = file.OpenReadStream();
            var read = await stream.ReadAsync(header.AsMemory(0, headerLength));

            var matchesAny = magicOptions.Any(magic =>
                read >= magic.Length &&
                magic.Select((b, i) => header[i] == b).All(x => x));

            if (!matchesAny)
                return $"File content does not match the expected format for '{ext}'.";
        }

        return null; // valid
    }
}
