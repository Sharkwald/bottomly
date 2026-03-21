using System.Reflection;
using Bottomly;
using Bottomly.Configuration;
using Bottomly.LlmBot;
using Bottomly.Repositories;
using Bottomly.Seed;
using Bottomly.Slack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddUserSecrets(typeof(Program).Assembly).AddJsonFile("appsettings.json");

// MongoDB
builder.AddMongoDBClient("mongodb");
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("mongodb")!;
    var mongoUrl = MongoUrl.Create(connectionString);
    var settings = MongoClientSettings.FromUrl(mongoUrl);
    settings.ClusterConfigurator = cb =>
        cb.Subscribe(new MongoDB.Driver.Core.Extensions.DiagnosticSources.DiagnosticsActivityEventSubscriber(
            new MongoDB.Driver.Core.Extensions.DiagnosticSources.InstrumentationOptions { CaptureCommandText = true }));
    var instrumentedClient = new MongoClient(settings);
    var databaseName = mongoUrl.DatabaseName ?? "bottomly";
    return instrumentedClient.GetDatabase(databaseName);
});

builder.Services.AddBottomlyConfiguration(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddBottomlyRepositories();

builder.RegisterCommands(Assembly.GetExecutingAssembly());
builder.RegisterEventHandlers(Assembly.GetExecutingAssembly(), []);

builder.Services.AddBottomlySlack(builder.Configuration);
builder.AddBottomlyLlm();
builder.Services.AddBottomlySeeding();

var app = builder.Build();

await app.InitialiseAsync();

app.Run();