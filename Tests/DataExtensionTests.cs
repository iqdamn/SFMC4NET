using SFMC4NET.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public partial class SFMC4NET_Tests
    {
        [Fact]
        public async Task CreateDataExtension()
        {
            Exception exception = null;
            DataExtension de = new DataExtension();
            de.Columns = new List<DataExtensionColumn>();

            de.Name = "Test1";
            de.CustomerKey = Guid.NewGuid().ToString();
            de.FolderId = 269216;

            de.Columns.Add(new DataExtensionColumn()
            {
                Name = "Col1",
                IsNullable = false,
                MaxLength = 10,
                InferedType = DataType.Text
            });
            de.Columns.Add(new DataExtensionColumn()
            {
                Name = "Col2",
                IsNullable = false,
                MaxLength = 20,
                InferedType = DataType.Number
            });

            try
            {
                await dataExtensionManager.CreateDataExtension(de);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);

        }
    }
}
