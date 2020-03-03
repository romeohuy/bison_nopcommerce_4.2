using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Models.News;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using Nop.Core.Domain.News;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class CategoryNewsController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryNewsService _categoryNewsService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public CategoryNewsController(ICategoryNewsService categoryNewsService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            ICustomerActivityService customerActivityService, IWorkContext workContext,
            INotificationService notificationService)
        {
            _categoryNewsService = categoryNewsService;
            _languageService = languageService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _urlRecordService = urlRecordService;
            _customerActivityService = customerActivityService;
            _workContext = workContext;
            _notificationService = notificationService;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareLanguagesModel(CategoryNewsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var languages = _languageService.GetAllLanguages(true);
            foreach (var language in languages)
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Text = language.Name,
                    Value = language.Id.ToString()
                });
        }

        #endregion 
        #region Category News items

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();
            return View();
        }

        [HttpPost]
        public virtual IActionResult List(CategoryNewsSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedDataTablesJson();
            
            var categoryNewsList = _categoryNewsService.GetAllCategoryNews(_workContext.WorkingLanguage.Id, true).Select(cn => cn.ToModel<CategoryNewsModel>()).ToList();
            var model = new CategoryNewsListModel().PrepareToGridNoPaging(searchModel,categoryNewsList);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var model = new CategoryNewsModel();
            //languages
            PrepareLanguagesModel(model);

            //default values
            model.Published = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(CategoryNewsModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var categoryNews = model.ToEntity<CategoryNews>();
                categoryNews.UpdatedOnUtc = DateTime.UtcNow;
                categoryNews.CreatedOnUtc = DateTime.UtcNow;
                _categoryNewsService.InsertCategoryNews(categoryNews);

                //activity log
                _customerActivityService.InsertActivity("AddNewCategoryNews", _localizationService.GetResource("ActivityLog.AddNewCategoryNews"), categoryNews);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(categoryNews,model.SeName, model.Name, true);
                _urlRecordService.SaveSlug(categoryNews, seName, categoryNews.LanguageId);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.CategoryNews.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = categoryNews.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareLanguagesModel(model);
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var categoryNews = _categoryNewsService.GetCategoryNewsById(id);
            if (categoryNews == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            var model = categoryNews.ToModel<CategoryNewsModel>();
            model.SeName = _urlRecordService.GetSeName(categoryNews,model.LanguageId);
            //languages
            PrepareLanguagesModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(CategoryNewsModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var categoryNews = _categoryNewsService.GetCategoryNewsById(model.Id);
            if (categoryNews == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                categoryNews = model.ToEntity(categoryNews);
                categoryNews.UpdatedOnUtc = DateTime.UtcNow;
                _categoryNewsService.UpdateCategoryNews(categoryNews);

                //activity log
                _customerActivityService.InsertActivity("EditCategoryNewsNews", _localizationService.GetResource("ActivityLog.EditCategoryNews"), categoryNews);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(categoryNews,model.SeName, model.Name, true);
                _urlRecordService.SaveSlug(categoryNews, seName, categoryNews.LanguageId);

               _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.CategoryNews.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = categoryNews.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareLanguagesModel(model);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _categoryNewsService.GetCategoryNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            _categoryNewsService.DeleteCategoryNews(newsItem);

            //activity log
            _customerActivityService.InsertActivity("DeleteCategoryNews", _localizationService.GetResource("ActivityLog.DeleteCategoryNews"), newsItem);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.CategoryNews.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

    }
}