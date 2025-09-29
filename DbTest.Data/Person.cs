namespace DbTest;

internal class Person: EntityBase
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string Street { get; set; }
  public string City { get; set; }

  public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}

internal class EntityBase
{
  public DateTimeOffset Created { get; set; }
}