using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.News
{
    /// <summary>
    /// Represents a news item search model
    /// </summary>
    public partial class CategoryNewsSearchModel : BaseSearchModel
    {
        #region Ctor

        public NewsItemSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.ContentManagement.News.CategoryNews.List.SearchCategoryName")]
        public string SearchCategoryName { get; set; }

        #endregion
    }
}