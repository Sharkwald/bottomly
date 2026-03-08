using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Bottomly>();

var mongo = builder.AddMongoDB("mongo")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb");

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var qwen = ollama.AddModel("qwen3", "qwen3:4b");

var bottomly = builder.AddProject<Bottomly>("bottomly")
    .WaitFor(mongodb)
    .WaitFor(ollama)
    .WithReference(mongodb)
    .WithReference(qwen);

var app = builder.Build();

app.Run();