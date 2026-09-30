using System;
using System.Collections.Generic;


namespace Rubeus.Commands
{
    public class Hash : ICommand
    {
        public static string CommandName => "hash";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("\r\n[*] Eylem: Sifre Hash'lerini Hesapla\r\n");

            string user = "";
            string domain = "";
            string password = "";

            if (arguments.ContainsKey("/domain"))
            {
                domain = arguments["/domain"];
            }

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

            if (arguments.ContainsKey("/password"))
            {
                password = arguments["/password"];
            }
            else
            {
                Console.WriteLine("[X] /password:X saglanmalidir!");
                return;
            }

            Crypto.ComputeAllKerberosPasswordHashes(password, user, domain);
        }
    }
}