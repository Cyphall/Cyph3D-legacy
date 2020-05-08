using System;
using ObjLoader.Loader.Common;

namespace Renderer.Misc
{
	public static class Logger
	{
		public static DeltaStopwatch Time { get; } = DeltaStopwatch.StartNew();

		public static LogLevel LogLevel { get; } = LogLevel.Full;

		public static void Error(object message, string context = "")
		{
			if (LogLevel < LogLevel.Error) return;
			
			Console.Error.WriteLine(Format(message, context, "ERROR"));
		}
		
		public static void Warning(object message, string context = "")
		{
			if (LogLevel < LogLevel.Warning) return;
			
			Console.Out.WriteLine(Format(message, context, "WARN"));
		}
		
		public static void Info(object message, string context = "")
		{
			if (LogLevel < LogLevel.Full) return;
			
			Console.Out.WriteLine(Format(message, context, "INFO"));
		}

		private static string Format(object message, string context, string prefix)
		{
			TimeSpan ts = Time.Elapsed;
			return $"{(long)ts.TotalSeconds}.{ts:ffff} {(context.IsNullOrEmpty() ? "" : $"{context} ")}{prefix}: {message}";
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