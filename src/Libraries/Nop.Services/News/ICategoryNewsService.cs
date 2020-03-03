using Nop.Core.Domain.News;
using System.Collections.Generic;

namespace Nop.Services.News
{
    public interface ICategoryNewsService
    {
        #region Category News

        void DeleteCategoryNews(CategoryNews cateNewsItem);

        CategoryNews GetCategoryNewsById(int newsId);

        IList<CategoryNews> GetAllCategoryNews(int languageId = 0, bool showHidden = false);

        void InsertCategoryNews(CategoryNews cateNews);

        void UpdateCategoryNews(CategoryNews cateNews);

        #endregion
    }
}
