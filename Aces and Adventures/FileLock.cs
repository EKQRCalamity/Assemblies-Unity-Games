using System.Collections.Concurrent;

public static class FileLock
{
	private static readonly ConcurrentDictionary<string, object> _Locks = new ConcurrentDictionary<string, object>();

	public static object Lock(string filepath)
	{
		return _Locks.GetValueOrDefault(filepath) ?? (_Locks[filepath] = new object());
	}

	public static void WaitForLocks()
	{
		foreach (object value in _Locks.Values)
		{
			lock (value)
			{
			}
		}
	}
}
