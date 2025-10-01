# DbTest � EF Core �versikt och arbetsfl�den

Kort introduktion till EF Core, Code First, Repository/Unit of Work, Clean Code och SOLID i detta projekt. M�lgrupp: utvecklare utan f�rkunskaper i ORM/EF Core.

## Vad �r EF Core och ORM
- ORM mappar C#-klasser (entiteter) till tabeller i en relationsdatabas.
- EF Core �r Microsofts moderna ORM f�r .NET. Du skriver C#-kod, EF Core genererar SQL, sp�rar �ndringar och hanterar transaktioner.

## Projekt�versikt
- `DbTest.Data\MyContext` � din `DbContext` (porten till databasen). H�r finns `DbSet<Person>` och `DbSet<Order>` samt modellkonfiguration i `OnModelCreating`/konfigurationsklasser.
- `DbTest.Data\Person`, `DbTest.Data\Order` � entiteter (POCOs) som EF mappar till tabeller.
- `DbTest.Data\PersonConfiguration`, `DbTest.Data\OrderConfiguration` � Fluent API-konfiguration f�r tabeller, nycklar, relationer, kolumnstorlekar m.m.
- `DbTest.Data\MyContextFactory` � design-time factory s� att `dotnet ef` kan skapa `MyContext` vid design-time (migrationer). L�ser connection string via User Secrets.
- `DbTest.Data\Migrations\` � migreringsfiler (databasens versionshistorik).
- `DbTest.Data\MyInterceptor` � exempel p� EF Core-interceptor (loggning/m�tning/policyer).
- `DbTest.Data\DataExtensions` � DI-registreringar f�r `DbContext` m.m.
- `DbTest\Program.cs` � Console-host och DI bootstrap. Projektet �r ett .NET Worker Service.

M�lspec: .NET 9, C# 13.

## Centrala begrepp i EF Core
- `DbContext` � enhetens arbetsyta och transaktionsgr�ns. H�ller en Change Tracker och skriver till DB via `SaveChanges/SaveChangesAsync`.
- `DbSet<T>` � tabell/kollektionsing�ng. Querya med LINQ, �ndra via `Add/Update/Remove`.
- Entiteter � POCO-klasser som representerar din data.
- Relationer � definieras med navigationsegenskaper + Fluent API.
- Konfiguration � h�ll mapping i separata konfigurationsklasser f�r l�sbarhet och kontroll.
- Interceptors � hookar in i EF:s pipeline (loggning, regler, performance-m�tning).

## Code First och migrationer
Code First betyder att du modellerar i kod. Databasschemat versioneras via migrationer.

Typiskt fl�de:
1. �ndra entiteter/konfigurationer.
2. Skapa migration.
3. Uppdatera databasen.

### User Secrets (anslutningsstr�ng)
`MyContextFactory` laddar User Secrets fr�n `DbTest.Data`-assemblyn. Initiera/secreta d�r:

- Fr�n solution-roten:
  - Initiera: `dotnet user-secrets init --project .\DbTest.Data\DbTest.Data.csproj`
  - S�tt: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=DbTest3;Trusted_Connection=True;TrustServerCertificate=True" --project .\DbTest.Data\DbTest.Data.csproj`

Alternativt, k�r kommandona inifr�n katalogen `DbTest.Data` utan `--project`.

### dotnet-ef (CLI)
Installera globalt om du saknar det: `dotnet tool install -g dotnet-ef`

- Skapa migration (namn: `Initial`):
  - `dotnet ef migrations add Initial -c MyContext -p .\DbTest.Data\DbTest.Data.csproj -s .\DbTest\DbTest.csproj -o Migrations`
- Uppdatera databasen:
  - `dotnet ef database update -c MyContext -p .\DbTest.Data\DbTest.Data.csproj -s .\DbTest\DbTest.csproj`

Flaggor:
- `-c/--context` anger kontextklass (`MyContext`).
- `-p/--project` anger data-/migrationsprojekt (`DbTest.Data`).
- `-s/--startup-project` anger startup/host-projektet (`DbTest`).
- `-o/--output-dir` var migrationsfiler hamnar.

### Package Manager Console (Visual Studio)
- Skapa migration:
  - `Add-Migration Initial -Context MyContext -Project DbTest.Data -StartupProject DbTest -OutputDir Migrations`
- Uppdatera databasen:
  - `Update-Database -Context MyContext -Project DbTest.Data -StartupProject DbTest`

## Repository Pattern och Unit of Work
- Repository ger ett abstraktionslager (t.ex. `GetById`, `List`, `Add`, `Remove`).
- Unit of Work koordinerar �ndringar som en transaktion.
- I EF Core fungerar `DbContext` som Unit of Work och `DbSet<T>` som ett enkelt repository.
- Skapa egna repositories om du beh�ver ett begr�nsat API, kapsla aff�rsregler eller f�renkla test.

Minimal designid�:
- `IRepository<T>` med `GetByIdAsync`, `ListAsync`, `AddAsync`, `Remove`, och separata `IUnitOfWork` med `SaveChangesAsync`.
- Registrera i DI och h�ll `DbContext` som Scoped.

## Clean Code i detta sammanhang
- Separation of concerns � entiteter, konfigurationer, context och migrationer separerade.
- Beskrivande namngivning � klara klass- och egenskapsnamn.
- Asynkrona IO-v�gar � anv�nd `*Async` och `CancellationToken`.
- H�ll entiteterna som POCOs � undvik EF-specifik logik i dom�nmodellerna.
- Samla mapping i Fluent API-konfigurationer.

## SOLID-principer
- Single Responsibility � varje klass har ett tydligt ansvar (context, konfiguration, entitet, interceptor).
- Open/Closed � l�gg till nya entiteter/konfigurationer utan att �ndra befintliga.
- Liskov Substitution � implementationsbyten via gr�nssnitt ska vara transparenta.
- Interface Segregation � sm� fokuserade interfaces om du inf�r repository/UoW.
- Dependency Inversion � arbeta mot abstraktioner och anv�nd DI-kontainern.

## Vanliga arbetsfl�den
- Ny entitet:
  1. Skapa klass, t.ex. `Product` i `DbTest.Data`.
  2. L�gg till `DbSet<Product>` i `MyContext`.
  3. Skapa `ProductConfiguration` (nycklar, l�ngder, relationer).
  4. `dotnet ef migrations add AddProduct ...` och `dotnet ef database update`.
- L�sa data:
  - `await context.People.AsNoTracking().Where(p => p.City == "...").ToListAsync(ct)`.
- Spara data:
  - `context.People.Add(person); await context.SaveChangesAsync(ct);` � en implicit transaktion per `SaveChangesAsync`.
- Transaktioner �ver flera steg:
  - `using var tx = await context.Database.BeginTransactionAsync(ct); ... await tx.CommitAsync(ct);`

## Tips
- H�ll `DbContext` som Scoped i DI.
- Anv�nd `AsNoTracking()` f�r read-only queries.
- L�gg till loggning/m�tning via interceptors vid behov.
- I CI/produktion � hantera connection strings via milj�variabler/Secret store/Key Vault, inte i k�llkod.

---
Detta dokument ger en snabbstart f�r att arbeta vidare med EF Core, Code First och relaterade m�nster i denna l�sning. 