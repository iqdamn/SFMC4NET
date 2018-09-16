namespace SFMC4NET
{
    /// <summary>
    /// General settings for Salesforce Marketing Cloud API
    /// </summary>
    internal static class GeneralSettings
    {
        #region URLs

        //Authentication token
        public static string RequestToken_URL = "https://auth.exacttargetapis.com/v1/requestToken";

        //CHANGE this: it depends on where your SFMC implementation lives, change s7 for the corresponding stack
        public static string MainService_URL = "https://webservice.s7.exacttarget.com/Service.asmx";

        public static string Upsert_URL = "https://www.exacttargetapis.com/hub/v1/dataevents/key:{DE}/rowset";

        #endregion

        #region Credentials

        //API credentials
        public static string ClientId = "xxxx";
        public static string Secret = "xxxx";

        #endregion

        public static uint BatchSize = 2500;
    }
}
