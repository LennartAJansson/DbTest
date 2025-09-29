namespace DbTest;

internal class Order
{
  public int Id { get; set; }
  public int PersonId { get; set; }
  public decimal Amount { get; set; }

  // Navigation property
  public Person? Person { get; set; }
}
