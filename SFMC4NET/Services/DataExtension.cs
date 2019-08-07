using SFMC4NET.Entities;
using SFMC4NET.Infrastructure;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        public async Task CreateDataExtension(DataExtension dataExtension)
        {
            var requestMessage = await CreateDataExtensionRequestMessage(dataExtension);

            ServiceHandler client = new ServiceHandler();

            string response = await client.InvokeSOAPService(requestMessage, this.serviceURL,"Create");

            if(!response.Contains("<StatusCode>OK"))
            {
                throw new Exception("Something went wrong while creating the Data Extension");
            }
        }

        private async Task<string> CreateDataExtensionRequestMessage(DataExtension dataExtension)
        {
            if (accessToken == null || !accessToken.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            StringBuilder builder = new StringBuilder();

            builder.Append($"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Header><fueloauth xmlns=\"http://exacttarget.com\">{accessToken.Token}</fueloauth></s:Header>");
            builder.Append("<s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CreateRequest xmlns=\"http://exacttarget.com/wsdl/partnerAPI\">");
            builder.Append($"<Options></Options><Objects xmlns:ns1=\"http://exacttarget.com/wsdl/partnerAPI\" xsi:type=\"ns1:DataExtension\"><CustomerKey>{dataExtension.CustomerKey}</CustomerKey>");

            if (dataExtension.FolderId != 0)
            {
                builder.Append($"<CategoryID>{dataExtension.FolderId}</CategoryID>");
            }

            builder.Append($"<Name>{dataExtension.Name}</Name><IsSendable>false</IsSendable><Fields>");           

            foreach(DataExtensionColumn column in dataExtension.Columns)
            {
                builder.Append($"<Field><CustomerKey>{column.Name}</CustomerKey>");
                builder.Append($"<Name>{column.Name}</Name><FieldType>{column.InferedType}</FieldType><IsRequired>{(!column.IsNullable).ToString().ToLower()}</IsRequired>");

                if (column.InferedType == DataType.Text)
                    builder.Append($"<MaxLength>{column.MaxLength}</MaxLength>");

                builder.Append($"</Field>");
            }

            builder.Append("</Fields></Objects></CreateRequest></s:Body></s:Envelope>");

            return builder.ToString();
        }
    }
}
