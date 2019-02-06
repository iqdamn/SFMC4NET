using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public partial class SFMC4NET_Tests
    {
        private readonly string automationExternalKey = "";
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
    }
}
