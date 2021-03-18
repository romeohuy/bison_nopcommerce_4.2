using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.News;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.News;
using Nop.Web.Framework.Mvc.Filters;
using System;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class NewsCategoryController : BaseAdminController
    {
        #region Fields

        private readonly INewsCategoryService _newsCategoryService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;
        private readonly INewsCategoryModelFactory _newsCategoryModelFactory;
        #endregion

        #region Ctor

        public NewsCategoryController(INewsCategoryService newsCategoryService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            ICustomerActivityService customerActivityService, IWorkContext workContext,
            INotificationService notificationService, INewsCategoryModelFactory newsCategoryModelFactory)
        {
            _newsCategoryService = newsCategoryService;
            _languageService = languageService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _urlRecordService = urlRecordService;
            _customerActivityService = customerActivityService;
            _workContext = workContext;
            _notificationService = notificationService;
            _newsCategoryModelFactory = newsCategoryModelFactory;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareLanguagesModel(NewsCategoryModel model)
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
            var model = _newsCategoryModelFactory.PrepareNewsCategorySearchModel(new NewsCategorySearchModel());
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(NewsCategorySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedDataTablesJson();

            var model = _newsCategoryModelFactory.PrepareNewsCategoryListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var model = new NewsCategoryModel();
            //languages
            PrepareLanguagesModel(model);

            //default values
            model.Published = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(NewsCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var newsCategory = model.ToEntity<NewsCategory>();
                newsCategory.UpdatedOnUtc = DateTime.UtcNow;
                newsCategory.CreatedOnUtc = DateTime.UtcNow;
                _newsCategoryService.Insert(newsCategory);

                //activity log
                _customerActivityService.InsertActivity("AddNewNewsCategory", _localizationService.GetResource("ActivityLog.AddNewNewsCategory"), newsCategory);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(newsCategory, model.SeName, model.Name, true);
                _urlRecordService.SaveSlug(newsCategory, seName, newsCategory.LanguageId);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.NewsCategory.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = newsCategory.Id });
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

            var newsCategory = _newsCategoryService.GetNewsCategoryById(id);
            if (newsCategory == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            var model = newsCategory.ToModel<NewsCategoryModel>();
            model.SeName = _urlRecordService.GetSeName(newsCategory, model.LanguageId);
            //languages
            PrepareLanguagesModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(NewsCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsCategory = _newsCategoryService.GetNewsCategoryById(model.Id);
            if (newsCategory == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsCategory = model.ToEntity(newsCategory);
                newsCategory.UpdatedOnUtc = DateTime.UtcNow;
                _newsCategoryService.Update(newsCategory);

                //activity log
                _customerActivityService.InsertActivity("EditNewsCategory", _localizationService.GetResource("ActivityLog.EditNewsCategory"), newsCategory);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(newsCategory, model.SeName, model.Name, true);
                _urlRecordService.SaveSlug(newsCategory, seName, newsCategory.LanguageId);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.NewsCategory.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = newsCategory.Id });
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

            var newsItem = _newsCategoryService.GetNewsCategoryById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            _newsCategoryService.DeleteNewsCategory(newsItem);

            //activity log
            _customerActivityService.InsertActivity("DeleteNewsCategory", _localizationService.GetResource("ActivityLog.DeleteNewsCategory"), newsItem);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.NewsCategory.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

    }
}