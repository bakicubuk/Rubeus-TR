using System;
using System.Collections.Generic;
using System.IO;


namespace Rubeus.Commands
{
    public class Describe : ICommand
    {
        public static string CommandName => "describe";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("\r\n[*] Eylem: Ticket'i Acikla\r\n");
            byte[] serviceKey = null;
            byte[] asrepKey = null;
            byte[] krbKey = null;
            string serviceUser = "";
            string serviceDomain = "";
            string desPlainText = "";



            if (arguments.ContainsKey("/servicekey"))
            {
                serviceKey = Helpers.StringToByteArray(arguments["/servicekey"]);
            }
            if (arguments.ContainsKey("/asrepkey"))
            {
                asrepKey = Helpers.StringToByteArray(arguments["/asrepkey"]);
            }
            if (arguments.ContainsKey("/krbkey"))
            {
                krbKey = Helpers.StringToByteArray(arguments["/krbkey"]);
            }
            if (arguments.ContainsKey("/desplaintext"))
            {
                desPlainText = arguments["/desplaintext"];
            }

            // AES256 kullanilirken servis ticket hash'i uretmek icin
            if (arguments.ContainsKey("/serviceuser"))
            {
                serviceUser = arguments["/serviceuser"];
            }
            if (arguments.ContainsKey("/servicedomain"))
            {
                serviceDomain = arguments["/servicedomain"];
            }

            if (arguments.ContainsKey("/ticket"))
            {
                string kirbi64 = arguments["/ticket"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    LSA.DisplayTicket(kirbi, 2, false, false, true, false, serviceKey, asrepKey, serviceUser, serviceDomain, krbKey, null, desPlainText);
                }
                else if (File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = File.ReadAllBytes(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    LSA.DisplayTicket(kirbi, 2, false, false, true, false, serviceKey, asrepKey, serviceUser, serviceDomain, krbKey, null, desPlainText);
                }
                else
                {
                    Console.WriteLine("\r\n[X] /ticket:X bir .kirbi dosyasi veya base64 kodlu bir .kirbi olmalidir\r\n");
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
