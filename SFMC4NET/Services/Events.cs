using Newtonsoft.Json;
using Polly;
using RestSharp;
using SFMC4NET.Entities;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        public async Task<string> TriggerEvent(Event customEvent)
        {
            string instanceId = string.Empty;

            if (accessToken == null || !accessToken.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            //Setting up the request
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("Authorization", "Bearer " + accessToken.Token, ParameterType.HttpHeader);

            RestClient client = new RestClient(EventsURL);

            request.AddParameter("application/json", customEvent.ToJson(), ParameterType.RequestBody);

            //Using Polly Retry policy
            var policy = Policy.Handle<WebException>()
                .Or<HttpRequestException>()
                .OrResult<IRestResponse>(r => r.StatusCode != HttpStatusCode.Created)
                .RetryAsync(3);

            var policyResult = await policy.ExecuteAndCaptureAsync(() => client.ExecuteTaskAsync(request));
            Dictionary<string, string> result;

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                IRestResponse webResponse = policyResult.Result;
                result = JsonConvert.DeserializeObject<Dictionary<string, string>>(webResponse.Content);
            }
            else
            {
                throw new System.Exception($"{policyResult.FinalHandledResult.Content}");
            }

            instanceId = result["eventInstanceId"];

            return instanceId;
        }
    }
}
