using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PortHotel
{
    public static class X509Certificates
    {
        const string newCertificatePSCommand = "$cert=New-SelfSignedCertificate -DnsName \"{0}\" -FriendlyName \"{1}\" -Subject \"{2}\" -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -CertStoreLocation \"cert:\\LocalMachine\\My\"",
            storeLocation = "$store = new-object System.Security.Cryptography.X509Certificates.X509Store([System.Security.Cryptography.X509Certificates.StoreName]::{0},\"localmachine\")",
            openStore = "$store.Open(\"MaxAllowed\")",
            closeStore = "$store.Close()",
            addCertificate = "$store.Add($cert)",
            getCertThumbprint = "$cert.Thumbprint",
            removeCertRanges = "$store.RemoveRange($cert)",
            searchCert = "$cert = $store.Certificates | ? {$_.Thumbprint -match $thumbprint}";

        /// <summary>
        /// Create a new X509 certificate.
        /// </summary>
        /// <returns>Thumbprint</returns>
        public static string NewX509Certificate(string dnsName, string fiendlyName, string subject)
        {
            string thumbprint;
            using (var ps = PowerShell.Create())
            {
                ps.AddScript(String.Format(newCertificatePSCommand, dnsName, fiendlyName, subject));
                ps.AddScript(String.Format(storeLocation, "Root"));
                ps.AddScript(openStore);
                ps.AddScript(addCertificate);
                ps.AddScript(closeStore);
                ps.AddScript(getCertThumbprint);
                
                thumbprint = ps.Invoke()?[0]?.BaseObject?.ToString();
            }
            return thumbprint;
        }
        /// <summary>
        /// Remove X509 certificates from the system by thumbprint
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        public static void RemoveX509Certificate(string thumbprint)
        {
            using (var ps = PowerShell.Create())
            {
                void addScript(string location)
                {
                    ps.AddScript($"$thumbprint = '{thumbprint}'");
                    ps.AddScript(String.Format(storeLocation, location));
                    ps.AddScript(openStore);
                    ps.AddScript(searchCert);
                    ps.AddScript(removeCertRanges);
                    ps.AddScript(closeStore);
                }

                addScript("root");
                addScript("CertificateAuthority");
                addScript("My");

                ps.Invoke();
            }
        }

        /// <summary>
        /// Add a certificate to the http.
        /// </summary>
        public static bool InsertSslCert(string ip, uint port, string thumbprint, string guid)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddScript($"netsh http add sslcert ipport={ip}:{port} certhash={thumbprint} appid='{{{guid.ToUpper()}}}'");
                if(ps.Invoke()?[1].BaseObject?.ToString() == "SSL Certificate successfully added")
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// Add a certificate to the http.
        /// </summary>
        public static bool InsertSslCert(uint port, string domain, string thumbprint, string guid)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddScript($"netsh http add sslcert hostnameport={domain}:{port} certhash={thumbprint} appid='{{{guid.ToUpper()}}}' certstorename=my");
                if (ps.Invoke()?[1].BaseObject?.ToString() == "SSL Certificate successfully added")
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// Remove a certificate from the http.
        /// </summary>
        public static void RemoveCertificate(string ip, uint port)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddScript($"netsh http delete sslcert ipport={ip}:{port}");
                ps.Invoke();
            }
        }
        /// <summary>
        /// Remove a certificate from the http.
        /// </summary>
        public static void RemoveCertificate(uint port, string domain)
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddScript($"netsh http delete sslcert hostnameport={domain}:{port}");
                ps.Invoke();
            }
        }

    }
}
