using System;
using System.Collections.Generic;
using System.IO;


namespace Rubeus.Commands
{
    public class RenewCommand : ICommand
    {
        public static string CommandName => "renew";

        public void Execute(Dictionary<string, string> arguments)
        {
            string outfile = "";
            bool ptt = false;
            string dc = "";

            if (arguments.ContainsKey("/outfile"))
            {
                outfile = arguments["/outfile"];
            }

            if (arguments.ContainsKey("/ptt"))
            {
                ptt = true;
            }

            if (arguments.ContainsKey("/dc"))
            {
                dc = arguments["/dc"];
            }

            if (arguments.ContainsKey("/ticket"))
            {
                string kirbi64 = arguments["/ticket"];
                byte[] kirbiBytes = null;

                if (Helpers.IsBase64String(kirbi64))
                {
                    kirbiBytes = Convert.FromBase64String(kirbi64);
                }
                else if (File.Exists(kirbi64))
                {
                    kirbiBytes = File.ReadAllBytes(kirbi64);
                }

                if(kirbiBytes == null)
                {
                    Console.WriteLine("\r\n[X] /ticket:X bir .kirbi dosyasi veya base64 kodlu bir .kirbi olmalidir\r\n");
                }
                else
                {
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    if (arguments.ContainsKey("/autorenew"))
                    {
                        Console.WriteLine("[*] Eylem: Ticket'i Otomatik Yenile\r\n");
                        // TGT'yi yenileme sinirina kadar otomatik yenilemek istiyorsak
                        Renew.TGTAutoRenew(kirbi, dc);
                    }
                    else
                    {
                        Console.WriteLine("[*] Eylem: Ticket'i Yenile\r\n");
                        // aksi halde tek seferlik bir yenileme islemi
                        byte[] blah = Renew.TGT(kirbi, outfile, ptt, dc);
                    }
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