using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Fiscalapi.Credentials.Core;

/// <summary>
/// Read-only view of a SAT cryptographic certificate (FIEL/e.firma or CSD).
/// </summary>
public interface ICertificate
{
    /// <summary>
    /// The .cer file bytes encoded as base64. Preserved so the credential can be round-tripped through storage.
    /// </summary>
    string PlainBase64 { get; }

    /// <summary>
    /// Decoded bytes of the .cer file (Convert.FromBase64String of PlainBase64).
    /// </summary>
    byte[] CertificatePlainBytes { get; }

    /// <summary>
    /// RFC of the certificate holder, parsed from the subject's x500UniqueIdentifier (OID.2.5.4.45).
    /// </summary>
    string Rfc { get; }

    /// <summary>
    /// Razón social of the certificate holder (OID.2.5.4.41 with fallback to "O").
    /// </summary>
    string Organization
    {
        // CN: CommonName
        // OU: OrganizationalUnit
        // O: Organization
        // L: Locality
        // S: StateOrProvinceName
        // C: CountryName
        get;
    }

    /// <summary>
    /// OrganizationalUnit ("Sucursal"). As of 2019-08-01 only CSDs have OU; FIEL certificates do not.
    /// </summary>
    string OrganizationalUnit
    {
        // CN: CommonName
        // OU: OrganizationalUnit
        // O: Organization
        // L: Locality
        // S: StateOrProvinceName
        // C: CountryName
        get;
    }

    /// <summary>
    /// Big-endian hexadecimal serial number reported by X509Certificate2.SerialNumber.
    /// </summary>
    string SerialNumber { get; }

    /// <summary>
    /// 20-digit ASCII "noCertificado" required by the SAT in CFDI sealing.
    /// </summary>
    string CertificateNumber { get; }

    /// <summary>
    /// Issuer DN parsed into key/value pairs (key is Oid.FriendlyName when available, otherwise "OID.&lt;value&gt;").
    /// </summary>
    List<KeyValuePair<string, string>> IssuerKeyValuePairs { get; }

    /// <summary>
    /// Issuer DN as a single string (X509Certificate2.Issuer).
    /// </summary>
    string Issuer { get; }

    /// <summary>
    /// Subject DN parsed into key/value pairs (key is Oid.FriendlyName when available, otherwise "OID.&lt;value&gt;").
    /// </summary>
    List<KeyValuePair<string, string>> SubjectKeyValuePairs { get; }

    /// <summary>
    /// Subject DN as a single string (X509Certificate2.Subject).
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// X.509 format version (typically 3 for SAT-issued certificates).
    /// </summary>
    int Version { get; }

    /// <summary>
    /// NotBefore (certificate effective date) in local time.
    /// </summary>
    DateTime ValidFrom { get; }

    /// <summary>
    /// NotAfter (certificate expiration date) in local time.
    /// </summary>
    DateTime ValidTo { get; }

    /// <summary>
    /// Length in bytes of the raw DER-encoded certificate.
    /// </summary>
    int RawDataLength { get; }

    /// <summary>
    /// Raw DER-encoded certificate bytes.
    /// </summary>
    byte[] RawDataBytes { get; }

    /// <summary>
    /// True when ValidTo is in the future (compared against DateTime.Now).
    /// </summary>
    bool IsValid();

    /// <summary>
    /// True when the certificate is a FIEL/e.firma, identified by the absence of an OrganizationalUnit.
    /// </summary>
    bool IsFiel();

    /// <summary>
    /// Converts the X.509 DER (or its base64 form) to X.509 PEM.
    /// </summary>
    string GetPemRepresentation();

    /// <summary>
    /// True when this certificate and the given RSA private key form a matching pair
    /// (compares the modulus and exponent of the public keys).
    /// </summary>
    bool ArePaired(RSA privateKey);
}
