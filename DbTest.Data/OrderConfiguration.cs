namespace DbTest;

using Microsoft.EntityFrameworkCore;

internal class OrderConfiguration
  : IEntityTypeConfiguration<Order>
{
  public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Order> builder)
  {
    builder.ToTable("Orders");
    builder.Property(o => o.Amount).HasColumnType("decimal(18,2)");
    builder.HasOne(o => o.Person)
      .WithMany(p => p.Orders)
      .HasForeignKey(o => o.PersonId);
  }
}
