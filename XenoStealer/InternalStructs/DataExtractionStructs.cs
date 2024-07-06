using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoStealer
{
    public static class DataExtractionStructs
    {
        public struct GeckoLogin 
        {
            public string hostname;
            public string username;
            public string password;

            public GeckoLogin(string _hostname, string _username, string _password) 
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
            public ulong expiry;
            public bool isSecure;
            public bool isHttpOnly;
            public bool expired;

            public GeckoCookie(string _domain, string _path, string _name, string _value, ulong _expiry, bool _isSecure, bool _isHttpOnly) 
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


    }
}
