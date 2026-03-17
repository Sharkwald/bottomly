using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Bottomly>();

var mongo = builder.AddMongoDB("mongo")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb", "bottomly");

// var ollama = builder.AddOllama("ollama")
//     .WithEnvironment("OLLAMA_API_KEY", builder.Configuration["AppHost:OllamaApiKey"])
//     .WithDataVolume()
//     .WithLifetime(ContainerLifetime.Persistent);

var ollama = builder.AddOllamaLocal("ollama");

var bottomlyModel = ollama.AddModel("bottomlymodel", "qwen3.5:cloud");

var bottomly = builder.AddProject<Bottomly>("bottomly")
        .WaitFor(mongodb)
        .WaitFor(ollama)
        .WithReference(bottomlyModel)
        .WithReference(mongodb)
    ;

var app = builder.Build();

app.Run();