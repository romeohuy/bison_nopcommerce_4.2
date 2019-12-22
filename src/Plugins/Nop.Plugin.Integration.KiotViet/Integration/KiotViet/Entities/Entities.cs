using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Serialization;

namespace Nop.Plugin.Integration.KiotViet.Integration.KiotViet.Entities
{
    public class ApiResponse<T>
    {
        public HttpStatusCode Status { get; set; }
        public T Resource { get; set; }
        public ApiError Error { get; set; }
    }

    public class ApiError
    {
        public string error { get; set; }

        public ApiError(string error)
        {
            this.error = error;
        }
    }

    public class GetTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }


    public class KVCategory
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int retailerId { get; set; }
        public bool hasChild { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime? modifiedDate { get; set; }
        public List<KVCategory> children { get; set; }
    }

    public class GetCategoryResponse
    {
        public int total { get; set; }
        public int pageSize { get; set; }
        public List<KVCategory> data { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class Inventory
    {
        public int productId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public int branchId { get; set; }
        public string branchName { get; set; }
        public double cost { get; set; }
        public double onHand { get; set; }
        //public int reserved { get; set; }
    }

    public class Attribute
    {
        public int productId { get; set; }
        public string attributeName { get; set; }
        public string attributeValue { get; set; }

        [JsonIgnore]
        public int ProductAttributeValueId { get; set; }
    }

    public class KVProduct
    {
        public int id { get; set; }
        public int retailerId { get; set; }
        public string code { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string fullName { get; set; }
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public bool allowsSale { get; set; }
        public int type { get; set; }
        public bool hasVariants { get; set; }
        public decimal basePrice { get; set; }
        public string unit { get; set; }
        public int conversionValue { get; set; }
        public DateTime modifiedDate { get; set; }
        public bool isActive { get; set; }
        public List<Inventory> inventories { get; set; }
        public List<Attribute> attributes { get; set; }
        public List<string> images { get; set; }
        public string description { get; set; }
        public int? masterProductId { get; set; }
    }

    public class GetProductResponse
    {
        public List<int> removedIds { get; set; }
        public int total { get; set; }
        public int pageSize { get; set; }
        public List<KVProduct> data { get; set; }
        public DateTime timestamp { get; set; }
    }

}

namespace PAValue
{
    [XmlRoot(ElementName = "ProductAttributeValue")]
    public class ProductAttributeValueXml
    {
        [XmlElement(ElementName = "Value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "ProductAttribute")]
    public class ProductAttributeXml
    {
        [XmlElement(ElementName = "ProductAttributeValue")]
        public ProductAttributeValueXml ProductAttributeValue { get; set; }
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
    }

    [XmlRoot(ElementName = "Attributes")]
    public class AttributesXml
    {
        [XmlElement(ElementName = "ProductAttribute")]
        public List<ProductAttributeXml> ProductAttribute { get; set; }
    }

}
