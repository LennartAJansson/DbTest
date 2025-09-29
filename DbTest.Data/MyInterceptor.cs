namespace DbTest.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using System.Collections.Generic;

internal class MyInterceptor
  : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    DbContext? context = eventData.Context;

    if (context == null)
    {
      return base.SavingChanges(eventData, result);
    }

    IEnumerable<EntityEntry> entries = context.ChangeTracker.Entries()
      .Where(e => e.State == EntityState.Added && e.Entity is EntityBase);
    foreach (EntityEntry entry in entries)
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