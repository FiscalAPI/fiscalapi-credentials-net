using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Fiscalapi.Credentials.Common;

namespace Fiscalapi.Credentials.Core
{
    /// <summary>
    /// Represents a wrapper for FIEL and CSD certificate.
    /// </summary>
    public sealed class Certificate : ICertificate
    {
        private readonly X509Certificate2 _x509Certificate2;


        public Certificate(string plainBase64)
        {
            PlainBase64 = plainBase64;
            _x509Certificate2 = new X509Certificate2(CertificatePlainBytes);
        }

        /// <summary>
        /// The result of reading the bytes from the .cer file and converting them to base64
        /// </summary>
        public string PlainBase64 { get; }


        /// <summary>
        /// The equivalent of reading the bytes from the .cer file and converting them to base64
        /// </summary>
        public byte[] CertificatePlainBytes
        {
            get => Convert.FromBase64String(PlainBase64);
        }


        /// <summary>
        /// RFC as parsed from subject/x500UniqueIdentifier 
        /// see https://oidref.com/2.5.4.45
        /// </summary>
        public string Rfc
        {
            get => SubjectKeyValuePairs.FirstOrDefault(x => x.Key.Equals("OID.2.5.4.45")).Value[..13].Trim();
        }


        /// <summary>
        /// Organization = 'razón social'
        /// </summary>
        public string Organization
        {
            // CN: CommonName
            // OU: OrganizationalUnit
            // O: Organization
            // L: Locality
            // S: StateOrProvinceName
            // C: CountryName
            get => ExistsKey(SubjectKeyValuePairs, "O")
                ? SubjectKeyValuePairs.FirstOrDefault(x => x.Key.Equals("O")).Value.Trim()
                : string.Empty;
        }

        /// <summary>
        /// OrganizationalUnit = 'Sucursal'
        /// As of 2019-08-01 is known that only CSD have OU (Organization Unit)
        /// </summary>
        public string OrganizationalUnit
        {
            // CN: CommonName
            // OU: OrganizationalUnit
            // O: Organization
            // L: Locality
            // S: StateOrProvinceName
            // C: CountryName

            get => ExistsKey(SubjectKeyValuePairs, "OU")
                ? SubjectKeyValuePairs.FirstOrDefault(x => x.Key.Equals("OU")).Value.Trim()
                : string.Empty;
        }

        private static bool ExistsKey(IEnumerable<KeyValuePair<string, string>> keyValuePairs, string key)
        {
            return keyValuePairs.Any(pair => pair.Key.Equals(key));
        }


        /// <summary>
        /// All serial number
        /// </summary>
        public string SerialNumber
        {
            get => _x509Certificate2.SerialNumber;
        }

        /// <summary>
        /// Certificate number as Mexican tax authority (SAT) require.
        /// </summary>
        public string CertificateNumber
        {
            get => Encoding.ASCII.GetString(_x509Certificate2.GetSerialNumber().Reverse().ToArray());
        }


        /// <summary>
        /// Issuer data parsed into KeyValuePair collection
        /// </summary>
        public List<KeyValuePair<string, string>> IssuerKeyValuePairs
        {
            get => _x509Certificate2.Issuer.Split(',')
                .Select(x => new KeyValuePair<string, string>(x.Split('=')[0].Trim(), x.Split('=')[1].Trim())).ToList();
        }

        /// <summary>
        /// Raw X509Certificate2 Issuer property
        /// </summary>
        public string Issuer
        {
            get => _x509Certificate2.Issuer;
        }


        /// <summary>
        /// Subject data parsed into KeyValuePair collection
        /// see https://oidref.com/2.5.4.45
        /// </summary>
        public List<KeyValuePair<string, string>> SubjectKeyValuePairs
        {
            get => _x509Certificate2.Subject.Split(',')
                .Select(x => new KeyValuePair<string, string>(x.Split('=')[0].Trim(), x.Split('=')[1].Trim())).ToList();
        }

        /// <summary>
        /// Raw X509Certificate2 Subject property
        /// see https://oidref.com/2.5.4.45
        /// </summary>
        public string Subject
        {
            get => _x509Certificate2.Subject;
        }


        /// <summary>
        /// Certificate version
        /// </summary>
        public int Version
        {
            get => _x509Certificate2.Version;
        }

        /// <summary>
        /// Valid start date
        /// </summary>
        public DateTime ValidFrom
        {
            get => _x509Certificate2.NotBefore;
        }

        /// <summary>
        /// Valid end date
        /// </summary>
        public DateTime ValidTo
        {
            get => _x509Certificate2.NotAfter;
        }

        /// <summary>
        /// True if ValidTo date is less than the current date
        /// </summary>
        public bool IsValid()
        {
            return ValidTo > DateTime.Now;
        }

        /// <summary>
        /// True when is a FIEL certificate
        /// </summary>
        public bool IsFiel()
        {
            return string.IsNullOrEmpty(OrganizationalUnit);
        }


        /// <summary>
        /// Raw Data Length
        /// </summary>
        public int RawDataLength
        {
            get => _x509Certificate2.RawData.Length;
        }

        /// <summary>
        /// RawDataBytes
        /// </summary>
        public byte[] RawDataBytes
        {
            get => _x509Certificate2.RawData;
        }

        /// <summary>
        /// Convert X.509 DER base64 or X.509 DER to X.509 PEM
        /// </summary>
        /// <returns></returns>
        public string GetPemRepresentation()
        {
            var certPem = new string(PemEncoding.Write(Flags.PemCertificate, _x509Certificate2.RawData));

            return certPem;
        }
    }
}