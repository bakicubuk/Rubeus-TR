using System;
using System.Collections.Generic;
using System.IO;

namespace Rubeus.Commands
{
    public class S4u : ICommand
    {
        public static string CommandName => "s4u";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("[*] Eylem: S4U\r\n");

            string targetUser = "";
            string targetSPN = "";
            string altSname = "";
            string user = "";
            string domain = "";
            string hash = "";
            string outfile = "";
            bool ptt = false;
            string dc = "";
            string targetDomain = "";
            string targetDC = "";
            string impersonateDomain = "";
            bool self = false;
            bool opsec = false;
            bool bronzebit = false;
            bool pac = true;
            Interop.KERB_ETYPE encType = Interop.KERB_ETYPE.subkey_keymaterial; // gecici yer tutucu, gecerli bir degere degistirilecek
            KRB_CRED tgs = null;
            string proxyUrl = null;

            if (arguments.ContainsKey("/user"))
            {
                string[] parts = arguments["/user"].Split('\\');
                if (parts.Length == 2)
                {
                    domain = parts[0];
                    user = parts[1];
                }
                else
                {
                    user = arguments["/user"];
                }
            }
            if (arguments.ContainsKey("/domain"))
            {
                domain = arguments["/domain"];
            }
            if (arguments.ContainsKey("/ptt"))
            {
                ptt = true;
            }
            if (arguments.ContainsKey("/dc"))
            {
                dc = arguments["/dc"];
            }
            if (arguments.ContainsKey("/rc4"))
            {
                hash = arguments["/rc4"];
                encType = Interop.KERB_ETYPE.rc4_hmac;
            }
            if (arguments.ContainsKey("/aes256"))
            {
                hash = arguments["/aes256"];
                encType = Interop.KERB_ETYPE.aes256_cts_hmac_sha1;
            }
            if (arguments.ContainsKey("/impersonateuser"))
            {
                if (arguments.ContainsKey("/tgs"))
                {
                    Console.WriteLine("\r\n[X] Bir /impersonateuser veya bir /tgs saglamalisiniz, ikisi birden degil.\r\n");
                    return;
                }
                targetUser = arguments["/impersonateuser"];
            }
            if (arguments.ContainsKey("/impersonatedomain"))
            {
                impersonateDomain = arguments["/impersonatedomain"];
            }
            if (arguments.ContainsKey("/targetdomain"))
            {
                targetDomain = arguments["/targetdomain"];
            }
            if (arguments.ContainsKey("/targetdc"))
            {
                targetDC = arguments["/targetdc"];
            }
            if (arguments.ContainsKey("/outfile"))
            {
                outfile = arguments["/outfile"];
            }

            if (arguments.ContainsKey("/msdsspn"))
            {
                targetSPN = arguments["/msdsspn"];
            }

            if (arguments.ContainsKey("/altservice"))
            {
                altSname = arguments["/altservice"];
            }

            if (arguments.ContainsKey("/self"))
            {
                self = true;
            }

            if (arguments.ContainsKey("/opsec"))
            {
                opsec = true;
            }

            if (arguments.ContainsKey("/bronzebit"))
            {
                bronzebit = true;
            }
            if (arguments.ContainsKey("/nopac"))
            {
                pac = false;
            }
            if (arguments.ContainsKey("/proxyurl"))
            {
                proxyUrl = arguments["/proxyurl"];
            }

            if (arguments.ContainsKey("/tgs"))
            {
                string kirbi64 = arguments["/tgs"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    tgs = new KRB_CRED(kirbiBytes);
                }
                else if (File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = File.ReadAllBytes(kirbi64);
                    tgs = new KRB_CRED(kirbiBytes);
                }
                else
                {
                    Console.WriteLine("\r\n[X] /tgs:X bir .kirbi dosyasi veya base64 kodlu bir .kirbi olmalidir\r\n");
                    return;
                }

                targetUser = tgs.enc_part.ticket_info[0].pname.name_string[0];
            }

            if (String.IsNullOrEmpty(domain))
            {
                domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            }
            if (String.IsNullOrEmpty(targetUser) && tgs == null)
            {
                Console.WriteLine("\r\n[X] Taklit etmek (impersonate) icin bir /tgs saglamalisiniz!\r\n");
                Console.WriteLine("[X] Alternatif olarak, once S4U2Self gerceklestirmek icin bir /impersonateuser saglayin.\r\n");
                return;
            }
            if (String.IsNullOrEmpty(targetSPN) && tgs != null)
            {
                Console.WriteLine("\r\n[X] Bir /tgs saglaniyorsa, bir /msdsspn da saglamalisiniz!\r\n");
                return;
            }
            bool show = arguments.ContainsKey("/show");
            string createnetonly = null;

            if (arguments.ContainsKey("/createnetonly") && !String.IsNullOrWhiteSpace(arguments["/createnetonly"]))
            {
                createnetonly = arguments["/createnetonly"];
                ptt = true;
            }

            if (arguments.ContainsKey("/ticket"))
            {
                string kirbi64 = arguments["/ticket"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    S4U.Execute(kirbi, targetUser, targetSPN, outfile, ptt, dc, altSname, tgs, targetDC, targetDomain, self, opsec, bronzebit, hash, encType, domain, impersonateDomain, proxyUrl, createnetonly, show);
                }
                else if (File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = File.ReadAllBytes(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    S4U.Execute(kirbi, targetUser, targetSPN, outfile, ptt, dc, altSname, tgs, targetDC, targetDomain, self, opsec, bronzebit, hash, encType, domain, impersonateDomain, proxyUrl, createnetonly, show);
                }
                else
                {
                    Console.WriteLine("\r\n[X] /ticket:X bir .kirbi dosyasi veya base64 kodlu bir .kirbi olmalidir\r\n");
                }
                return;
            }
            else if (arguments.ContainsKey("/user"))
            {
                // kullanici once bir TGT talebi gerceklestirmek icin bir kullanici ve rc4/aes256 hash saglıyorsa

                user = arguments["/user"];

                if (String.IsNullOrEmpty(hash))
                {
                    Console.WriteLine("\r\n[X] Bir /rc4 veya /aes256 hash saglamalisiniz!\r\n");
                    return;
                }

                S4U.Execute(user, domain, hash, encType, targetUser, targetSPN, outfile, ptt, dc, altSname, tgs, targetDC, targetDomain, self, opsec, bronzebit, pac, proxyUrl, createnetonly, show);
                return;
            }
            else
            {
                Console.WriteLine("\r\n[X] S4U icin bir /ticket:X saglanmalidir!\r\n");
                Console.WriteLine("[X] Alternatif olarak, once bir TGT almak icin bir /user ve </rc4:X | /aes256:X> hash saglayin.\r\n");
                return;
            }
        }
    }
}
