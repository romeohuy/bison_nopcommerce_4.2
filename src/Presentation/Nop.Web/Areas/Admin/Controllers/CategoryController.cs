using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Cache;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CategoryController : BaseAdminController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILanguageService _languageService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICategoryAttributeService _categoryAttributeService;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor

        public CategoryController(IAclService aclService,
            ICategoryModelFactory categoryModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IExportManager exportManager,
            IImportManager importManager,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext, ISpecificationAttributeService specificationAttributeService, ILanguageService languageService, IProductAttributeService productAttributeService, ICategoryAttributeService categoryAttributeService, ICacheManager cacheManager)
        {
            _aclService = aclService;
            _categoryModelFactory = categoryModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _exportManager = exportManager;
            _importManager = importManager;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _specificationAttributeService = specificationAttributeService;
            _languageService = languageService;
            _productAttributeService = productAttributeService;
            _categoryAttributeService = categoryAttributeService;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Utilities
        protected virtual void UpdateLocales(ProductAttributeMapping pam, CategoryProductAttributeMappingModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pam,
                    x => x.TextPrompt,
                    localized.TextPrompt,
                    localized.LanguageId);
            }
        }
        protected virtual void UpdateLocales(CategoryProductAttributeMapping pam, CategoryProductAttributeMappingModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pam,
                    x => x.TextPrompt,
                    localized.TextPrompt,
                    localized.LanguageId);
            }
        }
        protected virtual void PrepareCategoryModel(CategoryModel model)
        {
            if (model != null)
            {
                //specification attributes
                model.AddSpecificationAttributeModel.AvailableAttributes = _cacheManager
                    .Get(NopModelCacheDefaults.SpecAttributesPrefixCacheKey, () =>
                    {
                        var availableSpecificationAttributes = new List<SelectListItem>();
                        foreach (var sa in _specificationAttributeService.GetSpecificationAttributes())
                        {
                            availableSpecificationAttributes.Add(new SelectListItem
                            {
                                Text = sa.Name,
                                Value = sa.Id.ToString()
                            });
                        }
                        return availableSpecificationAttributes;
                    });

                //options of preselected specification attribute
                if (model.AddSpecificationAttributeModel.AvailableAttributes.Any())
                {
                    var selectedAttributeId = int.Parse(model.AddSpecificationAttributeModel.AvailableAttributes.First().Value);
                    foreach (var sao in _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(selectedAttributeId))
                        model.AddSpecificationAttributeModel.AvailableOptions.Add(new SelectListItem
                        {
                            Text = sao.Name,
                            Value = sao.Id.ToString()
                        });
                }
                //default specs values
                model.AddSpecificationAttributeModel.ShowOnProductPage = true;
            }
        }
        protected virtual void UpdateLocales(Category category, CategoryModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(category, localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(category, seName, localized.LanguageId);
            }
        }

        protected virtual void UpdatePictureSeoNames(Category category)
        {
            var picture = _pictureService.GetPictureById(category.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(category.Name));
        }

        protected virtual void SaveCategoryAcl(Category category, CategoryModel model)
        {
            category.SubjectToAcl = model.SelectedCustomerRoleIds.Any();

            var existingAclRecords = _aclService.GetAclRecords(category);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(category, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }

        protected virtual void SaveStoreMappings(Category category, CategoryModel model)
        {
            category.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = _storeMappingService.GetStoreMappings(category);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(category, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }

        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = _categoryModelFactory.PrepareCategorySearchModel(new CategorySearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(CategorySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _categoryModelFactory.PrepareCategoryListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = _categoryModelFactory.PrepareCategoryModel(new CategoryModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(CategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var category = model.ToEntity<Category>();
                category.CreatedOnUtc = DateTime.UtcNow;
                category.UpdatedOnUtc = DateTime.UtcNow;
                _categoryService.InsertCategory(category);

                //search engine name
                model.SeName = _urlRecordService.ValidateSeName(category, model.SeName, category.Name, true);
                _urlRecordService.SaveSlug(category, model.SeName, 0);

                //locales
                UpdateLocales(category, model);

                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        //category.AppliedDiscounts.Add(discount);
                        category.DiscountCategoryMappings.Add(new DiscountCategoryMapping { Discount = discount });
                }

                _categoryService.UpdateCategory(category);

                //update picture seo file name
                UpdatePictureSeoNames(category);

                //ACL (customer roles)
                SaveCategoryAcl(category, model);

                //stores
                SaveStoreMappings(category, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewCategory",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name), category);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");
                
                return RedirectToAction("Edit", new { id = category.Id });
            }

            //prepare model
            model = _categoryModelFactory.PrepareCategoryModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = _categoryService.GetCategoryById(id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = _categoryModelFactory.PrepareCategoryModel(null, category);
            PrepareCategoryModel(model);
            model.CategoryProductAttributesExist = _productAttributeService.GetAllProductAttributes().Any();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(CategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = _categoryService.GetCategoryById(model.Id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = category.PictureId;

                category = model.ToEntity(category);
                category.UpdatedOnUtc = DateTime.UtcNow;
                _categoryService.UpdateCategory(category);

                //search engine name
                model.SeName = _urlRecordService.ValidateSeName(category, model.SeName, category.Name, true);
                _urlRecordService.SaveSlug(category, model.SeName, 0);

                //locales
                UpdateLocales(category, model);

                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new discount
                        if (category.DiscountCategoryMappings.Count(mapping => mapping.DiscountId == discount.Id) == 0)
                            category.DiscountCategoryMappings.Add(new DiscountCategoryMapping { Discount = discount });
                    }
                    else
                    {
                        //remove discount
                        if (category.DiscountCategoryMappings.Count(mapping => mapping.DiscountId == discount.Id) > 0)
                            category.DiscountCategoryMappings
                                .Remove(category.DiscountCategoryMappings.FirstOrDefault(mapping => mapping.DiscountId == discount.Id));
                    }
                }

                _categoryService.UpdateCategory(category);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != category.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }

                //update picture seo file name
                UpdatePictureSeoNames(category);

                //ACL
                SaveCategoryAcl(category, model);

                //stores
                SaveStoreMappings(category, model);

                //activity log
                _customerActivityService.InsertActivity("EditCategory",
                    string.Format(_localizationService.GetResource("ActivityLog.EditCategory"), category.Name), category);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");
                
                return RedirectToAction("Edit", new { id = category.Id });
            }

            //prepare model
            model = _categoryModelFactory.PrepareCategoryModel(model, category, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = _categoryService.GetCategoryById(id);
            if (category == null)
                return RedirectToAction("List");

            _categoryService.DeleteCategory(category);

            //activity log
            _customerActivityService.InsertActivity("DeleteCategory",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name), category);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Export / Import

        public virtual IActionResult ExportXml()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {
                var xml = _exportManager.ExportCategoriesToXml();

                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "categories.xml");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public virtual IActionResult ExportXlsx()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {
                var bytes = _exportManager
                    .ExportCategoriesToXlsx(_categoryService.GetAllCategories(showHidden: true, loadCacheableCopy: false).ToList());

                return File(bytes, MimeTypes.TextXlsx, "categories.xlsx");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual IActionResult ImportFromXlsx(IFormFile importexcelfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //a vendor cannot import categories
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportCategoriesFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Imported"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Products

        [HttpPost]
        public virtual IActionResult ProductList(CategoryProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedDataTablesJson();

            //try to get a category with the specified id
            var category = _categoryService.GetCategoryById(searchModel.CategoryId)
                ?? throw new ArgumentException("No category found with the specified id");

            //prepare model
            var model = _categoryModelFactory.PrepareCategoryProductListModel(searchModel, category);

            return Json(model);
        }

        public virtual IActionResult ProductUpdate(CategoryProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a product category with the specified id
            var productCategory = _categoryService.GetProductCategoryById(model.Id)
                ?? throw new ArgumentException("No product category mapping found with the specified id");

            //fill entity from product
            productCategory = model.ToEntity(productCategory);
            _categoryService.UpdateProductCategory(productCategory);

            return new NullJsonResult();
        }

        public virtual IActionResult ProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a product category with the specified id
            var productCategory = _categoryService.GetProductCategoryById(id)
                ?? throw new ArgumentException("No product category mapping found with the specified id", nameof(id));

            _categoryService.DeleteProductCategory(productCategory);

            return new NullJsonResult();
        }

        public virtual IActionResult ProductAddPopup(int categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = _categoryModelFactory.PrepareAddProductToCategorySearchModel(new AddProductToCategorySearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ProductAddPopupList(AddProductToCategorySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _categoryModelFactory.PrepareAddProductToCategoryListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult ProductAddPopup(AddProductToCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //get selected products
            var selectedProducts = _productService.GetProductsByIds(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingProductCategories = _categoryService.GetProductCategoriesByCategoryId(model.CategoryId, showHidden: true);
                foreach (var product in selectedProducts)
                {
                    //whether product category with such parameters already exists
                    if (_categoryService.FindProductCategory(existingProductCategories, product.Id, model.CategoryId) != null)
                        continue;

                    //insert the new product category mapping
                    _categoryService.InsertProductCategory(new ProductCategory
                    {
                        CategoryId = model.CategoryId,
                        ProductId = product.Id,
                        IsFeaturedProduct = false,
                        DisplayOrder = 1
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToCategorySearchModel());
        }

        #endregion

        #region Attribute Product
        [HttpPost]
        public virtual IActionResult OverWriteAllProductAttributes(string selectedIds)
        {
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                var productIds = new List<int>();
                foreach (var cateId in ids) productIds.AddRange(_productService.GetAllProductIdsByCategoryId(cateId));

                OverwriteProductsAttr(productIds.Distinct().ToList());
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public virtual IActionResult CategoryProductAttributeMappingList(CategoryProductAttributeMappingSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var category = _categoryService.GetCategoryById(searchModel.CategoryId);
            if (category == null)
                throw new ArgumentException("No category found with the specified id");
            var attributes = _categoryAttributeService.GetByCatId(searchModel.CategoryId);
            var attributesModel = attributes
                .Select(x =>
                {
                    var attributeModel = new CategoryProductAttributeMappingModel();
                    PrepareCategoryProductAttributeMappingModel(attributeModel, x, x.Category, false);
                    return attributeModel;
                })
                .ToList();
            var model = new CategoryProductAttributeMappingListModel().PrepareToGridNoPaging(searchModel, attributesModel);

            return Json(model);
        }
        protected virtual void PrepareCategoryProductAttributeMappingModel(CategoryProductAttributeMappingModel model,
            CategoryProductAttributeMapping pam, Category category, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            model.CategoryId = category.Id;

            foreach (var productAttribute in _productAttributeService.GetAllProductAttributes())
            {
                model.AvailableProductAttributes.Add(new SelectListItem
                {
                    Text = productAttribute.Name,
                    Value = productAttribute.Id.ToString()
                });
            }

            if (pam == null)
                return;

            model.Id = pam.Id;
            model.ProductAttribute = _productAttributeService.GetProductAttributeById(pam.ProductAttributeId).Name;
            model.AttributeControlType = _localizationService.GetLocalizedEnum(pam.AttributeControlType);
            model.IsUpdateProduct = true;
            if (!excludeProperties)
            {
                model.ProductAttributeId = pam.ProductAttributeId;
                model.TextPrompt = pam.TextPrompt;
                model.IsRequired = pam.IsRequired;
                model.AttributeControlTypeId = pam.AttributeControlTypeId;
                model.DisplayOrder = pam.DisplayOrder;
                model.ValidationMinLength = pam.ValidationMinLength;
                model.ValidationMaxLength = pam.ValidationMaxLength;
                model.ValidationFileAllowedExtensions = pam.ValidationFileAllowedExtensions;
                model.ValidationFileMaximumSize = pam.ValidationFileMaximumSize;
                model.DefaultValue = pam.DefaultValue;
            }

            if (pam.ValidationRulesAllowed())
            {
                var validationRules = new StringBuilder(string.Empty);
                if (pam.ValidationMinLength != null)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        _localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.ValidationRules.MinLength"),
                        pam.ValidationMinLength);
                if (pam.ValidationMaxLength != null)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        _localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.ValidationRules.MaxLength"),
                        pam.ValidationMaxLength);
                if (!string.IsNullOrEmpty(pam.ValidationFileAllowedExtensions))
                    validationRules.AppendFormat("{0}: {1}<br />",
                        _localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.ValidationRules.FileAllowedExtensions"),
                        WebUtility.HtmlEncode(pam.ValidationFileAllowedExtensions));
                if (pam.ValidationFileMaximumSize != null)
                    validationRules.AppendFormat("{0}: {1}<br />",
                        _localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.ValidationRules.FileMaximumSize"),
                        pam.ValidationFileMaximumSize);
                if (!string.IsNullOrEmpty(pam.DefaultValue))
                    validationRules.AppendFormat("{0}: {1}<br />",
                        _localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.ValidationRules.DefaultValue"),
                        WebUtility.HtmlEncode(pam.DefaultValue));
                model.ValidationRulesString = validationRules.ToString();
            }

        }

        public virtual IActionResult CategoryProductAttributeMappingCreate(int categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                throw new ArgumentException("No product found with the specified id");


            var model = new CategoryProductAttributeMappingModel();
            PrepareCategoryProductAttributeMappingModel(model, null, category, false);
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult CategoryProductAttributeMappingCreate(CategoryProductAttributeMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var category = _categoryService.GetCategoryById(model.CategoryId);
            if (category == null)
                throw new ArgumentException("No product found with the specified id");

            //ensure this attribute is not mapped yet
            if (_categoryAttributeService.GetByCatId(category.Id).Any(x => x.ProductAttributeId == model.ProductAttributeId))
            {
                //redisplay form
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.AlreadyExists"));
                //model
                PrepareCategoryProductAttributeMappingModel(model, null, category, true);
                return View(model);
            }

            //insert mapping
            var categoryProductAttributeMapping = new CategoryProductAttributeMapping
            {
                CategoryId = model.CategoryId,
                ProductAttributeId = model.ProductAttributeId,
                TextPrompt = model.TextPrompt,
                IsRequired = model.IsRequired,
                AttributeControlTypeId = model.AttributeControlTypeId,
                DisplayOrder = model.DisplayOrder,
                ValidationMinLength = model.ValidationMinLength,
                ValidationMaxLength = model.ValidationMaxLength,
                ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions,
                ValidationFileMaximumSize = model.ValidationFileMaximumSize,
                DefaultValue = model.DefaultValue
            };
            var listCateIds = _categoryAttributeService.Insert(categoryProductAttributeMapping, true);
            UpdateLocales(categoryProductAttributeMapping, model);


            var productIds = _productService.GetAllProductIdsByCategoryId(model.CategoryId);
            OverwriteProductsAttr(productIds);
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.Added"));

            if (continueEditing)
            {
                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("CategoryProductAttributeMappingEdit", new { id = categoryProductAttributeMapping.Id });
            }

            SaveSelectedTabName("tab-product-attributes");
            return RedirectToAction("Edit", new { id = category.Id });
        }

        private void OverwriteProductsAttr(List<int> productIds)
        {
            foreach (var productId in productIds)
            {
                // do
                // Collect all product category attribute mappings
                var categories = _categoryService.GetProductCategoriesByProductId(productId, true);
                var categoryAttributeMappings = categories.SelectMany(category => _categoryAttributeService.GetByCatId(category.CategoryId))
                    .DistinctBy(_ => _.ProductAttributeId).ToList();

                var oldMappings = _productAttributeService.GetProductAttributeMappingsByProductId(productId);
                foreach (var attributeMapping in oldMappings)
                {
                    _productAttributeService.DeleteProductAttributeMapping(attributeMapping);
                }

                var newMappings = new List<ProductAttributeMapping>();
                //foreach (var mapping in categoryAttributeMappings.Where(_ => _.ProductAttributeId.IsNotIn(oldMappings.Select(o => o.ProductAttributeId))))
                foreach (var mapping in categoryAttributeMappings)
                {
                    var productAttributeMapping = new ProductAttributeMapping
                    {
                        ProductId = productId,
                        ProductAttributeId = mapping.ProductAttributeId,
                        TextPrompt = mapping.TextPrompt,
                        IsRequired = mapping.IsRequired,
                        AttributeControlTypeId = mapping.AttributeControlTypeId,
                        DisplayOrder = mapping.DisplayOrder,
                        ValidationMinLength = mapping.ValidationMinLength,
                        ValidationMaxLength = mapping.ValidationMaxLength,
                        ValidationFileAllowedExtensions = mapping.ValidationFileAllowedExtensions,
                        ValidationFileMaximumSize = mapping.ValidationFileMaximumSize,
                        DefaultValue = mapping.DefaultValue
                    };
                    newMappings.Add(productAttributeMapping);
                    //var oldMapping = oldMappings.FirstOrDefault(_ => _.ProductAttributeId == mapping.ProductAttributeId);

                    //newMappings.Add(oldMapping != null ? oldMapping : productAttributeMapping);
                }

                foreach (var productAttributeMapping in newMappings)
                {
                    _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);
                }
            }
        }

        [HttpPost]
        public virtual IActionResult OverWriteAllProductSpecAttributes(string selectedIds)
        {
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                var productIds = new List<int>();
                foreach (var cateId in ids)
                {
                    productIds.AddRange(_productService.GetAllProductIdsByCategoryId(cateId));
                }

                OverwriteProductSpecsAttr(productIds.Distinct().ToList());
            }

            return RedirectToAction("Index");
        }

        private void OverwriteProductSpecsAttr(List<int> productIds)
        {
            foreach (var productId in productIds)
            {
                // A B C
                var oldMappings = _specificationAttributeService.GetProductSpecificationAttributes(productId);

                // B C D
                // => giữ B, C thêm D, xóa A
                // Collect all product category attribute mappings
                var productCategories = _categoryService.GetProductCategoriesByProductId(productId, true);
                //var categoryAttributeMappings = categories.SelectMany(category => _specificationAttributeService.GetCategorySpecificationAttributes(category.CategoryId))
                //    .DistinctBy(_ => _.SpecificationAttributeOption.SpecificationAttributeId).ToList();
                var categoryAttributeMappings = new List<CategorySpecificationAttribute>();
                foreach (var productCategory in productCategories)
                {
                    var categorySpecificationAttributes = _specificationAttributeService.GetCategorySpecificationAttributes(productCategory.CategoryId).ToList();
                    foreach (var item in categorySpecificationAttributes)
                    {
                        if (item.SpecificationAttributeOption == null && item.SpecificationAttributeOptionId > 0)
                        {
                            item.SpecificationAttributeOption = _specificationAttributeService.GetSpecificationAttributeOptionById(item.SpecificationAttributeOptionId);
                        }
                    }
                    categoryAttributeMappings.AddRange(categorySpecificationAttributes);
                }
                //var newMappings = new List<ProductSpecificationAttribute>();
                foreach (var mapping in categoryAttributeMappings.DistinctBy(_ => _.SpecificationAttributeOption.SpecificationAttributeId).ToList())
                {
                    var oldSpec = oldMappings.FirstOrDefault(_ => _.SpecificationAttributeOption.SpecificationAttributeId == mapping.SpecificationAttributeOption.SpecificationAttributeId);
                    if (oldSpec != null)
                    {
                        //newMappings.Add(oldSpec);
                    }
                    else
                    {
                        var newSpec = new ProductSpecificationAttribute
                        {
                            AttributeTypeId = mapping.AttributeTypeId,
                            SpecificationAttributeOptionId = mapping.SpecificationAttributeOptionId,
                            ProductId = productId,
                            CustomValue = mapping.CustomValue,
                            AllowFiltering = mapping.AllowFiltering,
                            ShowOnProductPage = mapping.ShowOnProductPage,
                            DisplayOrder = mapping.DisplayOrder,
                        };
                        //newMappings.Add(newSpec);
                        _specificationAttributeService.InsertProductSpecificationAttribute(newSpec);
                    }
                }
                
            }
        }

        public virtual IActionResult CategoryProductAttributeMappingEdit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var categoryProductAttributeMapping = _categoryAttributeService.Get(id);
            if (categoryProductAttributeMapping == null)
                throw new ArgumentException("No category attribute mapping found with the attribute id");

            var category = _categoryService.GetCategoryById(categoryProductAttributeMapping.CategoryId);
            if (category == null)
                throw new ArgumentException("No category found with the attribute id");

            var model = new CategoryProductAttributeMappingModel();
            PrepareCategoryProductAttributeMappingModel(model, categoryProductAttributeMapping, category, false);

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.TextPrompt = _localizationService.GetLocalized(categoryProductAttributeMapping,x => x.TextPrompt, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult CategoryProductAttributeMappingEdit(CategoryProductAttributeMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var categoryProductAttributeMapping = _categoryAttributeService.Get(model.Id);
            if (categoryProductAttributeMapping == null)
                throw new ArgumentException("No category product attribute mapping found with the specified id");

            var category = _categoryService.GetCategoryById(model.CategoryId);
            if (category == null)
                throw new ArgumentException("No product found with the specified id");

            //ensure this attribute is not mapped yet
            if (_categoryAttributeService.GetByCatId(category.Id)
                .Any(x => x.ProductAttributeId == model.ProductAttributeId && x.Id != categoryProductAttributeMapping.Id))
            {
                //redisplay form
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.AlreadyExists"));
                //model
                PrepareCategoryProductAttributeMappingModel(model, categoryProductAttributeMapping, category, true);
                return View(model);
            }

            categoryProductAttributeMapping.ProductAttributeId = model.ProductAttributeId;
            categoryProductAttributeMapping.TextPrompt = model.TextPrompt;
            categoryProductAttributeMapping.IsRequired = model.IsRequired;
            categoryProductAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
            categoryProductAttributeMapping.DisplayOrder = model.DisplayOrder;
            categoryProductAttributeMapping.ValidationMinLength = model.ValidationMinLength;
            categoryProductAttributeMapping.ValidationMaxLength = model.ValidationMaxLength;
            categoryProductAttributeMapping.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
            categoryProductAttributeMapping.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
            categoryProductAttributeMapping.DefaultValue = model.DefaultValue;
            _categoryAttributeService.Update(categoryProductAttributeMapping);

            UpdateLocales(categoryProductAttributeMapping, model);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.Updated"));
            if (continueEditing)
            {
                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("CategoryProductAttributeMappingEdit", new { id = categoryProductAttributeMapping.Id });
            }

            SaveSelectedTabName("tab-product-attributes");
            return RedirectToAction("Edit", new { id = category.Id });
        }

        [HttpPost]
        public virtual IActionResult CategoryProductAttributeMappingDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var categoryProductAttributeMapping = _categoryAttributeService.Get(id);
            if (categoryProductAttributeMapping == null)
                throw new ArgumentException("No category attribute mapping found with the specified id");

            var categoryId = categoryProductAttributeMapping.CategoryId;
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                throw new ArgumentException("No category found with the specified id");
            _categoryAttributeService.Delete(categoryProductAttributeMapping);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.Deleted"));

            return new NullJsonResult();
        }
        
        [HttpPost]
        public virtual IActionResult CategorySpecificationAttributeAdd(int attributeTypeId, int specificationAttributeOptionId,
            string customValue, bool allowFiltering, bool showOnProductPage,
            int displayOrder, int categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            //we allow filtering only for "Option" attribute type
            if (attributeTypeId != (int)SpecificationAttributeType.Option)
            {
                allowFiltering = false;
            }
            //we don't allow CustomValue for "Option" attribute type
            if (attributeTypeId == (int)SpecificationAttributeType.Option)
            {
                customValue = null;
            }

            var psa = new CategorySpecificationAttribute
            {
                AttributeTypeId = attributeTypeId,
                SpecificationAttributeOptionId = specificationAttributeOptionId,
                SpecificationAttributeOption = _specificationAttributeService.GetSpecificationAttributeOptionById(specificationAttributeOptionId),
                CategoryId = categoryId,
                CustomValue = customValue,
                AllowFiltering = allowFiltering,
                ShowOnProductPage = showOnProductPage,
                DisplayOrder = displayOrder,
            };
            _specificationAttributeService.Insert(psa, true);
            var productIds = _productService.GetAllProductIdsByCategoryId(categoryId);
            OverwriteProductSpecsAttr(productIds);
            
            return new NullJsonResult();
        }


        [HttpPost]
        public virtual IActionResult CategorySpecAttrList(CategorySpecificationSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var productrSpecs = _specificationAttributeService.GetCategorySpecificationAttributes(searchModel.CategoryId);

            var productrSpecsModel = productrSpecs
                .Select(x =>
                {
                    var psaModel = new CategorySpecificationAttributeModel
                    {
                        Id = x.Id,
                        AttributeTypeId = x.AttributeTypeId,
                        AttributeTypeName = _localizationService.GetLocalizedEnum(x.AttributeType),
                        AttributeId = x.SpecificationAttributeOption.SpecificationAttribute.Id,
                        AttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                        AllowFiltering = x.AllowFiltering,
                        ShowOnProductPage = x.ShowOnProductPage,
                        DisplayOrder = x.DisplayOrder
                    };
                    switch (x.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            psaModel.ValueRaw = WebUtility.HtmlEncode(x.SpecificationAttributeOption.Name);
                            psaModel.SpecificationAttributeOptionId = x.SpecificationAttributeOptionId;
                            break;
                        case SpecificationAttributeType.CustomText:
                            psaModel.ValueRaw = WebUtility.HtmlEncode(x.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            //do not encode?
                            //psaModel.ValueRaw = x.CustomValue;
                            psaModel.ValueRaw = WebUtility.HtmlEncode(x.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            psaModel.ValueRaw = x.CustomValue;
                            break;
                        default:
                            break;
                    }
                    return psaModel;
                })
                .ToList();
            //prepare grid model
            var model = new CategorySpecificationAttributeListModel().PrepareToGridNoPaging(searchModel, productrSpecsModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult CategorySpecAttrUpdate(CategorySpecificationAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetCategorySpecificationAttributeById(model.Id);
            if (psa == null)
                return Content("No category specification attribute found with the specified id");

            //we allow filtering and change option only for "Option" attribute type
            if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
            {
                psa.AllowFiltering = model.AllowFiltering;
                psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
            }

            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;
            _specificationAttributeService.UpdateCategorySpecificationAttribute(psa);
            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult CategorySpecAttrDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetCategorySpecificationAttributeById(id);
            if (psa == null)
                throw new ArgumentException("No specification attribute found with the specified id");

            _specificationAttributeService.DeleteCategorySpecificationAttribute(psa);

            var productIds = _productService.GetAllProductIdsByCategoryId(psa.CategoryId);
            OverwriteProductSpecsAttr(productIds);
            return new NullJsonResult();
        }

        #endregion
    }
}