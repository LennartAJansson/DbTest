namespace DbTest;

using Microsoft.EntityFrameworkCore;

internal class MyContext(DbContextOptions options)
  : DbContext(options)
{
  public DbSet<Person> People { get; set; }
  public DbSet<Order> Orders { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Person>().ToTable("People").Property("Name").HasMaxLength(50);
  }
}
