namespace DbTest;

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
    var cStr = configuration.GetConnectionString("DefaultConnection");
    var optionsBuilder = new DbContextOptionsBuilder<MyContext>();
    optionsBuilder.UseSqlServer(cStr);
    return new MyContext(optionsBuilder.Options);
  }
}