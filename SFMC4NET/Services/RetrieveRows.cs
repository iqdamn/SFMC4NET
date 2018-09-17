using Newtonsoft.Json;
using SFMC4NET.Entities;
using SFMC4NET.Infrastructure;
using SFMC4NET.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        private AccessToken token;
        private string clientId = string.Empty;
        private string secret = string.Empty;
        private string stack = "s7";
        private const string MainServiceURL = "https://webservice.{Stack}.exacttarget.com/Service.asmx";
        private string serviceURL = string.Empty;
        
        public static DataExtensionManager Build { get { return new DataExtensionManager(); } }

        private DataExtensionManager()
        {
        }

        public DataExtensionManager SetStack(string stackNumber)
        {
            stack = stackNumber;
            serviceURL = MainServiceURL.Replace("{Stack}", stack);
            return this;
        }

        public DataExtensionManager UsingCredentials(string ClientId, string Secret)
        {
            clientId = ClientId;
            secret = Secret;
            return this;
        }

        public async Task<IList<T>> GetRows<T>(string DataExtensionExternalKey, string filter = "")
        {
            List<T> rowList = Activator.CreateInstance<List<T>>();
            RequestParameters parameter = new RequestParameters();

            if (!string.IsNullOrEmpty(filter))
            {
                parameter.Filter = filter;
            }

            string requestMessage = await GetRequestMessage<T>(DataExtensionExternalKey, parameter);
            ServiceHandler client = new ServiceHandler();
            
            string response = await client.InvokeSOAPService(requestMessage, this.serviceURL);
            
            while (!string.IsNullOrEmpty(parameter.RequestId = GetRowList(rowList, response)))
            {
                requestMessage = await GetRequestMessage<T>(DataExtensionExternalKey, parameter);
                response = await client.InvokeSOAPService(requestMessage, this.serviceURL);
            }
            
            return rowList;
        }

        private string GetRowList<T>(IList<T> list, string response)
        {
            string requestId = string.Empty;

            if (string.IsNullOrEmpty(response))
                return string.Empty;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);
            string jsonText = JsonConvert.SerializeXmlNode(doc);

            Root root = Root.FromJson(jsonText);

            if (root == null || root.SoapEnvelope == null || root.SoapEnvelope.SoapBody.RetrieveResponseMsg == null || root.SoapEnvelope.SoapBody.RetrieveResponseMsg.Results == null || root.SoapEnvelope.SoapBody.RetrieveResponseMsg.Results.Length <= 0)
                return string.Empty;

            PropertiesManager propertiesManager = new PropertiesManager();
            if(root.SoapEnvelope.SoapBody.RetrieveResponseMsg.OverallStatus != "OK")
            {
                requestId = root.SoapEnvelope.SoapBody.RetrieveResponseMsg.RequestId.ToString();
            }
            
            foreach (Result result in root.SoapEnvelope.SoapBody.RetrieveResponseMsg.Results)
            {
                var propertiesDictionary = result.Properties.Property.ToDictionary(n => n.Name, v => v.Value);
                T instance = Activator.CreateInstance<T>();
                propertiesManager.CreateInstance<T>(instance, propertiesDictionary);

                if (instance != null)
                    list.Add(instance);
            }

            return requestId;
        }


        /// <summary>
        /// Generates the request message to be send in the SOAP call
        /// </summary>
        /// <returns>The generated request message</returns>
        private async Task<string> GetRequestMessage<T>(string DataExtensionExternalKey, RequestParameters parameter = null)
        {
            if (token == null || !token.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken();
                token = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            StringBuilder builder = new StringBuilder();

            builder.Append($"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><fueloauth xmlns=\"http://exacttarget.com\">{token.Token}</fueloauth></s:Header>");
            builder.Append("<s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><RetrieveRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
            builder.Append($"<RetrieveRequest><ObjectType>DataExtensionObject[{DataExtensionExternalKey}]</ObjectType>");

            if(parameter!= null && !string.IsNullOrEmpty(parameter.RequestId))
            {
                builder.Append($"<ContinueRequest>{parameter.RequestId}</ContinueRequest>");
            }

            //Let's get all the properties
            PropertiesManager propertiesManager = new PropertiesManager();
            foreach(string prop in propertiesManager.GetDataExtensionFields<T>())
            {
                builder.Append($"<Properties>{prop}</Properties>");
            }

            if(parameter != null && !string.IsNullOrEmpty(parameter.Filter))
            {
                builder.Append(parameter.Filter);
            }

            builder.Append("</RetrieveRequest></RetrieveRequestMsg></s:Body></s:Envelope>");

            return builder.ToString();
        }  
    }
}
