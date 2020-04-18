using System;

namespace Renderer
{
	public static class Logger
	{
		public static DeltaStopwatch Time { get; } = DeltaStopwatch.StartNew();

		public static LogLevel LogLevel { get; } = LogLevel.FULL;

		public static void Info(object message, string context = "")
		{
			if ((int)LogLevel >= 2)
				Print(message, string.IsNullOrEmpty(context) ? "INFO" : context + " INFO");
		}

		public static void Error(object message, string context = "")
		{
			if ((int)LogLevel >= 1)
				Print(message, string.IsNullOrEmpty(context) ? "ERROR" : context + " ERROR");
		}

		private static void Print(object message, string prefix)
		{
			TimeSpan ts = Time.Elapsed;
			Console.WriteLine($"{(long)ts.TotalSeconds}.{ts:ffff} {prefix}: {message}");
		}
	}

	public enum LogLevel
	{
		NONE = 0,
		ERROR = 1,
		FULL = 2
	}
}