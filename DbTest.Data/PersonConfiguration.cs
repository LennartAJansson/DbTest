namespace DbTest;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class PersonConfiguration
  : IEntityTypeConfiguration<Person>
{
  public void Configure(EntityTypeBuilder<Person> builder)
  {
    builder.ToTable("People");
    builder.Property(p => p.Name).HasMaxLength(50);
  }
}
