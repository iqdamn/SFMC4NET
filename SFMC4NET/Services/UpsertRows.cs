using Polly;
using RestSharp;
using SFMC4NET.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        private string UpsertURL = "https://www.exacttargetapis.com/hub/v1/dataevents/key:{DE}/rowset";
        
        public async Task SendRows<T>(string DataExtensionExternalKey, List<T> list)
        {
            if (token == null || !token.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken();
                token = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            await InsertRows(DataExtensionExternalKey, list);
        }

        private async Task InsertRows<T>(string DataExtensionExternalKey, IList<T> list)
        {
            //Setting up the request
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("Authorization", "Bearer " + token.Token, ParameterType.HttpHeader);
            
            string upsertURL = UpsertURL.Replace("{DE}", DataExtensionExternalKey);

            RestClient client = new RestClient(upsertURL);

            PropertiesManager propertiesManager = new PropertiesManager();
            
            StringBuilder message = new StringBuilder();
            message.Append("[");

            var lastItem = list.Last();    

            foreach (var item in list)
            {
                var keys = propertiesManager.GetKeys(item);
                var values = propertiesManager.GeValues(item);

                if (keys != null && keys.Count > 0)
                {
                    message.Append("{\"keys\":{");
                    var lastKey = keys.Last();
                    foreach (var key in keys)
                    {
                        message.Append($"\"{key.Key}\":\"{key.Value}\"");
                        if(key.Key != lastKey.Key)
                        {
                            message.Append(",");
                        }
                    }
                    
                    message.Append("},");
                }

                if(values != null && values.Count > 0)
                {
                    message.Append("\"values\":{");
                    var lastValue = values.Last();

                    foreach(var val in values)
                    {
                        message.Append($"\"{val.Key}\":\"{val.Value}\"");

                        if(lastValue.Key != val.Key)
                        {
                            message.Append(",");
                        }

                    }
                    message.Append("}}");
                }

                if(!item.Equals(lastItem))
                {
                    message.Append(",");
                }
            }

            message.Append("]");

            request.AddParameter("application/json", message.ToString(), ParameterType.RequestBody);

            //Using Polly Retry policy
            var policy = Policy.Handle<WebException>()
                .Or<HttpRequestException>()
                .OrResult<IRestResponse>(r => r.StatusCode != HttpStatusCode.OK)
                .RetryAsync(3);

            var policyResult = await policy.ExecuteAndCaptureAsync(() => client.ExecuteTaskAsync(request));
            string serviceResult;

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                IRestResponse webResponse = policyResult.Result;
                serviceResult = webResponse.Content;
            }
            else
            {
                throw policyResult.FinalException;
            }
        }       
    }
}
