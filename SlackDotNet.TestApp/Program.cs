using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace SlackDotNet.TestApp
{
    class Program
    {
        private ILogger rootLogger;

        static void Main(string[] args)
        {
            EventLoop.Run(delegate { return new Program().MainAsync(); });
        }

        private void WriteInfo()
        {
            AssemblyName asmName = Assembly.GetEntryAssembly().GetName();
            rootLogger?.LogInformation($"{asmName.Name} {asmName.Version}");

            Version slackDotNetVersion = typeof(WebApi.WebApiClient).GetTypeInfo().Assembly.GetName().Version;

            rootLogger?.LogInformation($"Slack.NET library version {slackDotNetVersion} (commit {GitCommitInfo.Instance.ShortCommitHash})");
        }

        private string GetAccessToken()
        {
            var tokenStorage = new TokenStorage(new TokenStorageOptions
            {
                Entropy = new Guid("a45b37dc-4794-421f-9612-1d959cb9194d"),
            }, rootLogger);

            string accessToken;

            if (tokenStorage.IsTokenAvailable == false)
            {
                Console.Write("Enter an access token: ");
                accessToken = Console.ReadLine();
                if (accessToken == null)
                {
                    rootLogger?.LogError("Access token is required");
                    return null;
                }

                tokenStorage.StoreToken(accessToken);
            }
            else
            {
                accessToken = tokenStorage.LoadToken();
                if (accessToken == null)
                {
                    rootLogger?.LogError("Access token is unavailable");
                    return null;
                }
            }

            return accessToken;
        }

        private async Task MainAsync()
        {
            LoggerFactory loggerFactory = null;

            // comment bellow line to totally deactivate logging
            loggerFactory = new LoggerFactory();

            //loggerFactory?.AddConsole(LogLevel.Trace);
            //loggerFactory?.AddDebug(LogLevel.Trace);
            loggerFactory?.AddProvider(new CustomConsoleLoggerProvider());

            rootLogger = loggerFactory?.CreateLogger(nameof(TestApp));

            WriteInfo();

            string accessToken = GetAccessToken();
            if (accessToken == null)
                return;

            var slackClient = new SlackClient(accessToken, loggerFactory);

            if (await slackClient.Start() == false)
            {
                rootLogger?.LogError("Failed to start real time messaging");
                return;
            }

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                        break;
                }

                await Task.Delay(100);
            }

            rootLogger?.LogDebug("Exiting...");

            await slackClient.Stop();

            rootLogger?.LogInformation("Done");
        }
    }
}
