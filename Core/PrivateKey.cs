using System;
using System.Security.Cryptography;
using System.Text;
using Fiscalapi.Credentials.Common;

namespace Fiscalapi.Credentials.Core;

/// <summary>
/// Represents a wrapper for FIEL and CSD private key.
/// </summary>
public class PrivateKey : IPrivateKey
{
    public PrivateKey(string fileInbase64, string passwordPhrase)
    {
        PasswordPhrase = passwordPhrase;
        Base64 = fileInbase64;
        RsaPrivateKey = RSA.Create();
        RsaPrivateKey.ImportEncryptedPkcs8PrivateKey(PasswordPhraseBytes, PrivateKeyBytes, out _);
    }

    /// <summary>
    /// File .key encoded in base64
    /// </summary>
    public string Base64 { get; }

    /// <summary>
    /// Private key password
    /// </summary>
    public string PasswordPhrase { get; }

    /// <summary>
    /// Private key RSA object
    /// </summary>
    public RSA RsaPrivateKey { get; }

    /// <summary>
    /// File .key in bytes
    /// </summary>
    public byte[] PrivateKeyBytes
    {
        get => Convert.FromBase64String(Base64);
    }

    /// <summary>
    /// Private key password converted to bytes
    /// </summary>
    public byte[] PasswordPhraseBytes
    {
        get => Encoding.ASCII.GetBytes(PasswordPhrase);
    }

    /// <summary>
    /// Convert PKCS#8 DER private key to PKCS#8 PEM
    /// </summary>
    /// <returns></returns>
    public string GetPemRepresentation()
    {
        var keyPem = new string(PemEncoding.Write(Flags.PemPrivateKey, RsaPrivateKey.ExportPkcs8PrivateKey()));
        return keyPem;
    }

    /// <summary>
    /// Sign some data
    /// </summary>
    /// <param name="toSign">string to be signed</param>
    /// <returns>signed bytes</returns>
    /// see CredentialSettings class
    public byte[] SignData(string toSign)
    {
        //get bytes array to sing
        var bytesToSign = toSign.GetBytes();

        //Sing and get signed bytes array
        var signedBytes = RsaPrivateKey.SignData(bytesToSign, CredentialSettings.SignatureAlgorithm, CredentialSettings.SignaturePadding);

        //Converts signed bytes to base64
        return signedBytes;
    }

    /// <summary>
    /// Verify the signature of some data
    /// </summary>
    /// <param name="dataToVerify">original data in bytes</param>
    /// <param name="signedData">signed data in bytes</param>
    /// <returns>True when the signature is valid, otherwise false</returns>
    public bool VerifyData(byte[] dataToVerify, byte[] signedData)
    {
        try
        {
            //Validation 
            var isValid = RsaPrivateKey.VerifyData(dataToVerify, signedData, CredentialSettings.SignatureAlgorithm,
                CredentialSettings.SignaturePadding);


            return isValid;
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}