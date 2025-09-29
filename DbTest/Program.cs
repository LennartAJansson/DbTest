using DbTest.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets(typeof(Program).Assembly);

builder.Services.AddData(builder.Configuration);

var app = builder.Build();

await app.UseDb();