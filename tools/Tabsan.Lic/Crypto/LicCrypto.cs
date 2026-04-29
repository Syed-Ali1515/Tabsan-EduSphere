using System.Security.Cryptography;
using System.Text;

namespace Tabsan.Lic.Crypto;

/// <summary>
/// Provides AES-256-CBC encryption and RSA-2048 signing operations used when building .tablic files.
/// </summary>
internal static class LicCrypto
{
    // ── .tablic binary layout ──────────────────────────────────────────────────
    // Offset  0 –  6 : Magic header "TABLIC\x01" (7 bytes)
    // Offset  7 – 262: RSA-2048 PKCS#1 v1.5 signature of SHA-256(IV + ciphertext) (256 bytes)
    // Offset 263 – 278: AES-256-CBC IV (16 bytes)
    // Offset 279+     : AES-256-CBC encrypted JSON payload (padded to 16-byte blocks)

    internal static readonly byte[] Magic = "TABLIC\x01"u8.ToArray();

    /// <summary>
    /// Encrypts <paramref name="plaintext"/> using AES-256-CBC with a freshly generated IV.
    /// Returns <c>(ciphertext, iv)</c>.
    /// </summary>
    internal static (byte[] Ciphertext, byte[] Iv) EncryptAes(byte[] plaintext)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = Convert.FromBase64String(EmbeddedKeys.AesKeyBase64);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var enc = aes.CreateEncryptor();
        var cipher = enc.TransformFinalBlock(plaintext, 0, plaintext.Length);
        return (cipher, aes.IV);
    }

    /// <summary>
    /// Signs the SHA-256 hash of <paramref name="data"/> using the embedded RSA private key
    /// with PKCS#1 v1.5 padding.  Returns the raw 256-byte signature.
    /// </summary>
    internal static byte[] SignRsa(byte[] data)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(EmbeddedKeys.RsaPrivateKeyPem);
        var hash = SHA256.HashData(data);
        return rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    /// <summary>
    /// Serialises a complete .tablic binary: magic + signature + IV + ciphertext.
    /// </summary>
    internal static byte[] BuildTablicFile(string payloadJson)
    {
        var plaintext = Encoding.UTF8.GetBytes(payloadJson);
        var (ciphertext, iv) = EncryptAes(plaintext);

        // Sign IV + ciphertext
        var dataToSign = new byte[iv.Length + ciphertext.Length];
        iv.CopyTo(dataToSign, 0);
        ciphertext.CopyTo(dataToSign, iv.Length);
        var signature = SignRsa(dataToSign);

        // Assemble file
        using var ms = new MemoryStream();
        ms.Write(Magic);
        ms.Write(signature);   // 256 bytes
        ms.Write(iv);          // 16 bytes
        ms.Write(ciphertext);  // variable
        return ms.ToArray();
    }
}
