using System;
using System.Configuration;

namespace TagConverter
{
    internal class DSConfig
    {

        private const string CLIENT_ID = "DS_CLIENT_ID";
        private const string IMPERSONATED_USER_GUID = "DS_IMPERSONATED_USER_GUID";
        private const string TARGET_ACCOUNT_ID = "DS_TARGET_ACCOUNT_ID";
        private const string PRIVATE_KEY = "DS_PRIVATE_KEY";
        private const string DS_AUTH_SERVER = "DS_AUTH_SERVER";
        private const string PICKUP_DIR = "PICK_UP_DIR";
        private const string DROPOFF_DIR = "DROP_OFF_DIR";
        private const string TAGS = "CSV_TAGS";
        private const string DS_TAGS = "DS_TAGS";
        private const string DS_TAGS_ON = "DS_TAGS_";
        private const string FILE_NAME_APPEND = "DS_TAGS";

        static DSConfig()
        {
            PickupDir = GetSetting(PICKUP_DIR);
            DropoffDir = GetSetting(DROPOFF_DIR);
            CsvTags = GetSetting(TAGS);
            DsTags = GetSetting(DS_TAGS);
            ClientID = GetSetting(CLIENT_ID);
            ImpersonatedUserGuid = GetSetting(IMPERSONATED_USER_GUID);
            TargetAccountID = GetSetting(TARGET_ACCOUNT_ID);
            OAuthRedirectURI = "https://www.docusign.com";
            PrivateKey = GetSetting(PRIVATE_KEY);
            AuthServer = GetSetting(DS_AUTH_SERVER);
            AuthenticationURL = GetSetting(DS_AUTH_SERVER);
            API = "restapi/v2";
            PermissionScopes = "signature impersonation";
            JWTScope = "signature";
        }

        private static string GetSetting(string configKey)
        {
            string val = Environment.GetEnvironmentVariable(configKey)
                ?? ConfigurationManager.AppSettings.Get(configKey);

            if (PRIVATE_KEY.Equals(configKey) && "FALSE".Equals(val))
                return null;

            return val ?? "";
        }

        public static string ClientID { get; private set; }
        public static string ImpersonatedUserGuid { get; private set; }
        public static string TargetAccountID { get; private set; }
        public static string OAuthRedirectURI { get; private set; }
        public static string PrivateKey { get; private set; }
        private static string authServer;
        public static string AuthServer
        {
            get { return authServer; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value) && value.StartsWith("https://"))
                {
                    authServer = value.Substring(8);
                }
                else if (!String.IsNullOrWhiteSpace(value) && value.StartsWith("http://"))
                {
                    authServer = value.Substring(7);
                }
                else
                {
                    authServer = value;
                }
            }
        }
        public static string AuthenticationURL { get; private set; }
        public static string API { get; private set; }
        public static string PermissionScopes { get; private set; }
        public static string JWTScope { get; private set; }
        public static string PickupDir { get; private set; }
        public static string DropoffDir { get; private set; }
        public static string CsvTags { get; private set; }
        public static string DsTags { get; private set; }
    }
}
