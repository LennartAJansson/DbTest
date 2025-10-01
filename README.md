# DbTest – EF Core översikt och arbetsflöden

Kort introduktion till EF Core, Code First, Repository/Unit of Work, Clean Code och SOLID i detta projekt. Målgrupp: utvecklare utan förkunskaper i ORM/EF Core.

## Vad är EF Core och ORM
- ORM mappar C#-klasser (entiteter) till tabeller i en relationsdatabas.
- EF Core är Microsofts moderna ORM för .NET. Du skriver C#-kod, EF Core genererar SQL, spårar ändringar och hanterar transaktioner.

## Projektöversikt
- `DbTest.Data\MyContext` – din `DbContext` (porten till databasen). Här finns `DbSet<Person>` och `DbSet<Order>` samt modellkonfiguration i `OnModelCreating`/konfigurationsklasser.
- `DbTest.Data\Person`, `DbTest.Data\Order` – entiteter (POCOs) som EF mappar till tabeller.
- `DbTest.Data\PersonConfiguration`, `DbTest.Data\OrderConfiguration` – Fluent API-konfiguration för tabeller, nycklar, relationer, kolumnstorlekar m.m.
- `DbTest.Data\MyContextFactory` – design-time factory så att `dotnet ef` kan skapa `MyContext` vid design-time (migrationer). Läser connection string via User Secrets.
- `DbTest.Data\Migrations\` – migreringsfiler (databasens versionshistorik).
- `DbTest.Data\MyInterceptor` – exempel på EF Core-interceptor (loggning/mätning/policyer).
- `DbTest.Data\DataExtensions` – DI-registreringar för `DbContext` m.m.
- `DbTest\Program.cs` – Console-host och DI bootstrap. Projektet är ett .NET Worker Service.

Målspec: .NET 9, C# 13.

## Centrala begrepp i EF Core
- `DbContext` – enhetens arbetsyta och transaktionsgräns. Håller en Change Tracker och skriver till DB via `SaveChanges/SaveChangesAsync`.
- `DbSet<T>` – tabell/kollektionsingång. Querya med LINQ, ändra via `Add/Update/Remove`.
- Entiteter – POCO-klasser som representerar din data.
- Relationer – definieras med navigationsegenskaper + Fluent API.
- Konfiguration – håll mapping i separata konfigurationsklasser för läsbarhet och kontroll.
- Interceptors – hookar in i EF:s pipeline (loggning, regler, performance-mätning).

## Code First och migrationer
Code First betyder att du modellerar i kod. Databasschemat versioneras via migrationer.

Typiskt flöde:
1. Ändra entiteter/konfigurationer.
2. Skapa migration.
3. Uppdatera databasen.

### User Secrets (anslutningssträng)
`MyContextFactory` laddar User Secrets från `DbTest.Data`-assemblyn. Initiera/secreta där:

- Från solution-roten:
  - Initiera: `dotnet user-secrets init --project .\DbTest.Data\DbTest.Data.csproj`
  - Sätt: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=DbTest3;Trusted_Connection=True;TrustServerCertificate=True" --project .\DbTest.Data\DbTest.Data.csproj`

Alternativt, kör kommandona inifrån katalogen `DbTest.Data` utan `--project`.

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
- Unit of Work koordinerar ändringar som en transaktion.
- I EF Core fungerar `DbContext` som Unit of Work och `DbSet<T>` som ett enkelt repository.
- Skapa egna repositories om du behöver ett begränsat API, kapsla affärsregler eller förenkla test.

Minimal designidé:
- `IRepository<T>` med `GetByIdAsync`, `ListAsync`, `AddAsync`, `Remove`, och separata `IUnitOfWork` med `SaveChangesAsync`.
- Registrera i DI och håll `DbContext` som Scoped.

## Clean Code i detta sammanhang
- Separation of concerns – entiteter, konfigurationer, context och migrationer separerade.
- Beskrivande namngivning – klara klass- och egenskapsnamn.
- Asynkrona IO-vägar – använd `*Async` och `CancellationToken`.
- Håll entiteterna som POCOs – undvik EF-specifik logik i domänmodellerna.
- Samla mapping i Fluent API-konfigurationer.

## SOLID-principer
- Single Responsibility – varje klass har ett tydligt ansvar (context, konfiguration, entitet, interceptor).
- Open/Closed – lägg till nya entiteter/konfigurationer utan att ändra befintliga.
- Liskov Substitution – implementationsbyten via gränssnitt ska vara transparenta.
- Interface Segregation – små fokuserade interfaces om du inför repository/UoW.
- Dependency Inversion – arbeta mot abstraktioner och använd DI-kontainern.

## Vanliga arbetsflöden
- Ny entitet:
  1. Skapa klass, t.ex. `Product` i `DbTest.Data`.
  2. Lägg till `DbSet<Product>` i `MyContext`.
  3. Skapa `ProductConfiguration` (nycklar, längder, relationer).
  4. `dotnet ef migrations add AddProduct ...` och `dotnet ef database update`.
- Läsa data:
  - `await context.People.AsNoTracking().Where(p => p.City == "...").ToListAsync(ct)`.
- Spara data:
  - `context.People.Add(person); await context.SaveChangesAsync(ct);` – en implicit transaktion per `SaveChangesAsync`.
- Transaktioner över flera steg:
  - `using var tx = await context.Database.BeginTransactionAsync(ct); ... await tx.CommitAsync(ct);`

## Tips
- Håll `DbContext` som Scoped i DI.
- Använd `AsNoTracking()` för read-only queries.
- Lägg till loggning/mätning via interceptors vid behov.
- I CI/produktion – hantera connection strings via miljövariabler/Secret store/Key Vault, inte i källkod.

---
Detta dokument ger en snabbstart för att arbeta vidare med EF Core, Code First och relaterade mönster i denna lösning. 