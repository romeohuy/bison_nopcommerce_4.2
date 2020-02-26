using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Catalog
{
    public interface ICategoryAttributeService
    {
        IList<CategoryProductAttributeMapping> GetByCatId(int categoryId);
        CategoryProductAttributeMapping Get(int id);
        void Delete(CategoryProductAttributeMapping mapping);
        void Insert(CategoryProductAttributeMapping mapping);
        List<int> Insert(CategoryProductAttributeMapping mapping, bool cascadeToChildren);
        void Update(CategoryProductAttributeMapping mapping);
    }

    public class CategoryAttributeService : ICategoryAttributeService
    {
        private const string CATEGORY_PRODUCTATTRIBUTE_ALL_KEY = "Nop.categoryproductattribute.all-{0}";
        private const string CATEGORY_PRODUCTATTRIBUTE_BY_ID_KEY = "Nop.categoryproductattribute.id-{0}";
        private const string CATEGORY_PRODUCTATTRIBUTE_PATTERN_KEY = "Nop.categoryproductattribute.";

        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<CategoryProductAttributeMapping> _repo;
        private readonly ICategoryService _categoryService;

        #region Category Mapping Product Attribute

        public CategoryAttributeService(ICacheManager cacheManager, IRepository<CategoryProductAttributeMapping> repo, IEventPublisher eventPublisher, ICategoryService categoryService)
        {
            _cacheManager = cacheManager;
            _repo = repo;
            _eventPublisher = eventPublisher;
            _categoryService = categoryService;
        }

        public IList<CategoryProductAttributeMapping> GetByCatId(int categoryId)
        {
            var key = string.Format(CATEGORY_PRODUCTATTRIBUTE_ALL_KEY, categoryId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pa in _repo.Table
                            where pa.CategoryId == categoryId
                            orderby pa.DisplayOrder
                            select pa;
                return query.ToList();
            });
        }

        public CategoryProductAttributeMapping Get(int id)
        {
            if (id == 0)
                return null;

            var key = string.Format(CATEGORY_PRODUCTATTRIBUTE_BY_ID_KEY, id);
            return _cacheManager.Get(key, () => _repo.GetById(id));
        }

        public void Delete(CategoryProductAttributeMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            _repo.Delete(mapping);
            //cache
            _cacheManager.RemoveByPrefix(CATEGORY_PRODUCTATTRIBUTE_PATTERN_KEY);
            //event notification
            _eventPublisher.EntityDeleted(mapping);
        }

        public virtual void Insert(CategoryProductAttributeMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _repo.Insert(mapping);

            //cache
            _cacheManager.RemoveByPrefix(CATEGORY_PRODUCTATTRIBUTE_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(mapping);
        }

        public virtual List<int> Insert(CategoryProductAttributeMapping mapping, bool cascadeToChildren)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            var listCategories = new List<int>();
            if (GetByCatId(mapping.CategoryId).All(x => x.ProductAttributeId != mapping.ProductAttributeId))
            {
                listCategories.Add(mapping.CategoryId);
                Insert(mapping);
                foreach (var category in _categoryService.GetAllCategoriesByParentCategoryId(mapping.CategoryId, true, true))
                {
                    if (GetByCatId(category.Id).All(x => x.ProductAttributeId != mapping.ProductAttributeId))
                    {
                        var categoryProductAttributeMapping = new CategoryProductAttributeMapping
                        {
                            CategoryId = category.Id,
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
                        listCategories.Add(category.Id);
                        Insert(categoryProductAttributeMapping);
                    }
                }
            }
            return listCategories;
        }

        public virtual void Update(CategoryProductAttributeMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _repo.Update(mapping);

            //cache
            _cacheManager.RemoveByPrefix(CATEGORY_PRODUCTATTRIBUTE_PATTERN_KEY);
            //event notification
            _eventPublisher.EntityUpdated(mapping);
        }

        #endregion
    }
}
