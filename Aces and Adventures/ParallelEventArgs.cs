using System;

public class ParallelEventArgs : EventArgs
{
	private readonly object state;

	private readonly Exception exception;

	public object State => state;

	public Exception Exception => exception;

	internal ParallelEventArgs(object state, Exception exception)
	{
		this.state = state;
		this.exception = exception;
	}
}
