namespace DbTest.Data;

internal class Person
  : EntityBase
{
  public int Id { get; set; } //Primary key property
  public required string Name { get; set; }
  public required string Street { get; set; }
  public required string City { get; set; }

  // Navigation property
  public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}
