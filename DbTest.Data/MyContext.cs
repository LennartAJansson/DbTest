namespace DbTest.Data;

using Microsoft.EntityFrameworkCore;

internal class MyContext(DbContextOptions options)
  : DbContext(options)
{
  public DbSet<Person> People => Set<Person>();
  public DbSet<Order> Orders => Set<Order>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyContext).Assembly);
  }
}
