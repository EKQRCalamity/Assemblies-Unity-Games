public class ToBackgroundThread
{
	public int interval;

	private ToBackgroundThread(int interval = 0)
	{
		this.interval = interval;
	}

	public static ToBackgroundThread Create(int interval = 0)
	{
		return new ToBackgroundThread(interval);
	}
}
