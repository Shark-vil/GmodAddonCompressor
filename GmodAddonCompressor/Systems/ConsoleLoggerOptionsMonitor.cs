using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;

namespace GmodAddonCompressor.Systems
{
    internal class ConsoleLoggerOptionsMonitor : IOptionsMonitor<ConsoleLoggerOptions>
    {

		public ConsoleLoggerOptions CurrentValue => this.option;

		private readonly ConsoleLoggerOptions option = new ConsoleLoggerOptions();

		public ConsoleLoggerOptionsMonitor(LogLevel level)
		{
			option.LogToStandardErrorThreshold = level;
		}

		public ConsoleLoggerOptions Get(string name)
		{
			return this.option;
		}

		public IDisposable OnChange(Action<ConsoleLoggerOptions, string> listener)
		{
			return new ConsoleLoggerOptionsMonitorDisposable();
		}

		private sealed class ConsoleLoggerOptionsMonitorDisposable : IDisposable
		{
			public void Dispose() { }
		}
	}
}
