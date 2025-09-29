namespace DbTest.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class DataExtensions
{
  public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<MyInterceptor>();
    var cStr = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<MyContext>((sp, options) =>
    {
      options.UseSqlServer(cStr);
      options.AddInterceptors(sp.GetRequiredService<MyInterceptor>());
    });

    return services;
  }

  public static async Task<IHost> UseDb(this IHost host)
  {
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<MyContext>();

    if (db.Database.GetPendingMigrations().Any())
      db.Database.Migrate();

    var person = new Person { Name = "Nisse", Street = "Ågatan 1", City = "Gävle" };
    db.People.Add(person);
    var order = new Order { Person = person, Amount = 123.45M };
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    var orders = await db.Orders.Include("Person").AsNoTracking().ToListAsync();
    foreach (var o in orders)
      Console.WriteLine($"{o.Person.Id}: {o.Person.Name}, {o.Person.Street}, {o.Person.City}: {o.Amount} ");

    var people = await db.People.Include("Orders").AsNoTracking().ToListAsync();
    foreach (var p in people)
      Console.WriteLine($"{p.Id}: {p.Name}, {p.Street}, {p.City}: {p.Orders.Count} orders");

    return host;
  }
}
