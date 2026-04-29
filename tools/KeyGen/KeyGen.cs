using System.Security.Cryptography;

var rsa = RSA.Create(2048);
var privDer = rsa.ExportRSAPrivateKey();
var pubDer  = rsa.ExportRSAPublicKey();

Console.WriteLine("=== RSA PRIVATE KEY (PKCS#1 PEM) ===");
Console.WriteLine("-----BEGIN RSA PRIVATE KEY-----");
Console.WriteLine(Convert.ToBase64String(privDer, Base64FormattingOptions.InsertLineBreaks));
Console.WriteLine("-----END RSA PRIVATE KEY-----");

Console.WriteLine();
Console.WriteLine("=== RSA PUBLIC KEY (PKCS#1 PEM) ===");
Console.WriteLine("-----BEGIN RSA PUBLIC KEY-----");
Console.WriteLine(Convert.ToBase64String(pubDer, Base64FormattingOptions.InsertLineBreaks));
Console.WriteLine("-----END RSA PUBLIC KEY-----");

var aes = Aes.Create();
aes.KeySize = 256;
aes.GenerateKey();
Console.WriteLine();
Console.WriteLine("=== AES-256 KEY (Base64) ===");
Console.WriteLine(Convert.ToBase64String(aes.Key));
