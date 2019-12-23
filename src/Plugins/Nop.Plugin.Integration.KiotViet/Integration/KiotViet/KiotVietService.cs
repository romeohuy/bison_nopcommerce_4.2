using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Integration.KiotViet.Integration.KiotViet.Entities;
using Nop.Services.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.Integration.KiotViet.Integration.KiotViet
{
    public class KiotVietService
    {
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        public KiotVietService(ICategoryService categoryService, IUrlRecordService urlRecordService)
        {
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
        }

        public void InsertKiotVietCatalog(List<KVCategory> kvCategories)
        {
            foreach (var cate in kvCategories)
            {
                var categories = _categoryService.GetAllCategories(showHidden:true);
                var cateRoot = categories.FirstOrDefault(t => t.Name.Equals(cate.categoryName, StringComparison.InvariantCultureIgnoreCase) && t.KiotVietCateId == cate.categoryId);
                if (cateRoot == null)
                {
                    cateRoot = new Category
                    {
                        Name = cate.categoryName,
                        CreatedOnUtc = cate.createdDate,
                        UpdatedOnUtc = cate.modifiedDate ?? DateTime.Now,
                        CategoryTemplateId = 1, //ViewPath: CategoryTemplate.ProductsInGridOrLines
                        KiotVietCateId = cate.categoryId,
                        Published = true,
                        IncludeInTopMenu = true,
                        PageSize = 12
                    };
                    _categoryService.InsertCategory(cateRoot);
                    var seName = _urlRecordService.ValidateSeName(cateRoot,string.Empty, cateRoot.Name, true);
                    _urlRecordService.SaveSlug(cateRoot, seName, 0);
                }
                if (cate.hasChild && cateRoot.Id > 0)
                {
                    foreach (var cate1 in cate.children)
                    {
                        categories = _categoryService.GetAllCategories(showHidden: true);
                        var cateChildFirst = categories.FirstOrDefault(t => t.Name.Equals(cate1.categoryName, StringComparison.InvariantCultureIgnoreCase) && t.KiotVietCateId == cate1.categoryId);
                        if (cateChildFirst == null)
                        {
                            cateChildFirst = new Category
                            {
                                Name = cate1.categoryName,
                                CreatedOnUtc = cate1.createdDate,
                                UpdatedOnUtc = cate1.modifiedDate ?? DateTime.Now,
                                ParentCategoryId = cateRoot.Id,
                                CategoryTemplateId = 1, //ViewPath: CategoryTemplate.ProductsInGridOrLines
                                KiotVietCateId = cate1.categoryId,
                                Published = true,
                                IncludeInTopMenu = true,
                                PageSize = 12
                            };

                            _categoryService.InsertCategory(cateChildFirst);
                            var seName = _urlRecordService.ValidateSeName(cateChildFirst,string.Empty, cateChildFirst.Name, true);
                            _urlRecordService.SaveSlug(cateChildFirst, seName, 0);
                        }
                        
                        if (cate1.hasChild && cateChildFirst.Id > 0)
                        {
                            foreach (var cate2 in cate1.children)
                            {
                                categories = _categoryService.GetAllCategories(showHidden: true);
                                var cateChildSecond = categories.FirstOrDefault(t => t.Name.Equals(cate2.categoryName, StringComparison.InvariantCultureIgnoreCase) && t.KiotVietCateId == cate2.categoryId);
                                if (cateChildSecond == null)
                                {
                                    cateChildSecond = new Category
                                    {
                                        Name = cate2.categoryName,
                                        CreatedOnUtc = cate2.createdDate,
                                        UpdatedOnUtc = cate2.modifiedDate ?? DateTime.Now,
                                        ParentCategoryId = cateChildFirst.Id,
                                        CategoryTemplateId = 1, //ViewPath: CategoryTemplate.ProductsInGridOrLines
                                        KiotVietCateId = cate2.categoryId,
                                        Published = true,
                                        IncludeInTopMenu = true,
                                        PageSize = 12
                                    };
                                    _categoryService.InsertCategory(cateChildSecond);
                                    var seName = _urlRecordService.ValidateSeName(cateChildSecond,string.Empty, cateChildSecond.Name, true);
                                    _urlRecordService.SaveSlug(cateChildSecond, seName, 0);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

