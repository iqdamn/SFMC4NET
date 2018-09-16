using Newtonsoft.Json;
using SFMC4NET.Attributes;
using SFMC4NET.Entities;
using SFMC4NET.Infrastructure;
using SFMC4NET.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;

namespace SFMC4NET.Services
{
    public class RetrieveRows<T> where T : class
    {
        private string dataextensionExternalKey;
        private AccessToken token;

        public RetrieveRows(string DataExtensionExternalKey, AccessToken accessToken = null)
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(DataExtensionAttribute),true);

            if (attributes == null || attributes.Length <= 0)
                throw new ArgumentException("T must be a class decorated with the attribute DataExtensionAttribute");

            dataextensionExternalKey = DataExtensionExternalKey;

            token = accessToken;
        }

        public async Task<IList<T>> GetRows(string filter = "")
        {
            List<T> rowList = Activator.CreateInstance<List<T>>();
            RequestParameters parameter = new RequestParameters();

            if (!string.IsNullOrEmpty(filter))
            {
                parameter.Filter = filter;
            }

            string requestMessage = await GetRequestMessage(parameter);
            ServiceHandler client = new ServiceHandler();
            
            string response = await client.InvokeSOAPService(requestMessage);
            
            while (!string.IsNullOrEmpty(parameter.RequestId = GetRowList(rowList, response)))
            {
                requestMessage = await GetRequestMessage(parameter);
                response = await client.InvokeSOAPService(requestMessage);
            }
            
            return rowList;
        }

        private string GetRowList(IList<T> list, string response)
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
        private async Task<string> GetRequestMessage(RequestParameters parameter = null)
        {
            if (token == null || !token.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken();
                token = await tokenBuilder.GetAccessToken();
            }

            StringBuilder builder = new StringBuilder();

            builder.Append($"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><fueloauth xmlns=\"http://exacttarget.com\">{token.Token}</fueloauth></s:Header>");
            builder.Append("<s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><RetrieveRequestMsg xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
            builder.Append($"<RetrieveRequest><ObjectType>DataExtensionObject[{dataextensionExternalKey}]</ObjectType>");

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
