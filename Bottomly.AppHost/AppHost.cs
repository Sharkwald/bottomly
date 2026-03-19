using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Bottomly>();

var mongo = builder.AddMongoDB("mongo")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb", "bottomly");

var bottomly = builder.AddProject<Bottomly>("bottomly")
        .WaitFor(mongodb)
        .WithReference(mongodb)
    ;

var app = builder.Build();

app.Run();