using System;
using System.Linq;
using System.Net.Http;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissPaulingBot.Common;
using Serilog;
using Serilog.Events;

using var host = new HostBuilder()
    .ConfigureAppConfiguration(x => x.AddEnvironmentVariables("PAULING_"))
    .ConfigureLogging(x =>
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateLogger();

        x.AddSerilog(logger, true);

        x.Services.Remove(x.Services.First(y => y.ServiceType == typeof(ILogger<>)));
        x.Services.AddSingleton(typeof(ILogger<>), typeof(DummyLogger<>));
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<HttpClient>();
        services.AddSingleton<Random>();
        services.AddDbContext<PaulingDbContext>(x => x.UseNpgsql(context.Configuration["DB_CONNECTION_STRING"]));
    })
    .ConfigureDiscordBot<MissPaulingBot.MissPaulingBot>((context, bot) =>
    {
        bot.Token = context.Configuration["TOKEN"];
        bot.Intents = GatewayIntents.All;

        bot.OwnerIds = new Snowflake[]
        {
            227578898521653249,
            167452465317281793
        };

        bot.Prefixes = new[]
        {
            "!"
        };
    })
    .Build();

try
{
    host.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadLine();
}