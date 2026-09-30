using System;
using System.Collections.Generic;
using System.IO;
using Rubeus.lib.Interop;


namespace Rubeus.Commands
{
    public class Asktgs : ICommand
    {
        public static string CommandName => "asktgs";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("[*] Eylem: TGS Iste\r\n");

            string outfile = "";
            bool ptt = false;
            string dc = "";
            string service = "";
            bool enterprise = false;
            bool opsec = false;
            Interop.KERB_ETYPE requestEnctype = Interop.KERB_ETYPE.subkey_keymaterial;
            KRB_CRED tgs = null;
            string targetDomain = "";
            string servicekey = "";
            string asrepkey = "";
            bool u2u = false;
            string targetUser = "";
            bool printargs = false;
            bool keyList = false;
            string proxyUrl = null;
            bool dmsa = false;
            string serviceType = "srv_inst";

            LUID targetLuid = new LUID();
            if (arguments.ContainsKey("/luid")) {
                try {
                    targetLuid = new LUID(arguments["/luid"]);
                } catch {
                    Console.WriteLine("[X] Gecersiz LUID formati ({0})\r\n", arguments["/luid"]);
                    return;
                }
            }

            if (arguments.ContainsKey("/keyList"))
            {
                keyList = true;
            }
            if (arguments.ContainsKey("/outfile"))
            {
                outfile = arguments["/outfile"];
            }

            if (arguments.ContainsKey("/ptt"))
            {
                ptt = true;
            }

            if (arguments.ContainsKey("/enterprise"))
            {
                enterprise = true;
            }

            if (arguments.ContainsKey("/opsec"))
            {
                opsec = true;
            }

            if (arguments.ContainsKey("/dc"))
            {
                dc = arguments["/dc"];
            }

            if (arguments.ContainsKey("/enctype"))
            {
                string encTypeString = arguments["/enctype"].ToUpper();

                if (encTypeString.Equals("RC4") || encTypeString.Equals("NTLM"))
                {
                    requestEnctype = Interop.KERB_ETYPE.rc4_hmac;
                }
                else if (encTypeString.Equals("AES128"))
                {
                    requestEnctype = Interop.KERB_ETYPE.aes128_cts_hmac_sha1;
                }
                else if (encTypeString.Equals("AES256") || encTypeString.Equals("AES"))
                {
                    requestEnctype = Interop.KERB_ETYPE.aes256_cts_hmac_sha1;
                }
                else if (encTypeString.Equals("DES"))
                {
                    requestEnctype = Interop.KERB_ETYPE.des_cbc_md5;
                }
                else
                {
                    Console.WriteLine("Desteklenmeyen etype : {0}", encTypeString);
                    return;
                }
            }

            // U2U talepleri icin
            if (arguments.ContainsKey("/u2u"))
            {
                u2u = true;
            }
            
            if (arguments.ContainsKey("/service"))
            {
                service = arguments["/service"];
            }
            else if (!u2u)
            {
                Console.WriteLine("[X] Bir veya daha fazla '/service:sname/server.domain.com' belirtimi gereklidir");
                return;
            }

            if (arguments.ContainsKey("/servicetype")) {
                serviceType = arguments["/servicetype"];
            }

            if (arguments.ContainsKey("/servicekey")) {
                servicekey = arguments["/servicekey"];
            }

            if (u2u || !String.IsNullOrEmpty(servicekey))
            {
                // ticket sahtelemek icin komut argumanlarini yazdir
                if (arguments.ContainsKey("/printargs"))
                {
                    printargs = true;
                }
            }


            if (arguments.ContainsKey("/asrepkey")) {
                asrepkey = arguments["/asrepkey"];
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

            }

            // taleplerde domain'i manuel olarak belirtmek icin
            if (arguments.ContainsKey("/targetdomain"))
            {
                targetDomain = arguments["/targetdomain"];
            }

            // bir PA-for-User PA veri bolumu eklemek icin
            if (arguments.ContainsKey("/targetuser"))
            {
                targetUser = arguments["/targetuser"];
            }

            // bir KDC proxy kullanmak icin
            if (arguments.ContainsKey("/proxyurl"))
            {
                proxyUrl = arguments["/proxyurl"];
            }

            if (arguments.ContainsKey("/dmsa"))
            {
                dmsa = true;
            }

            if (arguments.ContainsKey("/ticket"))
            {
                string kirbi64 = arguments["/ticket"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    Ask.TGS(kirbi, service, requestEnctype, outfile, ptt, dc, true, enterprise, false, opsec, tgs, targetDomain, servicekey, asrepkey, u2u, targetUser, printargs, proxyUrl, keyList, dmsa, serviceType, default);
                    return;
                }
                else if (File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = File.ReadAllBytes(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    Ask.TGS(kirbi, service, requestEnctype, outfile, ptt, dc, true, enterprise, false, opsec, tgs, targetDomain, servicekey, asrepkey, u2u, targetUser, printargs, proxyUrl, keyList, dmsa, serviceType, default);
                    return;
                }
                else
                {
                    Console.WriteLine("\r\n[X] /ticket:X bir .kirbi dosyasi veya base64 kodlu bir .kirbi olmalidir\r\n");
                }
                return;
            }
            else
            {
                if(arguments.ContainsKey("/user") || arguments.ContainsKey("/password") || arguments.ContainsKey("/dc") || arguments.ContainsKey("/u2u") || arguments.ContainsKey("/tgs"))
                    Console.WriteLine("\r\n[X] Bir /ticket:X saglanmalidir!\r\n");
                else
                    Ask.TGS(null, service, requestEnctype, outfile, ptt, dc, true, enterprise, false, opsec, tgs, targetDomain, servicekey, asrepkey, u2u, targetUser, printargs, proxyUrl, keyList, dmsa, serviceType, targetLuid);
            }
        }
    }
}