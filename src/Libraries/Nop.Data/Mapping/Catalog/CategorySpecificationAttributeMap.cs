using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class CategorySpecificationAttributeMap : NopEntityTypeConfiguration<CategorySpecificationAttribute>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public  override void Configure(EntityTypeBuilder<CategorySpecificationAttribute> builder)
        {
            builder.ToTable("Category_SpecificationAttribute_Mapping");
            builder.HasKey(psa => psa.Id);

            builder.Property(psa => psa.CustomValue).HasMaxLength(4000);

            builder.Ignore(psa => psa.AttributeType);

            builder.HasOne(psa => psa.SpecificationAttributeOption)
                .WithMany()
                .HasForeignKey(psa => psa.SpecificationAttributeOptionId);


            builder.HasOne(psa => psa.Category)
                .WithMany(p => p.CategorySpecificationAttributes)
                .HasForeignKey(psa => psa.CategoryId);

            base.Configure(builder);
        }
    }
}