using System;
using System.Collections.Generic;
using Rubeus.lib.Interop;


namespace Rubeus.Commands
{
    public class Triage : ICommand
    {
        public static string CommandName => "triage";

        public void Execute(Dictionary<string, string> arguments)
        {
            if (Helpers.IsHighIntegrity())
            {
                Console.WriteLine("\r\nEylem: Kerberos Ticket'lerini Degerlendir (Tum Kullanicilar)\r\n");
            }
            else
            {
                Console.WriteLine("\r\nEylem: Kerberos Ticket'lerini Degerlendir (Mevcut Kullanici)\r\n");
            }

            LUID targetLuid = new LUID();
            string targetUser = "";
            string targetService = "";
            string targetServer = "";

            if (arguments.ContainsKey("/luid"))
            {
                try
                {
                    targetLuid = new LUID(arguments["/luid"]);
                }
                catch
                {
                    Console.WriteLine("[X] Gecersiz LUID formati ({0})\r\n", arguments["/luid"]);
                    return;
                }
            }

            if (arguments.ContainsKey("/user"))
            {
                targetUser = arguments["/user"];
            }

            if (arguments.ContainsKey("/service"))
            {
                targetService = arguments["/service"];
            }

            if (arguments.ContainsKey("/server"))
            {
                targetServer = arguments["/server"];
            }

            // belirtilen hedefleme secenekleriyle ticket'lari (tam veriyle) cikar
            List<LSA.SESSION_CRED> sessionCreds = LSA.EnumerateTickets(false, targetLuid, targetService, targetUser, targetServer, true);
            // ticket'lari "Full" formatinda goster
            LSA.DisplaySessionCreds(sessionCreds, LSA.TicketDisplayFormat.Triage);
        }
    }
}