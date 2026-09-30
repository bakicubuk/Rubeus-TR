using System;
using System.Collections.Generic;


namespace Rubeus.Commands
{
    public class HarvestCommand : ICommand
    {
        public static string CommandName => "harvest";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("[*] Eylem: TGT Toplama (otomatik yenileme ile)");

            string targetUser = null;
            int monitorInterval = 60; // yeni TGT'lerin ne siklikta kontrol edilecegi
            int displayInterval = 1200; // calisan TGT kumesinin ne siklikta gosterilecegi
            string registryBasePath = null;
            bool nowrap = false;
            int runFor = 0;

            if (arguments.ContainsKey("/nowrap"))
            {
                nowrap = true;
            }
            if (arguments.ContainsKey("/filteruser"))
            {
                targetUser = arguments["/filteruser"];
            }
            if (arguments.ContainsKey("/targetuser"))
            {
                targetUser = arguments["/targetuser"];
            }
            if (arguments.ContainsKey("/interval"))
            {
                monitorInterval = Int32.Parse(arguments["/interval"]);
                displayInterval = Int32.Parse(arguments["/interval"]);
            }
            if (arguments.ContainsKey("/monitorinterval"))
            {
                monitorInterval = Int32.Parse(arguments["/monitorinterval"]);
            }
            if (arguments.ContainsKey("/displayinterval"))
            {
                displayInterval = Int32.Parse(arguments["/displayinterval"]);
            }
            if (arguments.ContainsKey("/registry"))
            {
                registryBasePath = arguments["/registry"];
            }
            if (arguments.ContainsKey("/runfor"))
            {
                runFor = Int32.Parse(arguments["/runfor"]);
            }

            if (!String.IsNullOrEmpty(targetUser))
            {
                Console.WriteLine("[*] Hedef kullanici     : {0:x}", targetUser);
            }
            Console.WriteLine("[*] Yeni TGT'ler icin her {0} saniyede bir izleniyor", monitorInterval);
            Console.WriteLine("[*] Calisan TGT onbellegi her {0} saniyede bir gosteriliyor", displayInterval);
            if (runFor > 0)
            {
                Console.WriteLine("[*] Toplama {0} saniye boyunca calistiriliyor", runFor);
            }
            Console.WriteLine("");

            var harvester = new Harvest(monitorInterval, displayInterval, true, targetUser, registryBasePath, nowrap, runFor);
            harvester.HarvestTicketGrantingTickets();
        }
    }
}
