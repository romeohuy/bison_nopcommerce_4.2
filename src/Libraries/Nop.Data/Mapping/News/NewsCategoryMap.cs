using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class NewsCategoryMap : NopEntityTypeConfiguration<NewsCategory>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public override void Configure(EntityTypeBuilder<NewsCategory> builder)
        {
            builder.ToTable("NewsCategory");
            builder.HasKey(ni => ni.Id);
            builder.Property(ni => ni.Name).IsRequired();
            builder.Property(ni => ni.MetaKeywords).HasMaxLength(400);
            builder.Property(ni => ni.MetaTitle).HasMaxLength(400);

            builder.HasOne(ni => ni.Language)
                .WithMany()
                .HasForeignKey(ni => ni.LanguageId)
                .IsRequired();
        }
    }
}