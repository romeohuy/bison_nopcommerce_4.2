using Nop.Core;
using Nop.Core.Domain.News;

namespace Nop.Services.News
{
    public interface INewsCategoryService
    {
        #region Category News

        void DeleteNewsCategory(NewsCategory newsCateItem);

        NewsCategory GetNewsCategoryById(int newsId);

        IPagedList<NewsCategory> GetAllNewsCategories(string searchCategoryName = null, int languageId = 0, bool showHidden = false);

        void Insert(NewsCategory newsCate);

        void Update(NewsCategory newsCate);

        #endregion
    }
}
