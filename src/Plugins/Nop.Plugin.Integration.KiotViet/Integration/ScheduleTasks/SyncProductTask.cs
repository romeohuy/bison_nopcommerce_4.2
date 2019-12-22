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
            var productAttributes = _productAttributeService.GetAllProductAttributesNoCache().Where(_ => _.KiotVietName != null && _.KiotVietName != string.Empty).ToList();
            //_logger.InsertLog(LogLevel.Information, "Start to sync data KiotViet to Bison nopcommerce.");
            var kiotVietCategory = _categoryService.GetAllCategories("KiotViet", showHidden: true).FirstOrDefault();
            if (kiotVietCategory == null) return;

            var sourceProductGroups = _apiConsumer.GetProducts().GroupBy(_ => _.sku);
            foreach (var group in sourceProductGroups)
            {
                // RST429|BLACK-L
                var sku = group.Key;
                var sourceProducts = group.ToList();
                var sourceProduct = sourceProducts.First();
                var basePrice = sourceProducts.Min(_ => _.basePrice);

                var product = _productService.GetProductBySku(sku);
                if (product == null) // create new product
                {
                    product = KiotVietHelper.MapNewProduct(sourceProduct);
                    product.Price = basePrice;
                    product.StockQuantity = (int)sourceProducts.SelectMany(_ => _.inventories).Sum(_ => _.onHand);

                    _productService.InsertProduct(product);
                    if (product.Id <= 0) continue;

                    var parentSeachEngineName = product.ValidateSeName(string.Empty, product.Name, true);
                    _urlRecordService.SaveSlug(product, parentSeachEngineName, 0);
                    SaveCategoryMappings(product.Id, kiotVietCategory.Id);

                    foreach (var variant in sourceProducts)
                    {
                        //MapProductAttributes("Size", variant, "Size", product, true, basePrice);
                        //MapProductAttributes("Colour", variant, "Color", product);

                        foreach (var productAttribute in productAttributes)
                        {
                            MapProductAttributes(productAttribute.KiotVietName, variant, productAttribute.Name, product, productAttribute.AdjustPrice, basePrice);
                        }
                    }


                    if (sourceProduct.hasVariants)
                    {
                        //Combine Attributes
                        CombineProductAttributes(product, sourceProducts);
                    }
                }
                else // update to existing product
                {

                    KiotVietHelper.MergeProduct(sourceProduct, product);
                    product.StockQuantity = (int)sourceProducts.SelectMany(_ => _.inventories).Sum(_ => _.onHand);
                    product.Price = basePrice;

                    var attMappings = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                    //var sizeMappings = attMappings.Where(_ => _.ProductAttribute.Name.Equals("Size", StringComparison.InvariantCultureIgnoreCase)).ToList();
                    //var colorMappings = attMappings.Where(_ => _.ProductAttribute.Name.Equals("Color", StringComparison.InvariantCultureIgnoreCase)).ToList();
                    //var madeInMappings = attMappings.Where(_ => _.ProductAttribute.Name.Equals("Made in", StringComparison.InvariantCultureIgnoreCase)).ToList();

                    List<ProductAttributeMapping> attributeMappings = new List<ProductAttributeMapping>();
                    foreach (var productAttribute in productAttributes)
                    {
                        var attributeMapping = attMappings.Where(_ => _.ProductAttribute.Name.Equals(productAttribute.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        attributeMappings.AddRange(attributeMapping);
                    }

                    //foreach (var mapping in attributeMappings)
                    //{
                    //    _productAttributeService.DeleteProductAttributeMapping(mapping);
                    //}

                    foreach (var variant in sourceProducts)
                    {
                        //MapProductAttributes("Size", variant, "Size", product, true, basePrice);
                        // MapProductAttributes("Colour", variant, "Color", product);
                        //foreach (var productAttribute in productAttributes.Where(_ => attributeMappings.All(a => a.ProductAttribute.Name != _.Name)))
                        foreach (var productAttribute in productAttributes)
                        {
                            MapProductAttributes(productAttribute.KiotVietName, variant, productAttribute.Name, product, productAttribute.AdjustPrice, basePrice);
                        }
                    }

                    _productService.UpdateProduct(product);

                    if (sourceProduct.hasVariants)
                    {
                        //Combine Attributes
                        CombineProductAttributes(product, sourceProducts);
                    }
                }

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

        private void SaveCategoryMappings(int productId, int categoryId)
        {
            if (categoryId > 0)
            {
                _categoryService.InsertProductCategory(new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    DisplayOrder = 0
                });
            }
        }

        private void MapProductAttributes(string kvAttributeName, KVProduct sourceVariant, string attributeName, Product product, bool adjustPrice = false, decimal originPrice = 0)
        {
            var kiotVietAttribute = sourceVariant.attributes?.FirstOrDefault(_ => _.attributeName.Equals(kvAttributeName, StringComparison.InvariantCultureIgnoreCase));

            if (kiotVietAttribute != null)
            {
                var attributeSize = _productAttributeService.GetAllProductAttributes().FirstOrDefault(_ => _.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));
                if (attributeSize != null)
                {
                    var productAttributeValueSystem = _productAttributeService.GetPredefinedProductAttributeValues(attributeSize.Id).ToList().FirstOrDefault(a => a.Name.Equals(kiotVietAttribute.attributeValue, StringComparison.InvariantCultureIgnoreCase));
                    var productAttMappings = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);

                    var sizeMapping = productAttMappings.FirstOrDefault(_ => _.ProductAttributeId == attributeSize.Id);

                    if (sizeMapping != null)
                    {

                        if (!sizeMapping.ProductAttributeValues.Any(a => a.Name.Equals(kiotVietAttribute.attributeValue, StringComparison.InvariantCultureIgnoreCase)))
                        {


                            var productAttributeValue = new ProductAttributeValue
                            {
                                ProductAttributeMappingId = sizeMapping.Id,
                                Name = kiotVietAttribute.attributeValue,
                                PriceAdjustment = adjustPrice ? sourceVariant.basePrice - originPrice : 0,
                                Quantity = (int)sourceVariant.inventories.FirstOrDefault().onHand,
                                DisplayOrder = productAttributeValueSystem != null ? productAttributeValueSystem.DisplayOrder : 0
                            };
                            _productAttributeService.InsertProductAttributeValue(productAttributeValue);

                            if (productAttributeValue.Id > 0)
                            {
                                kiotVietAttribute.ProductAttributeValueId = productAttributeValue.Id;
                            }
                        }
                        else
                        {
                            var productAttributeValue = sizeMapping.ProductAttributeValues.FirstOrDefault(a => a.Name.Equals(kiotVietAttribute.attributeValue, StringComparison.InvariantCultureIgnoreCase));
                            if (productAttributeValue != null)
                            {
                                kiotVietAttribute.ProductAttributeValueId = productAttributeValue.Id;
                            }
                        }
                    }
                    else
                    {
                        sizeMapping = new ProductAttributeMapping
                        {
                            ProductId = product.Id,
                            ProductAttributeId = attributeSize.Id,
                            TextPrompt = attributeName,
                            IsRequired = true,
                            AttributeControlTypeId = AttributeControlType.DropdownList.ToInt()
                        };
                        _productAttributeService.InsertProductAttributeMapping(sizeMapping);

                        if (sizeMapping.Id > 0)
                        {
                            var productAttributeValue = new ProductAttributeValue
                            {
                                ProductAttributeMappingId = sizeMapping.Id,
                                Name = kiotVietAttribute.attributeValue,
                                PriceAdjustment = adjustPrice ? sourceVariant.basePrice - originPrice : 0,
                                Quantity = (int)sourceVariant.inventories.FirstOrDefault().onHand,
                                DisplayOrder = productAttributeValueSystem != null ? productAttributeValueSystem.DisplayOrder : 0
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

    }
}
