namespace Fiscalapi.Credentials.Common;

/// <summary>
/// headers and footers for PEM formats files
/// </summary>
public static class Flags
{
    /// <summary>
    /// Header for private key
    /// </summary>
    public const string PemPrivateKey = "PRIVATE KEY";

    /// <summary>
    /// Header for certificate
    /// </summary>
    public const string PemCertificate = "CERTIFICATE";
    public const string PemPublicKey = "PUBLIC KEY";
    public const string PemRsaPrivateKey = "RSA PRIVATE KEY";
    public const string PemEncriptedPrivateKey = "ENCRYPTED PRIVATE KEY";
}