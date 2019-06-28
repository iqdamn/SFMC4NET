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

        public BearerToken()
        { }

        public BearerToken(string authenticationURL)
        {
            RequestToken_URL = authenticationURL;
        }

        public async Task<AccessToken> GetAccessToken(string clientid, string secret)
        {
            AccessToken token = null;
            
            RestClient client = new RestClient(RequestToken_URL);

            //Setting up the request
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");

            string jsonPayload = string.Empty;
            bool usingOAuth20 = RequestToken_URL.Contains("v2");

            if (usingOAuth20)
            {
                jsonPayload = $"{{\"client_id\" : \"{clientid}\",\"client_secret\":\"{secret}\",\"grant_type\":\"client_credentials\"}}";
            }
            else
            {
                jsonPayload = $"{{\"clientId\" : \"{clientid}\",\"clientSecret\":\"{secret}\"}}";
            }
            
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

                string rawToken, expiresIn;

                if(usingOAuth20)
                {
                    rawToken = responseData["access_token"];
                    expiresIn = responseData["expires_in"];
                }
                else
                {
                    rawToken = responseData["accessToken"];
                    expiresIn = responseData["expiresIn"];
                }
                
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
