namespace DbTest.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class OrderConfiguration
  : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.ToTable("Orders");
    builder.Property(o => o.Amount).HasColumnType("decimal(18,2)");
    builder.HasOne(o => o.Person)
      .WithMany(p => p.Orders)
      .HasForeignKey(o => o.PersonId);
  }
}
