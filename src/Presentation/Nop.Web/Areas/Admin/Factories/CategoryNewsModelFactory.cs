using Nop.Core.Domain.News;
using Nop.Services.News;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.News;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Linq;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the category model factory implementation
    /// </summary>
    public partial class CategoryNewsModelFactory : ICategoryNewsModelFactory
    {
        #region Fields

        private readonly ICategoryNewsService _categoryNewsService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public CategoryNewsModelFactory(
            ICategoryNewsService categoryNewsService,
            IUrlRecordService urlRecordService)
        {
            _categoryNewsService = categoryNewsService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        public virtual CategoryNewsSearchModel PrepareCategoryNewsSearchModel(CategoryNewsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public CategoryNewsListModel PrepareCategoryNewsListModel(CategoryNewsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get categories
            var allCategoryNews = _categoryNewsService.GetAllCategoryNews(searchModel.SearchCategoryName);
            //prepare grid model
            var model = new CategoryNewsListModel().PrepareToGridNoPaging(searchModel, allCategoryNews.Select(x =>
            {
                var categoryNewsModel = x.ToModel<CategoryNewsModel>();
                return categoryNewsModel;
            }).ToList());

            return model;
        }

        public CategoryNewsModel PrepareCategoryNewsModel(CategoryNewsModel model, CategoryNews category, bool excludeProperties = false)
        {
            if (category != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = category.ToModel<CategoryNewsModel>();
                    model.SeName = _urlRecordService.GetSeName(category, 0, true, false);
                }

            }

            //set default values for the new model
            if (category == null)
            {
                model.Published = true;
                model.IncludeInTopMenu = true;
            }

            return model;
        }

        #endregion
    }
}