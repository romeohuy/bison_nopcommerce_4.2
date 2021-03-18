using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.News
{
    public class NewsCategoryModel : BaseNopEntityModel
    {
        public NewsCategoryModel()
        {
            AvailableLanguages = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Language")]
        public int LanguageId { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Language")]
        public string LanguageName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Description")]
        public string Description { get; set; }
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Parent")]
        public int ParentCategoryId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.NewsCategory.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        public string Breadcrumb { get; set; }
    }
}
