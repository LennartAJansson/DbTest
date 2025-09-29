namespace DbTest.Data;

internal class Order
{
  public int Id { get; set; } //Primary key property
  public int PersonId { get; set; } //Foreign key property
  public decimal Amount { get; set; }

  // Navigation property
  public Person? Person { get; set; }
}
