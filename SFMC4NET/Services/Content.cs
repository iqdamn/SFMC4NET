using Newtonsoft.Json;
using SFMC4NET.Entities;
using SFMC4NET.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SFMC4NET.Services
{
    public partial class DataExtensionManager
    {
        private const string FolderResource = "/categories";
        private const int PageSize = 200;

        public async Task<ContentCategories> GetContentFolders(string parentFolderId = null)
        {
            ContentCategories categories = null;
            string serviceURL = this.ContentURL + FolderResource + "?$pagesize=" + PageSize;

            if (accessToken == null || !accessToken.IsValid)
            {
                BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
            }

            ServiceHandler serviceHandler = new ServiceHandler();
            string result = await serviceHandler.InvokeRESTServiceNoBody(serviceURL, accessToken);

            categories = ParseContentFolderServiceOutput(result);

            //Checking if more calls are required because the paging
            if(categories != null && categories.Items != null && categories.Items.Count < categories.Count)
            {
                ContentCategories overallCategories = new ContentCategories();
                overallCategories.Items = new System.Collections.Generic.List<Category>();

                overallCategories.Items.AddRange(categories.Items);
                overallCategories.Count = categories.Count;

                //The last call should have zero items, this indicates no more pages are available
                while (categories.Items.Count != 0)
                {
                    int pageToRequest = (int)(categories.Page + 1);
                    serviceURL += "&$page=" + pageToRequest;

                    //Because we are iterating, it's better to check if the token is still valid, with OAuth 2.0 the token only lasts 1079 seconds
                    if (accessToken == null || !accessToken.IsValid)
                    {
                        BearerToken tokenBuilder = new BearerToken(AuthenticationURL);
                        accessToken = await tokenBuilder.GetAccessToken(this.clientId, this.secret);
                    }

                    result = await serviceHandler.InvokeRESTServiceNoBody(serviceURL, accessToken);
                    categories = ParseContentFolderServiceOutput(result);

                    if (categories == null)
                        break;

                    overallCategories.Items.AddRange(categories.Items);
                }

                categories = overallCategories;
            }

            return categories;
        }

        private ContentCategories ParseContentFolderServiceOutput(string serviceOutput)
        {
            ContentCategories categories = null;

            try
            {
                categories = JsonConvert.DeserializeObject<ContentCategories>(serviceOutput);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return categories;
        }

        /// <summary>
        /// Transform ContentCategories into a tree representation, useful for binding to TreeView
        /// </summary>
        /// <param name="categories">Regular list of categories</param>
        /// <returns>Tree representation</returns>
        public Category ConvertContentFolderToCategoryTree(ContentCategories categories)
        {
            var sortedList = categories.Items.OrderBy(x => x.ParentId).ToList();

            Category root = null;

            foreach(var item in sortedList)
            {
                if(root == null)
                {
                    root = new Category { Id = item.Id,
                        CategoryType = item.CategoryType,
                        Description = item.Description,
                        EnterpriseId = item.EnterpriseId,
                        MemberId = item.MemberId,
                        Name = item.Name,
                        ParentId = item.ParentId
                    };

                    root.Folders = new List<IFolder>();

                    continue;                    
                }

                if (item.ParentId == root.Id)
                {
                    root.Folders.Add(item);
                }
                else
                {
                    var category = root.Folders.Where(c => c.Id == item.ParentId).FirstOrDefault();

                    if (category.Folders == null)
                    {
                        category.Folders = new List<IFolder>();
                    }

                    category.Folders.Add(item);
                }
            }

            return root;
        }
    }
}
