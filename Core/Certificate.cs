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
    /// Wrapper for a SAT cryptographic certificate (FIEL/e.firma or CSD).
    /// </summary>
    public sealed class Certificate : ICertificate
    {
        private readonly X509Certificate2 _x509Certificate2;

        /// <summary>
        /// Initializes the certificate from the .cer file bytes encoded as base64.
        /// </summary>
        public Certificate(string plainBase64)
        {
            PlainBase64 = plainBase64;
            _x509Certificate2 = new X509Certificate2(CertificatePlainBytes);
        }

        /// <summary>
        /// The .cer file bytes encoded as base64. Preserved so the credential can be round-tripped through storage.
        /// </summary>
        public string PlainBase64 { get; }

        /// <summary>
        /// Decoded bytes of the .cer file (Convert.FromBase64String of PlainBase64).
        /// </summary>
        public byte[] CertificatePlainBytes
        {
            get => Convert.FromBase64String(PlainBase64);
        }

        /// <summary>
        /// RFC of the certificate holder, parsed from the subject's x500UniqueIdentifier (OID.2.5.4.45).
        /// Cross-platform: Windows reports the key as "OID.2.5.4.45", Linux as "x500UniqueIdentifier".
        /// </summary>
        public string Rfc
        {
            get
            {
                var rfcPair = SubjectKeyValuePairs.FirstOrDefault(x =>
                    x.Key.Equals("OID.2.5.4.45", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(rfcPair.Value))
                {
                    rfcPair = SubjectKeyValuePairs.FirstOrDefault(x =>
                        x.Key.Equals("x500UniqueIdentifier", StringComparison.OrdinalIgnoreCase));
                }

                var value = rfcPair.Value;

                if (!string.IsNullOrEmpty(value) && value.Length >= 12)
                {
                    return value[..Math.Min(value.Length, 13)].Trim();
                }

                throw new Exception("Fiscalapi.Credentials wasn't able to resolve ICertificate.Rfc ");
            }
        }

        /// <summary>
        /// Razón social of the certificate holder, parsed from OID.2.5.4.41 ("name") with fallback to "O" (organizationName, OID.2.5.4.10).
        /// Cross-platform: Windows reports the key as "OID.2.5.4.41", Linux as "name".
        /// </summary>
        public string Organization
        {
            get
            {
                var pair = SubjectKeyValuePairs.FirstOrDefault(x =>
                    x.Key.Equals("OID.2.5.4.41", StringComparison.OrdinalIgnoreCase) ||
                    x.Key.Equals("name", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(pair.Value))
                {
                    pair = SubjectKeyValuePairs.FirstOrDefault(x =>
                        x.Key.Equals("O", StringComparison.OrdinalIgnoreCase));
                }

                return pair.Value?.Trim() ?? string.Empty;
            }
        }

        /// <summary>
        /// OrganizationalUnit ("Sucursal"). As of 2019-08-01 only CSDs have OU; FIEL certificates do not.
        /// </summary>
        public string OrganizationalUnit
        {
            get => ExistsKey(SubjectKeyValuePairs, "OU")
                ? SubjectKeyValuePairs.FirstOrDefault(x => x.Key.Equals("OU")).Value.Trim()
                : string.Empty;
        }

        private static bool ExistsKey(IEnumerable<KeyValuePair<string, string>> keyValuePairs, string key)
        {
            return keyValuePairs.Any(pair => pair.Key.Equals(key));
        }

        /// <summary>
        /// Big-endian hexadecimal serial number reported by X509Certificate2.SerialNumber.
        /// </summary>
        public string SerialNumber
        {
            get => _x509Certificate2.SerialNumber;
        }

        /// <summary>
        /// 20-digit ASCII "noCertificado" required by the SAT in CFDI sealing
        /// (raw serial bytes reversed to little-endian and decoded as ASCII).
        /// </summary>
        public string CertificateNumber
        {
            get => Encoding.ASCII.GetString(_x509Certificate2.GetSerialNumber().Reverse().ToArray());
        }

        /// <summary>
        /// Issuer DN parsed into key/value pairs (key is Oid.FriendlyName when available, otherwise "OID.&lt;value&gt;").
        /// </summary>
        public List<KeyValuePair<string, string>> IssuerKeyValuePairs
        {
            get => ParseDistinguishedName(_x509Certificate2.IssuerName);
        }

        /// <summary>
        /// Issuer DN as a single string (X509Certificate2.Issuer).
        /// </summary>
        public string Issuer
        {
            get => _x509Certificate2.Issuer;
        }

        /// <summary>
        /// Subject DN parsed into key/value pairs (key is Oid.FriendlyName when available, otherwise "OID.&lt;value&gt;").
        /// </summary>
        public List<KeyValuePair<string, string>> SubjectKeyValuePairs
        {
            get => ParseDistinguishedName(_x509Certificate2.SubjectName);
        }

        /// <summary>
        /// Parses an X.500 distinguished name into key/value pairs, honoring RFC 2253 escaping
        /// (avoids the IndexOutOfRangeException that string.Split(',') hits on values with literal commas).
        /// </summary>
        private static List<KeyValuePair<string, string>> ParseDistinguishedName(X500DistinguishedName dn)
        {
            var result = new List<KeyValuePair<string, string>>();

            foreach (var rdn in dn.EnumerateRelativeDistinguishedNames())
            {
                if (rdn.HasMultipleElements)
                    continue;

                var oid = rdn.GetSingleElementType();
                var value = rdn.GetSingleElementValue();

                if (value is null)
                    continue;

                var key = !string.IsNullOrEmpty(oid.FriendlyName)
                    ? oid.FriendlyName
                    : $"OID.{oid.Value}";

                result.Add(new KeyValuePair<string, string>(key, value));
            }

            return result;
        }

        /// <summary>
        /// Subject DN as a single string (X509Certificate2.Subject).
        /// </summary>
        public string Subject
        {
            get => _x509Certificate2.Subject;
        }

        /// <summary>
        /// X.509 format version (typically 3 for SAT-issued certificates).
        /// </summary>
        public int Version
        {
            get => _x509Certificate2.Version;
        }

        /// <summary>
        /// NotBefore (certificate effective date) in local time.
        /// </summary>
        public DateTime ValidFrom
        {
            get => _x509Certificate2.NotBefore;
        }

        /// <summary>
        /// NotAfter (certificate expiration date) in local time.
        /// </summary>
        public DateTime ValidTo
        {
            get => _x509Certificate2.NotAfter;
        }

        /// <summary>
        /// True when ValidTo is in the future (compared against DateTime.Now).
        /// </summary>
        public bool IsValid()
        {
            return ValidTo > DateTime.Now;
        }

        /// <summary>
        /// True when the certificate is a FIEL/e.firma, identified by the absence of an OrganizationalUnit.
        /// </summary>
        public bool IsFiel()
        {
            return string.IsNullOrEmpty(OrganizationalUnit);
        }

        /// <summary>
        /// Length in bytes of the raw DER-encoded certificate.
        /// </summary>
        public int RawDataLength
        {
            get => _x509Certificate2.RawData.Length;
        }

        /// <summary>
        /// Raw DER-encoded certificate bytes.
        /// </summary>
        public byte[] RawDataBytes
        {
            get => _x509Certificate2.RawData;
        }

        /// <summary>
        /// Converts the X.509 DER (or its base64 form) to X.509 PEM.
        /// </summary>
        public string GetPemRepresentation()
        {
            var certPem = new string(PemEncoding.Write(Flags.PemCertificate, _x509Certificate2.RawData));

            return certPem;
        }

        /// <summary>
        /// True when this certificate and the given RSA private key form a matching pair
        /// (compares the modulus and exponent of the public keys).
        /// </summary>
        public bool ArePaired(RSA privateKey)
        {
            if (privateKey is null)
                return false;

            try
            {
                using var certificatePublicKey = _x509Certificate2.GetRSAPublicKey();
                if (certificatePublicKey is null)
                    return false;

                var certParams = certificatePublicKey.ExportParameters(includePrivateParameters: false);
                var keyParams = privateKey.ExportParameters(includePrivateParameters: false);

                return certParams.Modulus.AsSpan().SequenceEqual(keyParams.Modulus)
                    && certParams.Exponent.AsSpan().SequenceEqual(keyParams.Exponent);
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
