using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFMC4NET.Entities;
using SFMC4NET.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        public async Task<string> StartAutomation(string automationExternalKey)
        {
            var automationInfo = await GetAutomationInfo(automationExternalKey);
            var requestMessage = await GetStartAutomationRequestMessage(automationInfo);

            ServiceHandler client = new ServiceHandler();
            string response = await client.InvokeSOAPService(requestMessage, this.serviceURL, "Perform");

            return GetStartStatusFromResponse(response);
        }

        private async Task<string> GetStartAutomationRequestMessage(AutomationInfo automationInfo)
        {
            if (accessToken == null || !accessToken.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            StringBuilder builder = new StringBuilder();

            builder.Append($"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><fueloauth xmlns=\"http://exacttarget.com\">{accessToken.Token}</fueloauth></s:Header>");
            builder.Append("<s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><PerformRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
            builder.Append($"<Options/><Action>start</Action><Definitions><Definition xsi:type=\"Automation\"><PartnerKey xsi:nil=\"true\"/><ObjectID>{automationInfo.ObjectID}</ObjectID></Definition></Definitions>");

            builder.Append("</PerformRequestMsg></s:Body></s:Envelope>");

            return builder.ToString();
        }

        public async Task<AutomationInfo> GetAutomationInfo(string automationExternalKey)
        {
            if (accessToken == null || !accessToken.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            StringBuilder builder = new StringBuilder();

            builder.Append($"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><fueloauth xmlns=\"http://exacttarget.com\">{accessToken.Token}</fueloauth></s:Header>");
            builder.Append("<s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><RetrieveRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\"><RetrieveRequest>");
            builder.Append($"<ObjectType>Automation</ObjectType><Properties>ProgramID</Properties><Properties>Status</Properties><Properties>Name</Properties><Properties>CustomerKey</Properties><Filter xsi:type=\"SimpleFilterPart\"><Property>CustomerKey</Property><SimpleOperator>equals</SimpleOperator><Value>{automationExternalKey}</Value></Filter>");

            builder.Append("</RetrieveRequest></RetrieveRequestMsg></s:Body></s:Envelope>");

            ServiceHandler client = new ServiceHandler();
            string response = await client.InvokeSOAPService(builder.ToString(), this.serviceURL);

            return GetAutomationInfoFromResponse(response);
        }

        private AutomationInfo GetAutomationInfoFromResponse(string response)
        {
            string requestId = string.Empty;

            if (string.IsNullOrEmpty(response))
                return null;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            JObject jObject = JObject.Parse(jsonText);

            string status = string.Empty;
            string requestResponse = string.Empty;

            var retrieveResponseMsg = jObject.Descendants()
                .Where(x => x is JObject)
                .Where(x => x["RetrieveResponseMsg"] != null).First();

            if (retrieveResponseMsg != null)
            {
                status = (string)retrieveResponseMsg.SelectToken("$['RetrieveResponseMsg']['OverallStatus']");
                requestResponse = (string)retrieveResponseMsg.SelectToken("$['RetrieveResponseMsg']['RequestID']");
            }
            else
                return null;

            if (status != "OK")
            {
                requestId = requestResponse;
            }

            AutomationInfo automationInfo = new AutomationInfo();
            automationInfo.Status = status;
            
            var results = retrieveResponseMsg.SelectToken("$['RetrieveResponseMsg']['Results']");

            if (results != null)
            {
                var customerKey = results.Value<string>("CustomerKey");
                var objectId = results.Value<string>("ObjectID");
                var name = results.Value<string>("Name");

                automationInfo.CustomerKey = customerKey;
                automationInfo.ObjectID = objectId;
                automationInfo.Name = name;
            }
            

            return automationInfo;
        }

        private string GetStartStatusFromResponse(string response)
        {
            string requestId = string.Empty;

            if (string.IsNullOrEmpty(response))
                return null;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            JObject jObject = JObject.Parse(jsonText);

            string status = string.Empty;
            string requestResponse = string.Empty;

            var retrieveResponseMsg = jObject.Descendants()
                .Where(x => x is JObject)
                .Where(x => x["PerformResponseMsg"] != null).First();

            if (retrieveResponseMsg != null)
            {
                status = (string)retrieveResponseMsg.SelectToken("$['PerformResponseMsg']['OverallStatus']");
                requestResponse = (string)retrieveResponseMsg.SelectToken("$['PerformResponseMsg']['RequestID']");
            }
            else
                return null;

            return status;
        }
    }
}
