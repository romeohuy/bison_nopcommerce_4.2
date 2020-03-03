using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.News
{
    public class CategoryNewsModel : BaseNopEntityModel
    {
        public CategoryNewsModel()
        {
            AvailableLanguages = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Language")]
        public int LanguageId { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Language")]
        public string LanguageName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Description")]
        public string Description { get; set; }
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Parent")]
        public int ParentCategoryId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.CategoryNews.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        public string Breadcrumb { get; set; }
    }
}
