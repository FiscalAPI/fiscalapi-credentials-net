namespace Fiscalapi.Credentials.Common;

/// <summary>
/// This will be used when implementing data persistence with entity framework core.
/// </summary>
public enum FileType
{
    CertificateCsd,
    PrivateKeyCsd,
    CertificateFiel,
    PrivateKeyFiel,
    Pfx,
}