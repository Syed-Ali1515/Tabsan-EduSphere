namespace Tabsan.Lic.Crypto;

/// <summary>
/// Embedded cryptographic key material for Tabsan-Lic.
/// The RSA private key is used to sign license payloads.
/// The AES-256 key is used to encrypt the JSON payload before writing the .tablic file.
/// IMPORTANT: Keep this file out of version control or use environment-specific key loading
/// in production deployments.
/// </summary>
internal static class EmbeddedKeys
{
    /// <summary>
    /// RSA-2048 private key in PKCS#1 PEM format.
    /// Used only by Tabsan-Lic to sign license files.
    /// The corresponding public key is embedded in EduSphere.
    /// </summary>
    internal const string RsaPrivateKeyPem =
        """
        -----BEGIN RSA PRIVATE KEY-----
        MIIEowIBAAKCAQEAyiJunggNrkgy6G6wz0OplTBBAUimPj5OgX7Nf3fGHca//Ikg
        XiWyyj3GQ/S63ghOI32NvKmNHGEjXOoy/QjHs7X7b2DceIW0Ti4r6Uc1/ajLxu/s
        06J2WQ7hCBE9MRJz7zda6nPTKyRMHAHoV9p/DNsxOD/NtzgzHd9LUld924C4vGmy
        fdblOlb45QZBkVAiIU4x0jh65o5Zz6EQEJnQC8IhpUJd9EPTfWl9KhJRtNTFu2iR
        5xPG1AJRUn78dnQM/LYG407PRwWj/VwWIvcIRX0afoKzYc4zSs0kubpHfVfj4gi0
        iGwDwGk9HsZXzSFPViAAU3mkR1TFJe/+AhwAAQIDAQABAoIBAQCFJsddXIq+iprW
        V8wqzDSSrRW1Jck06VBHp2LxG9Iq3TisvxvOSOEMrkLDkxvhlPD8GgHbDImC704f
        L7tkyXrbm/5EMTcqQVEzyuBsK9eZ/640numPw85X/iAoc0qu36v1Ia7HEINDQQbN
        0EfgT3Mv4df7aLQ3hFLP077HQBENHQKfDaMFWOwRxom7rV4lN3/n/4OY+jTO+5Jr
        B/xwQNpxwqr2fgjlboT6fG+8sU1SquFv/Ou4vRHspfMnUJ4TMM8Pn6AdO7FSatDq
        DghCoKRM/+9OakMq6B3zBNYxSq6hAGy9AjzauSSx9k+5NtFMxwm3PZcqQFWzagDA
        6uG+x219AoGBAPRGiBUiS6ijHxqEzw1kbw7xLB9U7+lsup+afQUPt6GNiRZwDxPu
        ZVBG60Sg288M7w87VvT+quvhdPtvDcuAT8WCTZJSEcdJ17qcF1K9fHXqIVLAfHya
        z9lmFNfFqHA3q37HDPNkNMmatyZlGAQkRRj+nQOPRe9pHT0DZBlVu7kjAoGBANPW
        GrggEr0O2DXXN2aKXEAQBq2vGg4rOVDvsXlyUKJD0Yn1aAuUB8IwPY9QQAEKRfRk
        OEcTEIyRUHFFbGE55D8gVCQa8yQq0KlxyKFzH8R4/qrCwMJrcKaktrpY+UYWEVvs
        X96wv6Rki/SVbyWCcbtgrW/qDjfKPaaDqytpyD6LAoGAe6JfKeMry/STv4ZMjYix
        tSxXmpwQuWIwqqs0b6Ve2cObCOI6n2nfmVvro9aOqiLvtBPilSl4NN7tqHyyzLbq
        qRqkTFSBbw5uw6JRI62IGt4fc5S87Qwl+vBxyCvgbruebxIr+dxT414NKL+uAhqh
        Zl8n9S9ExEG9bK4UscX0t2UCgYATLJIGkIChtDJFzVEqauOmuMyh8/N7zNXHSara
        v0olJdZVkmz0f1WkchFgY3cnoPJsCJY/eK5KyuxpFEuXEVJjlF2JVxci8u0oKTBr
        zKvXcMw8UJx5/JeZvdb8TwlhGqY/l8mlsoHmM2Ono88HqiL5Purz8k+PJTMnW0un
        BlAluQKBgAzhxN3knsL93LOnSQsDMXh2C/YURyY8hIRgUGZF7WvSgHjGf2oBM0fp
        y9E+FIGDy5MZK3eH1pqLPe+Y6UAE/5fu0HJy4WFkZCcfhveNUfy9vFZSYkzHyNhw
        8fSeBo6y7bfH8YkVElE/91WRzaPIgEluQur11pvSZbSIGqdNfEyD
        -----END RSA PRIVATE KEY-----
        """;

    /// <summary>
    /// AES-256 symmetric key shared with EduSphere (Base64-encoded, 32 bytes).
    /// Used to encrypt the license payload in the .tablic file.
    /// </summary>
    internal const string AesKeyBase64 = "NIdsTzpLjAK2PZwGMQkJLn7SVBJm2yWx0hIpv/R6UnE=";
}
