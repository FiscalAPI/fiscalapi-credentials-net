//using StringBuilder = Chilkat.StringBuilder;

//namespace Credencials.Common
//{
//    class Credentials
//    {
//        private byte[] certificateBytes;
//        private byte[] keyBytes;
//        private string password;

//        public Credentials(byte[] certificateBytes, byte[] keyBytes, string password)
//        {
//            this.certificateBytes = certificateBytes;
//            this.keyBytes = keyBytes;
//            this.password = password;
//        }
                                
//        public string GetCertificateNumber()
//        {
//            Cert certificate = LoadCertificate();
//            return GetCertificateNumber(certificate);
//        }
//        private string GetCertificateNumber(Cert certificate)
//        {
//            string hexadecimalString = certificate.SerialNumber;
//            StringBuilder sb = new StringBuilder();
//            for (int i = 0; i <= hexadecimalString.Length - 2; i += 2)
//            {
//                sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(hexadecimalString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber))));
//            }
//            return sb.ToString();
//        }
//        public string GetCertificateName()
//        {
//            Cert certificate = LoadCertificate();
//            return GetCertificateName(certificate);
//        }
//        private string GetCertificateName(Cert certificate)
//        {
//            string nameCertificate = "";
//            string[] subjects = certificate.SubjectDN.Trim().Split(',');
//            for (int i = 0; i < subjects.Length; i++)
//            {
//                string[] strConceptoTemp = subjects[i].Split('=');
//                if (strConceptoTemp[0].Trim() == "OID.2.5.4.41")
//                {
//                    nameCertificate = strConceptoTemp[1].Trim().Split('/')[0];
//                    //Bug Fix replace "
//                    nameCertificate = nameCertificate.Replace("\"", "");
//                    break;
//                }
//            }
//            return nameCertificate;
//        }
//        public string GetCertificateTaxId()
//        {
//            Cert certificate = LoadCertificate();
//            return GetCertificateTaxId(certificate);
//        }
//        private string GetCertificateTaxId(Cert certificate)
//        {
//            string taxIdCertificate = "";
//            string[] subjects = certificate.SubjectDN.Trim().Split(',');
//            for (int i = 0; i < subjects.Length; i++)
//            {
//                string[] strConceptoTemp = subjects[i].Split('=');
//                if (strConceptoTemp[0].Trim() == "OID.2.5.4.45")
//                {
//                    taxIdCertificate = strConceptoTemp[1].Trim().Split('/')[0];
//                    taxIdCertificate = taxIdCertificate.Replace("\"", "");
//                    break;
//                }
//            }
//            return taxIdCertificate.Trim().ToUpper();
//        }
//        public string GetCertificateValidFrom()
//        {
//            Cert certificate = LoadCertificate();
//            return GetCertificateValidFrom(certificate);
//        }
//        private string GetCertificateValidFrom(Cert certificate)
//        {
//            return
//                certificate.ValidFromStr;
//        }
//        public string GetCertificateValidTo()
//        {
//            Cert certificate = LoadCertificate();
//            return GetCertificateValidTo(certificate);
//        }
//        public string GetCertificateValidTo(Cert certificate)
//        {
//            return
//                certificate.ValidToStr;
//        }
//        public bool IsCSD()
//        {
//            Cert certificate = LoadCertificate();
//            return IsCSD(certificate);
//        }
//        private bool IsCSD(Cert certificate)
//        {
//            try
//            {
//                var subjects = certificate.SubjectDN.Split(',');
//                return subjects.Any(w => w.Trim().StartsWith("OU="));
//            }
//            catch
//            {
//                return false;
//            }
//        }
//        public bool IsFiel()
//        {
//            Cert certificate = LoadCertificate();
//            return IsFiel(certificate);
//        }
//        private bool IsFiel(Cert certificate)
//        {
//            try
//            {
//                var subjects = certificate.SubjectDN.Split(',');
//                return subjects.Any(w => !w.Trim().StartsWith("OU="));
//            }
//            catch
//            {
//                return false;
//            }
//        }
//        public bool IsValidKeyPair()
//        {
//            var publicKey = this.certificateBytes;
//            var privateKey = this.keyBytes;

//            if (publicKey == null)
//                throw new ArgumentException("publicKey is empty");
//            if (privateKey == null)
//                throw new ArgumentException("privateKey is empty");
//            if (string.IsNullOrEmpty(this.password))
//                throw new ArgumentException("password is empty");

//            Cert cert = LoadCertificate();
//            CertChain certChain = cert.GetCertChain();

//            PrivateKey privKey = new PrivateKey();
//            var success = privKey.LoadPkcs8Encrypted(privateKey, this.password);
//            if (!success)
//                throw new Exception("Private Key is not valid." + privKey.LastErrorText);
//            var modulusPrivateKey = GetModulusFromPublicKey(privKey.GetPublicKey());
//            var modulusPublicKey = GetModulusFromPublicKey(cert.ExportPublicKey());
//            return (modulusPrivateKey.Equals(modulusPublicKey));
//        }
//        private Cert LoadCertificate()
//        {
//            Cert certificate = new Cert();
//            var successLoad = certificate.LoadFromBinary(this.certificateBytes);
//            if (!successLoad) throw new Exception($"Invalid Certificate.{certificate.LastErrorText}");
//            return certificate;
//        }
//        private string GetModulusFromPublicKey(PublicKey publicKey)
//        {
//            Xml xml = new Xml();
//            xml.LoadXml(publicKey.GetXml());
//            string modulus = xml.GetChildContent("Modulus");
//            //  To convert to hex:
//            BinData binDat = new BinData();
//            binDat.Clear();
//            binDat.AppendEncoded(modulus, "base64");
//            return binDat.GetEncoded("hex");
//        }
//    }
//}
