using System;
using System.Collections.Generic;

namespace Fiscalapi.Credentials.Core;

public interface ICertificate
{
    /// <summary>
    /// The result of reading the bytes from the .cer file and converting them to base64
    /// </summary>
    string PlainBase64 { get; }

    /// <summary>
    /// The equivalent of reading the bytes from the .cer file and converting them to base64
    /// </summary>
    byte[] CertificatePlainBytes { get; }

    /// <summary>
    /// RFC as parsed from subject/x500UniqueIdentifier 
    /// see https://oidref.com/2.5.4.45
    /// </summary>
    string Rfc { get; }

    /// <summary>
    /// Organization = 'razón social'
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
    /// OrganizationalUnit = 'Sucursal'
    /// As of 2019-08-01 is known that only CSD have OU (Organization Unit)
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
    /// All serial number
    /// </summary>
    string SerialNumber { get; }

    /// <summary>
    /// Certificate number as Mexican tax authority (SAT) require.
    /// </summary>
    string CertificateNumber { get; }

    /// <summary>
    /// Issuer data parsed into KeyValuePair collection
    /// </summary>
    List<KeyValuePair<string, string>> IssuerKeyValuePairs { get; }

    /// <summary>
    /// Raw X509Certificate2 Issuer property
    /// </summary>
    string Issuer { get; }

    /// <summary>
    /// Subject data parsed into KeyValuePair collection
    /// see https://oidref.com/2.5.4.45
    /// </summary>
    List<KeyValuePair<string, string>> SubjectKeyValuePairs { get; }

    /// <summary>
    /// Raw X509Certificate2 Subject property
    /// see https://oidref.com/2.5.4.45
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// Certificate version
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Valid start date
    /// </summary>
    DateTime ValidFrom { get; }

    /// <summary>
    /// Valid end date
    /// </summary>
    DateTime ValidTo { get; }

    /// <summary>
    /// Raw Data Length
    /// </summary>
    int RawDataLength { get; }

    /// <summary>
    /// RawDataBytes
    /// </summary>
    byte[] RawDataBytes { get; }

    /// <summary>
    /// True if ValidTo date is less than the current date
    /// </summary>
    bool IsValid();

    /// <summary>
    /// True when is a FIEL certificate
    /// </summary>
    bool IsFiel();

    /// <summary>
    /// Convert X.509 DER base64 or X.509 DER to X.509 PEM
    /// </summary>
    /// <returns></returns>
    string GetPemRepresentation();
}