using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.News
{
    /// <summary>
    /// Represents a news item search model
    /// </summary>
    public partial class NewsCategorySearchModel : BaseSearchModel
    {
        #region Ctor

        //public NewsItemSearchModel()
        //{
        //    AvailableStores = new List<SelectListItem>();
        //}

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategory.List.SearchCategoryName")]
        public string SearchCategoryName { get; set; }

        #endregion
    }
}