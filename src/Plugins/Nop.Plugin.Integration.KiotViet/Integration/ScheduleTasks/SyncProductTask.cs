using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Extensions;
using Nop.Plugin.Integration.KiotViet.Integration.KiotViet;
using Nop.Plugin.Integration.KiotViet.Integration.KiotViet.Entities;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Tasks;
using PAValue;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Integration.KiotViet.Integration.ScheduleTasks
{
    public class SyncProductTask : IScheduleTask
    {
        private readonly KiotVietApiConsumer _apiConsumer;
        private KiotVietService _kiotVietService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductAttributeService _productAttributeService;

        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;

        public SyncProductTask(ILogger logger, ICategoryService categoryService,
            IProductService productService, IUrlRecordService urlRecordService, IProductAttributeService productAttributeService, IWorkContext workContext, IShoppingCartService shoppingCartService, IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser)
        {
            _logger = logger;
            _categoryService = categoryService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _productAttributeService = productAttributeService;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _apiConsumer = new KiotVietApiConsumer();
            _kiotVietService = new KiotVietService(categoryService, urlRecordService);
        }

        public void Execute()
        {
            try
            {
                var productAttributes = _productAttributeService
                    .GetAllProductAttributesNoCache()
                    .Where(_ => !string.IsNullOrEmpty(_.KiotVietName))
                    .ToList();
                //_logger.InsertLog(LogLevel.Information, "Start to sync data KiotViet to Bison nopcommerce.");
                var kiotVietCategory = _categoryService.GetAllCategories("KiotViet", showHidden: true).FirstOrDefault();
                if (kiotVietCategory == null || kiotVietCategory.Id <= 0)
                    return;

                var sourceProductGroups = _apiConsumer.GetProducts().GroupBy(_ => _.sku);
                foreach (var group in sourceProductGroups)
                {
                    // RST429|BLACK-L
                    var sku = group.Key;
                    var sourceProducts = group.ToList();
                    var sourceProduct = sourceProducts.First();
                    var basePrice = sourceProducts.Min(_ => _.basePrice);

                    var existing = _productService.GetProductBySku(sku);
                    if (existing == null) // create new product
                    {
                        existing = KiotVietHelper.MapNewProduct(sourceProduct);
                        existing.Price = basePrice;
                        existing.StockQuantity = (int)sourceProducts.SelectMany(_ => _.inventories).Sum(_ => _.onHand);

                        _productService.InsertProduct(existing);
                        if (existing.Id <= 0) continue;
                        _categoryService.InsertProductCategory(new ProductCategory
                        {
                            ProductId = existing.Id,
                            CategoryId = kiotVietCategory.Id,
                            DisplayOrder = 0
                        });

                        var parentSearchEngine = _urlRecordService.ValidateSeName(existing, string.Empty, existing.Name, true);
                        _urlRecordService.SaveSlug(existing, parentSearchEngine, 0);

                        foreach (var kvProduct in sourceProducts)
                        {
                            foreach (var productAttribute in productAttributes)
                            {
                                MapProductAttributes(productAttribute.KiotVietName,
                                    kvProduct,
                                    productAttribute.Name,
                                    existing,
                                    productAttribute.AdjustPrice,
                                    basePrice);
                            }
                        }

                        if (sourceProduct.hasVariants)
                        {
                            //Combine Attributes
                            CombineProductAttributes(existing, sourceProducts);
                        }
                    }
                    else // update to existing product
                    {
                        KiotVietHelper.MergeProduct(sourceProduct, existing);
                        existing.StockQuantity = (int)sourceProducts.SelectMany(_ => _.inventories).Sum(_ => _.onHand);
                        existing.Price = basePrice;

                        var attMappings = _productAttributeService.GetProductAttributeMappingsByProductId(existing.Id);

                        List<ProductAttributeMapping> attributeMappings = new List<ProductAttributeMapping>();
                        foreach (var productAttribute in productAttributes)
                        {
                            var attributeMapping = attMappings.Where(_ => _.ProductAttribute.Name.Equals(productAttribute.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
                            attributeMappings.AddRange(attributeMapping);
                        }

                        foreach (var kvProduct in sourceProducts)
                        {
                            foreach (var productAttribute in productAttributes)
                            {
                                MapProductAttributes(productAttribute.KiotVietName,
                                    kvProduct,
                                    productAttribute.Name,
                                    existing,
                                    productAttribute.AdjustPrice,
                                    basePrice);
                            }
                        }

                        _productService.UpdateProduct(existing);

                        if (sourceProduct.hasVariants)
                        {
                            //Combine Attributes
                            CombineProductAttributes(existing, sourceProducts);
                        }
                    }

                }

                //Delete product not exist in kiotViet
                var kiotVietSkus = sourceProductGroups.Select(_ => _.Key).Distinct().ToList();
                _productService.DeleteProductsNotInKiotViet(kiotVietSkus);
            }
            catch (TimeoutException)
            {
                //ignore
            }
        }


        private void CombineProductAttributes(Product product, List<KVProduct> sourceProducts)
        {
            //Combine 
            var productAttributeMappings = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (productAttributeMappings.Count > 0)
            {
                var allAttributesXml = _productAttributeParser.GenerateAllCombinations(product, true);
                var productAttributeCombines = _productAttributeService.GetAllProductAttributeCombinations(product.Id);
                foreach (var item in productAttributeCombines)
                {
                    _productAttributeService.DeleteProductAttributeCombination(item);
                }

                foreach (var sourceProduct in sourceProducts)
                {
                    foreach (var attributesXml in allAttributesXml)
                    {
                        AttributesXml productAttributeValue = KiotVietHelper.XmlToObject(attributesXml, typeof(AttributesXml));
                        if (sourceProduct.attributes.Select(_ => _.ProductAttributeValueId.ToString()).All(productAttributeValue.ProductAttribute.Select(_ => _.ProductAttributeValue.Value).Contains))
                        {
                            //save combination
                            var combination = new ProductAttributeCombination
                            {
                                ProductId = product.Id,
                                AttributesXml = attributesXml,
                                StockQuantity = (int)sourceProduct.inventories.FirstOrDefault().onHand,
                                AllowOutOfStockOrders = false,
                                Sku = sourceProduct.code,
                                ManufacturerPartNumber = null,
                                Gtin = null,
                                OverriddenPrice = sourceProduct.basePrice,
                                NotifyAdminForQuantityBelow = 1
                            };
                            _productAttributeService.InsertProductAttributeCombination(combination);

                            break;
                        }
                    }
                }
            }
        }

        private void MapProductAttributes(string kvAttributeName,
            KVProduct kvProduct,
            string attributeName,
            Product product,
            bool adjustPrice = false,
            decimal originPrice = 0)
        {
            var kiotVietAttribute = kvProduct.attributes?.FirstOrDefault(_ => _.attributeName.Equals(kvAttributeName, StringComparison.InvariantCultureIgnoreCase));
            if (kiotVietAttribute == null) return;

            var existingAttribute = _productAttributeService.GetAllProductAttributes().FirstOrDefault(_ => _.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));
            if (existingAttribute == null) return;

            var existingAttributeValue = _productAttributeService
                .GetPredefinedProductAttributeValues(existingAttribute.Id)
                .ToList()
                .FirstOrDefault(a => a.Name.Equals(kiotVietAttribute.attributeValue, StringComparison.InvariantCultureIgnoreCase));

            var productAttMappings = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            var existingAttributeMapping = productAttMappings.FirstOrDefault(_ => _.ProductAttributeId == existingAttribute.Id);

            var firstInventory = kvProduct.inventories.FirstOrDefault();
            var quantity = 0;
            if (firstInventory != null) quantity = Convert.ToInt32(firstInventory.onHand);

            if (existingAttributeMapping != null)
            {
                if (!existingAttributeMapping.ProductAttributeValues.Any(a => a.Name.Equals(kiotVietAttribute.attributeValue, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var productAttributeValue = new ProductAttributeValue
                    {
                        ProductAttributeMappingId = existingAttributeMapping.Id,
                        Name = kiotVietAttribute.attributeValue,
                        PriceAdjustment = adjustPrice ? kvProduct.basePrice - originPrice : 0,
                        Quantity = quantity,
                        DisplayOrder = existingAttributeValue != null ? existingAttributeValue.DisplayOrder : 0
                    };
                    _productAttributeService.InsertProductAttributeValue(productAttributeValue);

                    if (productAttributeValue.Id > 0)
                    {
                        kiotVietAttribute.ProductAttributeValueId = productAttributeValue.Id;
                    }
                }
                else
                {
                    var productAttributeValue = existingAttributeMapping.ProductAttributeValues.FirstOrDefault(a => a.Name.Equals(kiotVietAttribute.attributeValue, StringComparison.InvariantCultureIgnoreCase));
                    if (productAttributeValue != null)
                    {
                        kiotVietAttribute.ProductAttributeValueId = productAttributeValue.Id;
                    }
                }
            }
            else
            {
                existingAttributeMapping = new ProductAttributeMapping
                {
                    ProductId = product.Id,
                    ProductAttributeId = existingAttribute.Id,
                    TextPrompt = attributeName,
                    IsRequired = true,
                    AttributeControlTypeId = AttributeControlType.DropdownList.ToInt()
                };
                _productAttributeService.InsertProductAttributeMapping(existingAttributeMapping);

                if (existingAttributeMapping.Id > 0)
                {
                    var productAttributeValue = new ProductAttributeValue
                    {
                        ProductAttributeMappingId = existingAttributeMapping.Id,
                        Name = kiotVietAttribute.attributeValue,
                        PriceAdjustment = adjustPrice ? kvProduct.basePrice - originPrice : 0,
                        Quantity = quantity,
                        DisplayOrder = existingAttributeValue?.DisplayOrder ?? 0
                    };
                    _productAttributeService.InsertProductAttributeValue(productAttributeValue);

                    if (productAttributeValue.Id > 0)
                    {
                        kiotVietAttribute.ProductAttributeValueId = productAttributeValue.Id;
                    }
                }
            }
        }
    }
}
