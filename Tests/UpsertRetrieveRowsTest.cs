using SFMC4NET.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests.Entities;
using Xunit;

namespace Tests
{
    public partial class SFMC4NET_Tests
    {
        private readonly DataExtensionManager dataExtensionManager;
        private readonly string clientId = "";
        private readonly string secret = "";
        private readonly string dataExtensionId = "";
        private readonly string stack = "";
        private readonly string authenticationURL = "";
        private readonly string tenantSubdomain;

        public SFMC4NET_Tests()
        {
            DotNetEnv.Env.Load();
            clientId = DotNetEnv.Env.GetString("ClientId");
            secret = DotNetEnv.Env.GetString("Secret");
            stack = DotNetEnv.Env.GetString("Stack");
            dataExtensionId = DotNetEnv.Env.GetString("TestDE");
            authenticationURL = DotNetEnv.Env.GetString("AuthenticationURL");
            automationExternalKey = DotNetEnv.Env.GetString("AutomationId");
            tenantSubdomain = DotNetEnv.Env.GetString("TenantSubdomain");

            dataExtensionManager = DataExtensionManager.Build
                .SetStack(stack)
                .SetTenantSubdomain(tenantSubdomain)
                .UsingCredentials(clientId,secret);
        }

        [Fact]
        public async Task InsertOneRowWithDate()
        {
            var list = new List<SFMC4NET_TestDE>();
            var user = new SFMC4NET_TestDE { Id = "1", FirstName = "FirstUser", LastName = "Lastname", TestTime = DateTime.Now };
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
            var user = new SFMC4NET_TestDE { Id = "2", FirstName = "SecondUser", LastName = "Lastname" };
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
            var user = new SFMC4NET_TestDE { Id = "3", FirstName = "ThirdUser", TestTime = DateTime.Now };
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
    }
}
