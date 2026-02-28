using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb");

var bottomly = builder.AddProject<Bottomly>("bottomly")
    .WaitFor(mongodb)
    .WithReference(mongodb);

var app = builder.Build();

app.Run();
