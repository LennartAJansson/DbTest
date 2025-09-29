using DbTest.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets(typeof(Program).Assembly);

builder.Services.AddData(builder.Configuration);

var app = builder.Build();

await app.UseDb();