using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

builder.Build().Run();

