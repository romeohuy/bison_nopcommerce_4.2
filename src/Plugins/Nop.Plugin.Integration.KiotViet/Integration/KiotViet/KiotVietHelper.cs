using Nop.Core.Domain.Catalog;
using Nop.Core.Extensions;
using Nop.Plugin.Integration.KiotViet.Integration.KiotViet.Entities;
using PAValue;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Nop.Plugin.Integration.KiotViet.Integration.KiotViet
{
    public static class KiotVietHelper
    {
        public static string GetSku(string value)
        {
            if (value.Contains("|"))
            {
                return value.Split('|').First();
            }

            if (value.Contains("/"))
            {
                var arr = value.Split('/');
                if (arr.Length == 2)
                {
                    return arr.First();
                }
            }

            return value;
        }

        public static string GetName(string value)
        {
            return value.Contains("|") ? value.Split('|').First() : value;
        }

        public static void MergeProduct(KVProduct kvProduct, Product product)
        {
            if (kvProduct.fullName.IsNotNullOrEmpty())
            {
                product.Name = GetName(kvProduct.name);
            }

            if (kvProduct.code.IsNotNullOrEmpty())
            {
                product.Sku = GetSku(kvProduct.code);
            }
            product.Price = kvProduct.basePrice;
            product.UpdatedOnUtc = DateTime.UtcNow;
            product.KiotVietId = kvProduct.id.ToString();
            if (kvProduct.hasVariants)
            {
                product.ManageInventoryMethodId = ManageInventoryMethod.ManageStockByAttributes.ToInt();
                product.AllowAddingOnlyExistingAttributeCombinations = true;
            }
        }

        public static Product MapNewProduct(KVProduct kvProduct)
        {
            var inventory = kvProduct.inventories.FirstOrDefault();
            var stock = inventory != null ? Convert.ToInt32(inventory.onHand) : 0;

            return new Product
            {
                Name = GetName(kvProduct.fullName),
                Sku = GetSku(kvProduct.code),
                Price = kvProduct.basePrice,
                StockQuantity = stock,
                KiotVietId = kvProduct.id.ToString(),

                ProductTypeId = ProductType.SimpleProduct.ToInt(),
                ProductTemplateId = 1, //1: Simple product  | 2: Grouped product(with variants)
                ManageInventoryMethodId = kvProduct.hasVariants ? ManageInventoryMethod.ManageStockByAttributes.ToInt() : ManageInventoryMethod.ManageStock.ToInt(), //1 track inventory product
                LowStockActivityId = 1,
                NotifyAdminForQuantityBelow = 1,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                AllowAddingOnlyExistingAttributeCombinations = kvProduct.hasVariants,
                Published = true,
                VisibleIndividually = true,
                AllowCustomerReviews = true,
                DisplayStockAvailability = true,
                DisplayStockQuantity = false,

                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
        }

        public static AttributesXml XmlToObject(string xml, Type objectType)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            AttributesXml obj = null;
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(objectType);
                xmlReader = new XmlTextReader(strReader);
                obj = (AttributesXml)serializer.Deserialize(xmlReader);
            }
            catch (Exception)
            {
                //Handle Exception Code
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }
    }
}
