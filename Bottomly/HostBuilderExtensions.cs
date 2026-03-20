using System.Reflection;
using Bottomly.Commands;
using Bottomly.Slack.MessageEventHandlers;
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
                .Where(t => t.Name != nameof(HelpHandler))
                .Where(t => !exclude.Contains(t))
                .ToList()
                .ForEach(t => builder.Services.AddSingleton(typeof(IMessageEventHandler), t));

        public void RegisterCommands(Assembly assembly) =>
            assembly.GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .ToList()
                .ForEach(t => builder.Services.AddSingleton(t));
    }
}