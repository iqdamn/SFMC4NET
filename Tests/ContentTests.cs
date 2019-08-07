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
        private string folderId = string.Empty;

        [Fact]
        public async Task GetAllFolders()
        {
            Exception exception = null;
            ContentCategories categories = null;

            try
            {
                categories = await dataExtensionManager.GetContentFolders();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
            Assert.True(categories != null);
        }

        [Fact]
        public async Task GetFoldersTree()
        {
            Exception exception = null;
            ContentCategories categories = null;
            Category category = null;

            try
            {
                categories = await dataExtensionManager.GetContentFolders();
                category = dataExtensionManager.ConvertContentFolderToCategoryTree(categories);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
            Assert.True(categories != null);
            Assert.True(category != null);
        }

        [Fact]
        public async Task GetFolderWithFilter()
        {
            Exception exception = null;
            ContentCategories categories = null;

            try
            {
                categories = await dataExtensionManager.GetContentFolders("0");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
            Assert.True(categories != null);
        }

        [Fact]
        public async Task CreateFolder()
        {
            Exception exception = null;
            bool result = false;

            try
            {
                folderId = await dataExtensionManager.CreateFolder("251268", "DeleteMe123");
                result = await dataExtensionManager.DeleteFolder(folderId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.Null(exception);
            Assert.True(result);
        }
    }
}
