using System;

namespace Cyph3D.Misc
{
	public static class Logger
	{
		public static DeltaStopwatch Time { get; } = DeltaStopwatch.StartNew();

		public static LogLevel LogLevel { get; } = LogLevel.Full;
		
		private static object _lock = new object();

		public static void Error(object message, string context = "Main")
		{
			if (LogLevel < LogLevel.Error) return;

			Print(message.ToString(), context, "ERROR", ConsoleColor.DarkRed);
		}
		
		public static void Warning(object message, string context = "Main")
		{
			if (LogLevel < LogLevel.Warning) return;
			
			Print(message.ToString(), context, "WARN", ConsoleColor.DarkYellow);
		}
		
		public static void Info(object message, string context = "Main")
		{
			if (LogLevel < LogLevel.Full) return;

			lock (_lock)
			{
				Print(message.ToString(), context, "INFO", ConsoleColor.Green);
			}
		}

		private static void Print(string message, string context, string prefix, ConsoleColor color)
		{
			ConsoleColor oldColor = Console.ForegroundColor;
			
			TimeSpan ts = Time.Elapsed;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write($"{(long)ts.TotalSeconds}.{ts:ffff} ");

			if (!string.IsNullOrEmpty(context))
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write($"[{context}] ");
			}

			Console.ForegroundColor = color;
			Console.Write($"{prefix}");
			
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write($" > {message}");
			
			Console.WriteLine();
			
			Console.ForegroundColor = oldColor;
		}
	}

	public enum LogLevel
	{
		None = 0,
		Error = 1,
		Warning = 2,
		Full = 3
	}
}