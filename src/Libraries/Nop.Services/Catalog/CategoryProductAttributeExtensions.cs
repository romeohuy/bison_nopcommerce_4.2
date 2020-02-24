using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CategoryProductAttributeExtensions
    {
     
        public static bool ShouldHaveValues(this CategoryProductAttributeMapping categoryProductAttributeMapping)
        {
            if (categoryProductAttributeMapping == null)
                return false;

            if (categoryProductAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
                return false;

            //other attribute controle types support values
            return true;
        }


        public static bool CanBeUsedAsCondition(this CategoryProductAttributeMapping categoryProductAttributeMapping)
        {
            if (categoryProductAttributeMapping == null)
                return false;

            if (categoryProductAttributeMapping.AttributeControlType == AttributeControlType.ReadonlyCheckboxes ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.Datepicker ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
                return false;

            //other attribute controle types support it
            return true;
        }

        public static bool ValidationRulesAllowed(this CategoryProductAttributeMapping categoryProductAttributeMapping)
        {
            if (categoryProductAttributeMapping == null)
                return false;

            if (categoryProductAttributeMapping.AttributeControlType == AttributeControlType.TextBox ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.MultilineTextbox ||
                categoryProductAttributeMapping.AttributeControlType == AttributeControlType.FileUpload)
                return true;

            //other attribute controle types does not have validation
            return false;
        }

       
        public static bool IsNonCombinable(this CategoryProductAttributeMapping categoryProductAttributeMapping)
        {
            if (categoryProductAttributeMapping == null)
                return false;
            
            var result = !ShouldHaveValues(categoryProductAttributeMapping);
            return result;
        }
    }
}
