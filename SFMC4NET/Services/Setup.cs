using SFMC4NET.Entities;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        private AccessToken accessToken;
        private string clientId = string.Empty;
        private string secret = string.Empty;
        private string stack = "s7";
        private string MainServiceURL = "https://webservice.{Stack}.exacttarget.com/Service.asmx";
        private string AuthenticationURL = "https://auth.exacttargetapis.com/v1/requestToken";
        private string UpsertURL = "https://www.exacttargetapis.com/hub/v1/dataevents/key:{DE}/rowset";
        private string EventsURL = "https://www.exacttargetapis.com/interaction/v1/events";
        private string ContentURL = "https://www.exacttargetapis.com/asset/v1/content";
        private string BaseRESTURL = "https://www.exacttargetapis.com";
        private string serviceURL = string.Empty;
        private string tenantSubdomain = string.Empty;
        private bool useOAuth20 = true;
        
        private DataExtensionManager()
        {
        }

        public static DataExtensionManager Build { get { return new DataExtensionManager(); } }

        public DataExtensionManager SetStack(string stackNumber)
        {
            stack = stackNumber;
            serviceURL = MainServiceURL.Replace("{Stack}", stack);
            return this;
        }

        public DataExtensionManager SetMainServiceURL(string url)
        {
            MainServiceURL = url;
            return this;
        }

        public DataExtensionManager SetAuthenticationURL(string url)
        {
            AuthenticationURL = url;
            return this;
        }

        public DataExtensionManager UsingCredentials(string ClientId, string Secret)
        {
            clientId = ClientId;
            secret = Secret;
            return this;
        }

        public DataExtensionManager UseOAuth2(bool useOAuth)
        {
            useOAuth20 = useOAuth;
            return this;
        }

        public DataExtensionManager UsingToken(string token)
        {
            accessToken = new AccessToken(token);
            return this;
        }

        public DataExtensionManager SetTenantSubdomain(string subdomain)
        {
            tenantSubdomain = subdomain;
            MainServiceURL = $"https://{subdomain}.soap.marketingcloudapis.com/Service.asmx";
            serviceURL = MainServiceURL;

            if (!useOAuth20)
                AuthenticationURL = $"https://{subdomain}.auth.marketingcloudapis.com/v1/requestToken";
            else
                AuthenticationURL = $"https://{subdomain}.auth.marketingcloudapis.com/v2/token";

            UpsertURL = "https://" + subdomain + ".rest.marketingcloudapis.com/hub/v1/dataevents/key:{DE}/rowset";
            EventsURL = $"https://{subdomain}.rest.marketingcloudapis.com/interaction/v1/events";
            ContentURL = $"https://{subdomain}.rest.marketingcloudapis.com/asset/v1/content";
            BaseRESTURL = $"https://{subdomain}.rest.marketingcloudapis.com";

            return this;
        }
    }
}
