﻿using Polly;
using RestSharp;
using SFMC4NET.Entities;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFMC4NET.Infrastructure
{
    internal class ServiceHandler
    {
        public async Task<string> InvokeSOAPService(string request, string MainService_URL, string soapAction = "Retrieve")
        {
            string soapResult = string.Empty;
            RestClient client = new RestClient(MainService_URL);
            RestRequest serviceRequest = new RestRequest(Method.POST);
            serviceRequest.AddHeader("SOAPAction", soapAction);
            serviceRequest.RequestFormat = DataFormat.Xml;
            serviceRequest.AddParameter("text/xml", request, ParameterType.RequestBody);

            var policy = Policy.Handle<WebException>()
                .Or<HttpRequestException>()
                .OrResult<IRestResponse>(r => r.StatusCode != HttpStatusCode.OK)
                .RetryAsync(3);

            var policyResult = await policy.ExecuteAndCaptureAsync(() => client.ExecuteTaskAsync(serviceRequest));

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                IRestResponse webResponse = policyResult.Result;
                soapResult = webResponse.Content;
            }
            else
            {
                throw new System.Exception($"{policyResult.FinalHandledResult.Content}");
            }

            return soapResult;
        }   
        
        public async Task<string> InvokeRESTServiceNoBody(string serviceURL, AccessToken accessToken, Method method = Method.GET)
        {
            string result = string.Empty;

            RestClient client = new RestClient(serviceURL);
            RestRequest serviceRequest = new RestRequest(method);
            serviceRequest.AddHeader("Accept", "application/json");
            serviceRequest.AddParameter("Authorization", "Bearer " + accessToken.Token, ParameterType.HttpHeader);

            var policy = Policy.Handle<WebException>()
                .Or<HttpRequestException>()
                .OrResult<IRestResponse>(r => r.StatusCode != HttpStatusCode.OK)
                .RetryAsync(3);

            var policyResult = await policy.ExecuteAndCaptureAsync(() => client.ExecuteTaskAsync(serviceRequest));

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                IRestResponse webResponse = policyResult.Result;
                result = webResponse.Content;
            }
            else
            {
                throw new System.Exception($"{policyResult.FinalHandledResult.Content}");
            }

            return result;
        }
    }
}
