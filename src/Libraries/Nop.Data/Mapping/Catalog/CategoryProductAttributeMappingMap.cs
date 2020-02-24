using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class CategoryProductAttributeMappingMap : NopEntityTypeConfiguration<CategoryProductAttributeMapping>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public  override void Configure(EntityTypeBuilder<CategoryProductAttributeMapping> builder)
        {
            builder.ToTable("Category_ProductAttribute_Mapping");
            builder.HasKey(pam => pam.Id);
            builder.Ignore(pam => pam.AttributeControlType);
            builder.HasOne(pam => pam.Category)
                .WithMany(p => p.CategoryAttributeMappings)
                .HasForeignKey(pam => pam.CategoryId);

            builder.HasOne(pam => pam.ProductAttribute)
                .WithMany()
                .HasForeignKey(pam => pam.ProductAttributeId);

            base.Configure(builder);
        }
    }
}