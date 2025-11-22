using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using Fiscalapi.Credentials.Common;

namespace Fiscalapi.Credentials.Core;

/// <summary>
/// Represents a wrapper for certificate and private key. Something like 'FIEL and CSD'
/// </summary>
public class Credential : ICredential
{
    //certificate and private key into X509Certificate2 object
    private readonly X509Certificate2 certificateWithPrivateKey;


    public Credential(ICertificate certificate, IPrivateKey privateKey)
    {
        Certificate = certificate;
        PrivateKey = privateKey;
        PasswordPhrase = privateKey.PasswordPhrase;

        PemCertificate = certificate.GetPemRepresentation();
        PemPrivateKey = privateKey.GetPemRepresentation();

        certificateWithPrivateKey = X509Certificate2.CreateFromPem(PemCertificate, PemPrivateKey);
        
        // Default algorithm is SHA256 (for invoicing)
        SignatureAlgorithm = HashAlgorithmName.SHA256;
        
        // Default padding is Pkcs1 (used for both invoicing and XML downloader)
        SignaturePadding = RSASignaturePadding.Pkcs1;
    }

    /// <summary>
    /// Private key password
    /// </summary>
    public string PasswordPhrase { get; }

    /// <summary>
    ///  Dotnetcfdi certificate wrapper
    /// </summary>
    public ICertificate Certificate { get; }

    /// <summary>
    ///  Dotnetcfdi PrivateKey wrapper
    /// </summary>
    public IPrivateKey PrivateKey { get; }


    public string PemCertificate { get; }

    public string PemPrivateKey { get; }

    /// <summary>
    /// Signature algorithm used for signing data. Default is SHA256 (for invoicing).
    /// </summary>
    public HashAlgorithmName SignatureAlgorithm { get; set; }

    /// <summary>
    /// Signature padding used for signing data. Default is Pkcs1 (used for both invoicing and XML downloader).
    /// </summary>
    public RSASignaturePadding SignaturePadding { get; set; }

    public byte[] CreatePFX()
    {
        var pfxBytes = certificateWithPrivateKey.Export(X509ContentType.Pfx, PasswordPhrase);

        return pfxBytes;
    }

    private string GetCertificatePemRepresentation()
    {
        return Certificate.GetPemRepresentation();
    }

    private string GetPrivateKeyPemRepresentation()
    {
        return PrivateKey.GetPemRepresentation();
    }

    /// <summary>
    /// Sign some data
    /// </summary>
    /// <param name="toSign">string to be signed</param>
    /// <returns>signed bytes</returns>
    public byte[] SignData(string toSign)
    {
        //Sing and get signed bytes array
        var signedBytes = PrivateKey.SignData(toSign, SignatureAlgorithm, SignaturePadding);

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
        var isValid = PrivateKey.VerifyData(dataToVerify, signedData, SignatureAlgorithm, SignaturePadding);

        return isValid;
    }


    /// <summary>
    /// True if Certificate.ValidTo date is less than the current date
    /// </summary>
    public bool IsValid()
    {
        return Certificate.IsValid();
    }

    /// <summary>
    /// True when is a FIEL certificate
    /// </summary>
    public bool IsFiel()
    {
        return Certificate.IsFiel();
    }

    /// <summary>
    /// True when certificate.ValidTo date is less than the current date and is a FIEL certificate
    /// </summary>
    /// <returns></returns>
    public bool IsValidFiel()
    {
        return IsFiel() && IsValid();
    }

    /// <summary>
    /// Fiel whe credential.certificate is a FIEL certificate otherwise csd
    /// </summary>
    public CredentialType CredentialType
    {
        get => Certificate.IsFiel() ? CredentialType.Fiel : CredentialType.Csd;
    }

    /// <summary>
    ///  Convert the input string to a byte array and compute the hash.
    /// </summary>
    /// <param name="input">data to hashing</param>
    /// <returns>encoded b64 hash</returns>
    public string CreateHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        var inputBytes = Encoding.UTF8.GetBytes(input);

        using var hashAlgorithm = SHA1.Create();

        var hashBytes = hashAlgorithm.ComputeHash(inputBytes);
        var encodedBytes = hashBytes.ToBase64String();

        return encodedBytes;
    }

    //public string CreateHash(string input)
    //{
    //    var inputBytes = Encoding.UTF8.GetBytes(input);

    //    var hashBytes = CredentialSettings.HashAlgorithm.ComputeHash(inputBytes);

    //    var encodedBytes = hashBytes.ToBase64String();

    //    return encodedBytes;
    //}

    /// <summary>
    /// Verify a hash against a string.
    /// </summary>
    /// <param name="input">data to hashing</param>
    /// <param name="hash">encoded b64 hash</param>
    /// <returns> true when computed hash is same of input hash otherwise false</returns>
    public bool VerifyHash(string input, string hash)
    {
        // Hash the input.
        var hashOfInput = CreateHash(input);

        // Create a StringComparer an compare the hashes.
        var comparer = StringComparer.OrdinalIgnoreCase;

        return comparer.Compare(hashOfInput, hash) == 0;
    }


    /// <summary>
    /// Transform XML documents into Pipe character in accordance with the schemes established in Mexican legislation.
    /// </summary>
    /// <param name="xmlAsString"> Xml file as string</param>
    /// <returns>cadena original</returns>
    public string GetOriginalStringByXmlString(string xmlAsString)
    {
        if (string.IsNullOrEmpty(CredentialSettings.OriginalStringPath))
            throw new ArgumentNullException(nameof(CredentialSettings.OriginalStringPath),
                "The path to cadenaoriginal.xslt file cannot be null or empty.");


        if (string.IsNullOrEmpty(xmlAsString))
            throw new ArgumentNullException(nameof(xmlAsString),
                "The xml to calculate the original string cannot be null or empty.");

        using var stringReader = new StringReader(xmlAsString);
        using var xmlReader = XmlReader.Create(stringReader);
        using var stringWriter = new Utf8StringWriter();

        var xsltSettings = new XsltSettings
        {
            EnableDocumentFunction = true,
            EnableScript = true
        };
        var resolver = new XmlUrlResolver();
        var transformer = new XslCompiledTransform();

        transformer.Load(CredentialSettings.OriginalStringPath, xsltSettings, resolver);
        transformer.Transform(xmlReader, null, stringWriter);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Configure the Signature algorithm to do invoicing, using the donetcfdi/invoicing library.
    /// Sets HashAlgorithmName.SHA256 for signing invoices. Returns this for fluent interface.
    /// </summary>
    public ICredential ConfigureAlgorithmForInvoicing()
    {
        SignatureAlgorithm = HashAlgorithmName.SHA256;
        return this;
    }

    /// <summary>
    /// Configure the Signature algorithm to do xml-downloader, using the donetcfdi/xml-downloader library.
    /// Sets HashAlgorithmName.SHA1 for downloading xml. Returns this for fluent interface.
    /// </summary>
    public ICredential ConfigureAlgorithmForXmlDownloader()
    {
        SignatureAlgorithm = HashAlgorithmName.SHA1;
        return this;
    }
}