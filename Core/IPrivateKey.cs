using System.Security.Cryptography;

namespace Fiscalapi.Credentials.Core;

/// <summary>
/// Represents a wrapper for FIEL and CSD private key.
/// </summary>
public interface IPrivateKey
{
    /// <summary>
    /// File .key encoded in base64
    /// </summary>
    string Base64 { get; }

    /// <summary>
    /// Private key password
    /// </summary>
    string PasswordPhrase { get; }

    /// <summary>
    /// Private key RSA object
    /// </summary>
    RSA RsaPrivateKey { get; }

    /// <summary>
    /// File .key in bytes
    /// </summary>
    byte[] PrivateKeyBytes { get; }

    /// <summary>
    /// Private key password converted to bytes
    /// </summary>
    byte[] PasswordPhraseBytes { get; }

    /// <summary>
    /// Convert PKCS#8 DER private key to PKCS#8 PEM
    /// </summary>
    /// <returns></returns>
    string GetPemRepresentation();

    /// <summary>
    /// Sign some data
    /// </summary>
    /// <param name="toSign">string to be signed</param>
    /// <param name="algorithm">Hash algorithm to use for signing</param>
    /// <param name="padding">Signature padding to use</param>
    /// <returns>signed bytes</returns>
    byte[] SignData(string toSign, HashAlgorithmName algorithm, RSASignaturePadding padding);

    /// <summary>
    /// Verify the signature of some data
    /// </summary>
    /// <param name="dataToVerify">original data in bytes</param>
    /// <param name="signedData">signed data in bytes</param>
    /// <param name="algorithm">Hash algorithm to use for verification</param>
    /// <param name="padding">Signature padding to use</param>
    /// <returns>True when the signature is valid, otherwise false</returns>
    bool VerifyData(byte[] dataToVerify, byte[] signedData, HashAlgorithmName algorithm, RSASignaturePadding padding);
}