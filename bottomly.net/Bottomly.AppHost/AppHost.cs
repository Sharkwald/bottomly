using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Bottomly>();

var mongo = builder.AddMongoDB("mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb");

var bottomly = builder.AddProject<Bottomly>("bottomly")
    .WaitFor(mongodb)
    .WithReference(mongodb);

var app = builder.Build();

app.Run();