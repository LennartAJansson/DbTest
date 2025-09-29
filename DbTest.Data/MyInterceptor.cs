namespace DbTest.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

internal class MyInterceptor
  : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    var context = eventData.Context;
    if (context == null) return base.SavingChanges(eventData, result);
    var entries = context.ChangeTracker.Entries()
      .Where(e => e.State == EntityState.Added && e.Entity is EntityBase);
    foreach (var entry in entries)
    {
      entry.Property("Created").CurrentValue = DateTimeOffset.UtcNow;
    }
    return base.SavingChanges(eventData, result);
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
  {
    return SavingChanges(eventData, result) is InterceptionResult<int> r
      ? new ValueTask<InterceptionResult<int>>(r)
      : base.SavingChangesAsync(eventData, result, cancellationToken);
  }
}