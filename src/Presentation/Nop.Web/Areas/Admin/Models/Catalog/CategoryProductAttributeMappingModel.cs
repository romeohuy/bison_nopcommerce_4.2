using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class CategoryProductAttributeMappingModel : ProductModel.ProductAttributeMappingModel
    {
        public int CategoryId { get; set; }
        public CategoryModel CategoryModel { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.CategoryProductAttributes.Attributes.Fields.IsUpdateProduct")]
        public bool IsUpdateProduct { get; set; }
    }

}