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
    public partial class NewsCategoryModelFactory : INewsCategoryModelFactory
    {
        #region Fields

        private readonly INewsCategoryService _newsCategoryService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public NewsCategoryModelFactory(
            INewsCategoryService newsCategoryService,
            IUrlRecordService urlRecordService)
        {
            _newsCategoryService = newsCategoryService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        public virtual NewsCategorySearchModel PrepareNewsCategorySearchModel(NewsCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public NewsCategoryListModel PrepareNewsCategoryListModel(NewsCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get categories
            var newsCategories = _newsCategoryService.GetAllNewsCategories(searchModel.SearchCategoryName);
            //prepare grid model
            var model = new NewsCategoryListModel().PrepareToGrid(searchModel, newsCategories, () =>
            {
                return newsCategories.Select(x =>
                {
                    var newsCategoryModel = x.ToModel<NewsCategoryModel>();
                    return newsCategoryModel;
                });
            });

            return model;
        }

        public NewsCategoryModel PrepareNewsCategoryModel(NewsCategoryModel model, NewsCategory newsCategory, bool excludeProperties = false)
        {
            if (newsCategory != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = newsCategory.ToModel<NewsCategoryModel>();
                    model.SeName = _urlRecordService.GetSeName(newsCategory, 0, true, false);
                }

            }

            //set default values for the new model
            if (newsCategory == null)
            {
                model.Published = true;
                model.IncludeInTopMenu = true;
            }

            return model;
        }

        #endregion
    }
}