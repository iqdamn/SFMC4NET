using SFMC4NET.Entities;
using SFMC4NET.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests.Entities;
using Xunit;

namespace Tests
{
    public partial class SFMC4NET_TenantSubdomain_Tests
    {
        private readonly DataExtensionManager dataExtensionManager;
        private readonly string tenantSubdomain;
        private readonly string clientId;
        private readonly string secret;
        private readonly string dataExtensionId;
        private readonly string automationExternalKey = string.Empty;
        private readonly string eventDefinitionKey = string.Empty;

        public SFMC4NET_TenantSubdomain_Tests()
        {
            DotNetEnv.Env.Load();
            tenantSubdomain = DotNetEnv.Env.GetString("TenantSubdomain");
            clientId = DotNetEnv.Env.GetString("ClientId");
            secret = DotNetEnv.Env.GetString("Secret");
            dataExtensionId = DotNetEnv.Env.GetString("TestDE");
            automationExternalKey = DotNetEnv.Env.GetString("AutomationId");
            eventDefinitionKey = DotNetEnv.Env.GetString("EventDefinitionKey");

            dataExtensionManager = DataExtensionManager.Build.
                                   SetTenantSubdomain(tenantSubdomain).
                                   UsingCredentials(clientId, secret);
        }

        [Fact]
        public async Task InsertOneRowWithDate()
        {
            var list = new List<SFMC4NET_TestDE>();
            var user = new SFMC4NET_TestDE { Id = "4", FirstName = "Fourth", LastName = "Lastname", TestTime = DateTime.Now };
            list.Add(user);
            Exception exception = null;

            try
            {
                await dataExtensionManager.SendRows(dataExtensionId, list);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
        }

        [Fact]
        public async Task InsertOneRowWithoutDate()
        {
            var list = new List<SFMC4NET_TestDE>();
            var user = new SFMC4NET_TestDE { Id = "5", FirstName = "Fifth", LastName = "Lastname" };
            list.Add(user);
            Exception exception = null;

            try
            {
                await dataExtensionManager.SendRows(dataExtensionId, list);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
        }

        [Fact]
        public async Task InsertOneRowWithNull()
        {
            var list = new List<SFMC4NET_TestDE>();
            var user = new SFMC4NET_TestDE { Id = "6", FirstName = "Sixth", TestTime = DateTime.Now };
            list.Add(user);
            Exception exception = null;

            try
            {
                await dataExtensionManager.SendRows(dataExtensionId, list);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
        }

        [Fact]
        public void RetrieveRows()
        {
            var task = Task.Run(async () => await dataExtensionManager.GetRows<SFMC4NET_TestDE>(dataExtensionId, string.Empty));

            var list = task.Result;

            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task StartAutomation()
        {
            Exception exception = null;
            string status = string.Empty;

            try
            {
                status = await dataExtensionManager.StartAutomation(automationExternalKey);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
            Assert.True(status == "OK");
        }

        [Fact]
        public async Task TriggerEvent()
        {
            Exception exception = null;
            string instanceId = string.Empty;
            Event customEvent = new Event();

            customEvent.ContactKey = "testuser@gmail.com";
            customEvent.EventDefinitionKey = eventDefinitionKey;
            customEvent.Data = new Dictionary<string, string>();
            customEvent.Data.Add("SubscriberKey", "testuser@gmail.com");
            customEvent.Data.Add("Name", "Test user");
            customEvent.Data.Add("Event", "API Event");

            try
            {
                instanceId = await dataExtensionManager.TriggerEvent(customEvent);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
            Assert.False(string.IsNullOrEmpty(instanceId));
        }
    }
}
