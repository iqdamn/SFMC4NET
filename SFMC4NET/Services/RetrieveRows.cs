using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                return string.Empty;

            if (status != "OK")
            {
                requestId = requestResponse;
            }

            var results = retrieveResponseMsg.SelectToken("$['RetrieveResponseMsg']['Results']");

            PropertiesManager propertiesManager = new PropertiesManager();
            
            //WTF?, DRY!
            if (results is JArray)
            {
                foreach (var prop in results)
                {
                    Dictionary<string, string> propertiesDictionary = new Dictionary<string, string>();
                    var properties = prop.SelectToken("$['Properties']['Property']");
                    propertiesDictionary.Clear();
                    var propList = properties.ToList();
                    foreach (var item in propList)
                    {
                        propertiesDictionary.Add(item["Name"].ToString(), item["Value"].ToString());
                    }

                    T instance = Activator.CreateInstance<T>();
                    propertiesManager.CreateInstance<T>(instance, propertiesDictionary);

                    if (instance != null)
                        list.Add(instance);
                }
            }
            else
            {
                if(results != null)
                {
                    var properties = results.SelectToken("$['Properties']['Property']");
                    var propList = properties.ToList();
                    Dictionary<string, string> propertiesDictionary = new Dictionary<string, string>();

                    foreach (var item in propList)
                    {
                        propertiesDictionary.Add(item["Name"].ToString(), item["Value"].ToString());
                    }

                    T instance = Activator.CreateInstance<T>();
                    propertiesManager.CreateInstance<T>(instance, propertiesDictionary);

                    if (instance != null)
                        list.Add(instance);
                }
            }

            return requestId;
        }


        /// <summary>
        /// Generates the request message to be send in the SOAP call
        /// </summary>
        /// <returns>The generated request message</returns>
        private async Task<string> GetRequestMessage<T>(string DataExtensionExternalKey, RequestParameters parameter = null)
        {
            if (accessToken == null || !accessToken.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            StringBuilder builder = new StringBuilder();

            builder.Append($"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><fueloauth xmlns=\"http://exacttarget.com\">{accessToken.Token}</fueloauth></s:Header>");
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
