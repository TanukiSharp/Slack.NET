using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SlackDotNet.TestApp
{
    public class CustomConsoleLogger : ILogger
    {
        private const string TimestampFormat = "HH:mm:ss.fff";
        private const string VerticalSeparator = " | ";

        private readonly string categoryName;
        private readonly string indentString;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public CustomConsoleLogger(string categoryName)
        {
            this.categoryName = categoryName;

            int prefixLength = TimestampFormat.Length + VerticalSeparator.Length + 4; // 4 = log level length
            if (categoryName != null)
                prefixLength += categoryName.Length + VerticalSeparator.Length;

            indentString = new string(' ', prefixLength) + VerticalSeparator;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string logLevelText;
            ConsoleColor color;

            switch (logLevel)
            {
                case LogLevel.Trace:
                    logLevelText = "TRCE";
                    color = ConsoleColor.DarkBlue;
                    break;
                case LogLevel.Debug:
                    logLevelText = "DBUG";
                    color = Console.ForegroundColor;
                    break;
                case LogLevel.Information:
                    logLevelText = "INFO";
                    color = ConsoleColor.DarkGreen;
                    break;
                case LogLevel.Warning:
                    logLevelText = "WARN";
                    color = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    logLevelText = "FAIL";
                    color = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    logLevelText = "CRIT";
                    color = ConsoleColor.Magenta;
                    break;
                default:
                    logLevelText = "----";
                    color = Console.ForegroundColor;
                    break;
            }

            lock (stringBuilder)
                UnsafeLog(color, logLevelText, formatter(state, exception));
        }

        private void UnsafeLog(ConsoleColor color, string logLevelText, string message)
        {
            stringBuilder.Append($"{DateTime.Now.ToString(TimestampFormat)}{VerticalSeparator}{logLevelText}{VerticalSeparator}");

            if (categoryName != null)
                stringBuilder.Append($"{categoryName}{VerticalSeparator}");

            int startIndex = 0;
            int endIndex = 0;

            bool isFirst = true;
            bool isRunning = true;

            while (isRunning)
            {
                endIndex = message.IndexOf('\n', startIndex);

                if (endIndex < 0)
                {
                    isRunning = false;
                    endIndex = message.Length - 1;
                }

                int localEndIndex = endIndex;

                while (message[localEndIndex] == '\r' || message[localEndIndex] == '\n')
                    localEndIndex--;

                localEndIndex = localEndIndex - startIndex + 1;

                if (localEndIndex > 0)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        stringBuilder.Append(indentString);
                    stringBuilder.Append(message, startIndex, localEndIndex);
                    stringBuilder.AppendLine();
                }

                startIndex = endIndex + 1;
            }

            Console.ForegroundColor = color;
            Console.Write(stringBuilder.ToString());
            Console.ResetColor();

            stringBuilder.Clear();
        }
    }

    public class CustomConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}
