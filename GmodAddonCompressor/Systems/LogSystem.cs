using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;

namespace GmodAddonCompressor.Systems
{
	internal class LogSystem
    {
		private static ILoggerFactory? _Factory = null;

		private static ILogger? _Logger = null;

		internal static ILoggerFactory LoggerFactory
		{
			get
			{
				if (_Factory == null)
				{
					_Factory = new LoggerFactory();
					_Factory.AddProvider(
						new ConsoleLoggerProvider(
							new ConsoleLoggerOptionsMonitor(LogLevel.Debug)
						)
					);
				}

				return _Factory;
			}
			set { _Factory = value; }
		}

		internal static ILogger Logger
        {
			get
			{
				if (_Logger == null)
					_Logger = LoggerFactory.CreateLogger("Program");
				return _Logger;
			}
			set { _Logger = value; }
		}

		internal static ILogger CreateLogger<T>()
        {
			return LoggerFactory.CreateLogger<T>();
        }
		
		/*
        private static string? GetMessage(object? value)
        {
            return Convert.ToString(value);
        }

        public static void Log(object? value)
        {
            Logger.LogInformation(GetMessage(value));
        }

        public static void Debug(object? value)
        {
            Logger.LogDebug(GetMessage(value));
        }

        public static void Warning(object? value)
        {
            Logger.LogWarning(GetMessage(value));
        }

        public static void Error(object? value)
        {
            Logger.LogError(GetMessage(value));
        }
		*/
    }
}
