using System.Reflection;
using Bottomly.Commands;
using Bottomly.Slack.MessageEventHandlers;
using Bottomly.Slack.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bottomly;

public static class HostBuilderExtensions
{
    extension(HostApplicationBuilder builder)
    {
        public void RegisterEventHandlers(Assembly assembly, Type[] exclude) =>
            assembly.GetTypes()
                .Where(t => typeof(IMessageEventHandler).IsAssignableFrom(t) &&
                            t is { IsInterface: false, IsAbstract: false })
                .Where(t => !exclude.Contains(t))
                .ToList()
                .ForEach(t =>
                {
                    builder.Services.AddSingleton(t);
                    builder.Services.AddSingleton<IMessageEventHandler>(sp =>
                        new TracingMessageEventHandlerDecorator(
                            (IMessageEventHandler)sp.GetRequiredService(t)));
                });

        public void RegisterCommands(Assembly assembly) =>
            assembly.GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .ToList()
                .ForEach(t => builder.Services.AddSingleton(t));
    }
}