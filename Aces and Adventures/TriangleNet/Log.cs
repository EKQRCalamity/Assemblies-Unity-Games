using System.Collections.Generic;
using TriangleNet.Logging;

namespace TriangleNet;

public sealed class Log : ILog<LogItem>
{
	private List<LogItem> log = new List<LogItem>();

	private LogLevel level;

	private static readonly Log instance;

	public static bool Verbose { get; set; }

	public static ILog<LogItem> Instance => instance;

	public IList<LogItem> Data => log;

	public LogLevel Level => level;

	static Log()
	{
		instance = new Log();
	}

	private Log()
	{
	}

	public void Add(LogItem item)
	{
		log.Add(item);
	}

	public void Clear()
	{
		log.Clear();
	}

	public void Info(string message)
	{
		log.Add(new LogItem(LogLevel.Info, message));
	}

	public void Warning(string message, string location)
	{
		log.Add(new LogItem(LogLevel.Warning, message, location));
	}

	public void Error(string message, string location)
	{
		log.Add(new LogItem(LogLevel.Error, message, location));
	}
}
