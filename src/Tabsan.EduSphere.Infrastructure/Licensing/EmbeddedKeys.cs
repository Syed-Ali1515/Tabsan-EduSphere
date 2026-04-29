namespace Tabsan.EduSphere.Infrastructure.Licensing;

/// <summary>
/// Compile-time embedded cryptographic key material used by EduSphere to verify and
/// decrypt incoming .tablic license files.
///
/// Only the RSA PUBLIC key and the AES-256 key live here.
/// The RSA private key exists solely in the Tabsan-Lic standalone tool.
/// </summary>
internal static class EmbeddedKeys
{
    /// <summary>
    /// RSA-2048 public key in PKCS#1 PEM format.
    /// Used to verify the RSA-2048 signature embedded in every .tablic file.
    /// </summary>
    internal const string RsaPublicKeyPem =
        """
        -----BEGIN RSA PUBLIC KEY-----
        MIIBCgKCAQEAyiJunggNrkgy6G6wz0OplTBBAUimPj5OgX7Nf3fGHca//IkgXiWy
        yj3GQ/S63ghOI32NvKmNHGEjXOoy/QjHs7X7b2DceIW0Ti4r6Uc1/ajLxu/s06J2
        WQ7hCBE9MRJz7zda6nPTKyRMHAHoV9p/DNsxOD/NtzgzHd9LUld924C4vGmyfdbl
        Olb45QZBkVAiIU4x0jh65o5Zz6EQEJnQC8IhpUJd9EPTfWl9KhJRtNTFu2iR5xPG
        1AJRUn78dnQM/LYG407PRwWj/VwWIvcIRX0afoKzYc4zSs0kubpHfVfj4gi0iGwD
        wGk9HsZXzSFPViAAU3mkR1TFJe/+AhwAAQIDAQAB
        -----END RSA PUBLIC KEY-----
        """;

    /// <summary>
    /// AES-256 symmetric key shared with Tabsan-Lic (Base64-encoded, 32 bytes).
    /// Used to decrypt the AES-256-CBC encrypted payload inside a .tablic file.
    /// </summary>
    internal const string AesKeyBase64 = "NIdsTzpLjAK2PZwGMQkJLn7SVBJm2yWx0hIpv/R6UnE=";
}
