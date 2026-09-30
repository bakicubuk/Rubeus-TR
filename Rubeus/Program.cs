using Rubeus.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rubeus
{
    public class Program
    {
        // ticket ciktisinin satir kaydirmali (wrap) olup olmayacagini belirten global degisken
        public static bool wrapTickets = true;

        public static bool Debug = false;

        private static void FileExecute(string commandName, Dictionary<string, string> parsedArgs)
        {
            // stdout/err dosyaya yonlendirilmis sekilde calistir

            string file = parsedArgs["/consoleoutfile"];

            TextWriter realStdOut = Console.Out;
            TextWriter realStdErr = Console.Error;

            using (StreamWriter writer = new StreamWriter(file, true))
            {
                writer.AutoFlush = true;
                Console.SetOut(writer);
                Console.SetError(writer);

                MainExecute(commandName, parsedArgs);

                Console.Out.Flush();
                Console.Error.Flush();
            }
            Console.SetOut(realStdOut);
            Console.SetError(realStdErr);
        }

        private static void MainExecute(string commandName, Dictionary<string,string> parsedArgs)
        {
            // ana calistirma mantigi

            Info.ShowLogo();

            try
            {
                // eger bir konsol varsa unicode karakterleri dogru yazdir
                if(IsConsolePresent()) Console.OutputEncoding = Encoding.UTF8;

                var commandFound = new CommandCollection().ExecuteCommand(commandName, parsedArgs);

                // komut adi icin herhangi bir komut bulunamadiysa kullanimi goster
                if (commandFound == false)
                    Info.ShowUsage();
            }
            catch (Exception e)
            {
                Console.WriteLine("\r\n[!] Islenmeyen Rubeus hatasi:\r\n");
                Console.WriteLine(e);
            }
        }

        public static string MainString(string command)
        {
            // girdi olarak verilen string komutu calistirip sonucu string olarak donduren yardimci fonksiyon
            //  PSRemoting ile calistirma icin kullanislidir

            string[] args = command.Split();

            var parsed = ArgumentParser.Parse(args);
            if (parsed.ParsedOk == false)
            {
                Info.ShowLogo();
                Info.ShowUsage();
                return "Argumanlar ayristirilirken hata olustu: ${command}";
            }

            var commandName = args.Length != 0 ? args[0] : "";

            TextWriter realStdOut = Console.Out;
            TextWriter realStdErr = Console.Error;
            TextWriter stdOutWriter = new StringWriter();
            TextWriter stdErrWriter = new StringWriter();
            Console.SetOut(stdOutWriter);
            Console.SetError(stdErrWriter);

            MainExecute(commandName, parsed.Arguments);

            Console.Out.Flush();
            Console.Error.Flush();
            Console.SetOut(realStdOut);
            Console.SetError(realStdErr);

            string output = "";
            output += stdOutWriter.ToString();
            output += stdErrWriter.ToString();

            return output;
        }

        private static bool IsConsolePresent()
        {
            return Interop.GetConsoleWindow() != IntPtr.Zero;
        }

        public static void Main(string[] args)
        {
            // komut satiri argumanlarini ayristirmaya calis, basarisiz olursa kullanimi goster ve cik
            var parsed = ArgumentParser.Parse(args);
            if (parsed.ParsedOk == false) {
                Info.ShowLogo();
                Info.ShowUsage();
                return;
            }

            var commandName = args.Length != 0 ? args[0] : "";

            if (parsed.Arguments.ContainsKey("/nowrap"))
            {
                wrapTickets = false;
            }

            if (parsed.Arguments.ContainsKey("/debug"))
            {
                Debug = true;
            }

            if (parsed.Arguments.ContainsKey("/consoleoutfile")) {
                // ciktiyi belirtilen dosyaya yonlendir
                FileExecute(commandName, parsed.Arguments);
            }
            else
            {
                MainExecute(commandName, parsed.Arguments);
            }
        }
    }
}
