using System;

namespace TriangleNet.Logging;

public class LogItem : ILogItem
{
	private DateTime time;

	private LogLevel level;

	private string message;

	private string info;

	public DateTime Time => time;

	public LogLevel Level => level;

	public string Message => message;

	public string Info => info;

	public LogItem(LogLevel level, string message)
		: this(level, message, "")
	{
	}

	public LogItem(LogLevel level, string message, string info)
	{
		time = DateTime.Now;
		this.level = level;
		this.message = message;
		this.info = info;
	}
}
