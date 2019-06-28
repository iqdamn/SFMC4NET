using Newtonsoft.Json.Linq;
using Polly;
using RestSharp;
using SFMC4NET.Entities;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        private const string AssetsResource = "/asset/v1/content/assets";
        
        public async Task<string> CreateAsset(Asset asset)
        {
            string serviceURL = this.BaseRESTURL + AssetsResource;
            string result = string.Empty;
            string url = string.Empty;

            if (accessToken == null || !accessToken.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            RestClient client = new RestClient(serviceURL);
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("Authorization", "Bearer " + accessToken.Token, ParameterType.HttpHeader);

            request.AddJsonBody(asset);

            var policy = Policy.Handle<WebException>()
                .Or<HttpRequestException>()
                .OrResult<IRestResponse>(r => r.StatusCode != HttpStatusCode.Created)
                .RetryAsync(3);

            var policyResult = await policy.ExecuteAndCaptureAsync(() => client.ExecuteTaskAsync(request));

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                IRestResponse webResponse = policyResult.Result;
                result = webResponse.Content;

                //extracting url from response
                JObject jObject = JObject.Parse(result);
                url = (string)jObject.SelectToken("$['fileProperties']['publishedURL']");
            }
            else
            {
                throw new System.Exception($"{policyResult.FinalHandledResult.Content}");
            }

            return url;
        }
    }
}
