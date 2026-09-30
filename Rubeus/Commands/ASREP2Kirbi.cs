using System;
using System.Collections.Generic;
using System.IO;
using Asn1;
using Rubeus.lib.Interop;


namespace Rubeus.Commands
{
    public class ASREP2Kirbi : ICommand
    {
        public static string CommandName => "asrep2kirbi";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("\r\n[*] Eylem: AS-REP'ten Kirbi'ye");

            AsnElt asrep = null;
            byte[] key = null;
            Interop.KERB_ETYPE encType = Interop.KERB_ETYPE.aes256_cts_hmac_sha1; //eger /enctype belirtilmemisse varsayilan
            bool ptt = false;
            string outfile = "";
            LUID luid = new LUID();

            if (arguments.ContainsKey("/outfile"))
            {
                outfile = arguments["/outfile"];
            }

            if (arguments.ContainsKey("/ptt"))
            {
                ptt = true;
            }

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

            if (arguments.ContainsKey("/asrep"))
            {
                string buffer = arguments["/asrep"];

                if (Helpers.IsBase64String(buffer))
                {
                    byte[] bufferBytes = Convert.FromBase64String(buffer);

                    asrep = AsnElt.Decode(bufferBytes);
                }
                else if (File.Exists(buffer))
                {
                    byte[] bufferBytes = File.ReadAllBytes(buffer);
                    asrep = AsnElt.Decode(bufferBytes);
                }
                else
                {
                    Console.WriteLine("\r\n[X] /asrep:X bir dosya veya base64 kodlu bir AS-REP mesaji olmalidir\r\n");
                    return;
                }
            }
            else
            {
                Console.WriteLine("\r\n[X] Bir /asrep:X saglanmalidir!\r\n");
                return;
            }

            if (arguments.ContainsKey("/key"))
            {
                if (Helpers.IsBase64String(arguments["/key"]))
                {
                    key = Convert.FromBase64String(arguments["/key"]);
                }
                else
                {
                    Console.WriteLine("\r\n[X] /key:X base64 kodlu bir istemci anahtari olmalidir\r\n");
                    //return;
                }
            }
            else if (arguments.ContainsKey("/keyhex"))
            {
                key = Helpers.StringToByteArray(arguments["/keyhex"]);
            }
            else
            {
                Console.WriteLine("\r\n[X]Bir /key:X veya /keyhex:X saglanmalidir!");
                return;
            }

            if (arguments.ContainsKey("/enctype"))
            {
                string encTypeString = arguments["/enctype"].ToUpper();

                if (encTypeString.Equals("RC4") || encTypeString.Equals("NTLM"))
                {
                    encType = Interop.KERB_ETYPE.rc4_hmac;
                }
                else if (encTypeString.Equals("AES128"))
                {
                    encType = Interop.KERB_ETYPE.aes128_cts_hmac_sha1;
                }
                else if (encTypeString.Equals("AES256") || encTypeString.Equals("AES"))
                {
                    encType = Interop.KERB_ETYPE.aes256_cts_hmac_sha1;
                }
                else if (encTypeString.Equals("DES"))
                {
                    encType = Interop.KERB_ETYPE.des_cbc_md5;
                }
            }

            Ask.HandleASREP(asrep, encType, Helpers.ByteArrayToString(key), outfile, ptt, luid, false, true);
        }
    }
}