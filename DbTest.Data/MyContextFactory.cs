namespace DbTest.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

internal class MyContextFactory
  : IDesignTimeDbContextFactory<MyContext>
{
  public MyContext CreateDbContext(string[] args)
  {
    IConfiguration configuration = new ConfigurationBuilder()
      .AddUserSecrets(typeof(MyContextFactory).Assembly)
      .Build();

    string? cStr = configuration.GetConnectionString("DefaultConnection");
    DbContextOptionsBuilder<MyContext> optionsBuilder = new();
    _ = optionsBuilder.UseSqlServer(cStr);

    return new MyContext(optionsBuilder.Options);
  }
}