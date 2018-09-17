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
    public class BearerToken
    {
        private string RequestToken_URL = "https://auth.exacttargetapis.com/v1/requestToken";

        public async Task<AccessToken> GetAccessToken(string clientid, string secret)
        {
            AccessToken token = null;

            RestClient client = new RestClient(RequestToken_URL);

            //Setting up the request
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");

            string jsonPayload = $"{{\"clientId\" : \"{clientid}\",\"clientSecret\":\"{secret}\"}}";
            request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);

            //Using Polly Retry policy
            var policy = Policy.Handle<WebException>()
                .Or<HttpRequestException>()
                .OrResult<IRestResponse>( r => r.StatusCode != HttpStatusCode.OK)
                .RetryAsync(3);

            var policyResult = await policy.ExecuteAndCaptureAsync(() => client.ExecuteTaskAsync(request));

            if (policyResult.Outcome == OutcomeType.Successful && policyResult.Result.IsSuccessful)
            {
                IRestResponse webResponse = policyResult.Result;
                Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(webResponse.Content);

                string rawToken = responseData["accessToken"];
                string expiresIn = responseData["expiresIn"];

                token = new AccessToken(rawToken, expiresIn);
            }
            else
            {
                throw new System.Exception("A communication error has occurred");
            }

            return token;
        }
    }
}
