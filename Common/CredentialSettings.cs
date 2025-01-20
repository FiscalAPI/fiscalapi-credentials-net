using System.Security.Cryptography;

namespace Fiscalapi.Credentials.Common;

public static class CredentialSettings
{
    /// <summary>
    /// Default algorithm to sing mexican invoicing 
    /// </summary>
    public static HashAlgorithmName SignatureAlgorithm { get; set; } = HashAlgorithmName.SHA1;

    /// <summary>
    /// Default signature padding
    /// </summary>
    public static RSASignaturePadding SignaturePadding { get; set; } = RSASignaturePadding.Pkcs1;

    /// <summary>
    /// Default algorithm to digest SAT services
    /// </summary>
    public static HashAlgorithm HashAlgorithm { get; set; } = SHA1.Create();

    /// <summary>
    /// Path of the CadenaOriginal.xslt file to do XML data transformation using an XSLT stylesheet.
    /// </summary>
    public static string? OriginalStringPath { get; set; }
}