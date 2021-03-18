using Nop.Web.Areas.Admin.Models.News;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Web.Models.News
{
    public partial class NewsItemListModel : BaseNopModel
    {
        public NewsItemListModel()
        {
            PagingFilteringContext = new NewsPagingFilteringModel();
            NewsItems = new List<NewsItemModel>();
            NewsCategoryModels = new List<NewsCategoryModel>();
        }

        public int WorkingLanguageId { get; set; }
        public int CurrentCategoryId { get; set; }
        public NewsPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<NewsItemModel> NewsItems { get; set; }

        public IList<NewsCategoryModel> NewsCategoryModels { get; set; }
    }
}