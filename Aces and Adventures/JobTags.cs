using System;
using System.Collections.Generic;

public static class JobTags<T>
{
	private static readonly Dictionary<Job, T> _Tags = new Dictionary<Job, T>();

	private static Action<Job> _ClearTagAction = ClearTag;

	public static T GetTag(Job job)
	{
		if (!_Tags.ContainsKey(job))
		{
			return default(T);
		}
		return _Tags[job];
	}

	public static void SetTag(Job job, T value, JobTagsClearType clearTagType = JobTagsClearType.OnStopRunning)
	{
		_Tags[job] = value;
		switch (clearTagType)
		{
		case JobTagsClearType.OnStopRunning:
			job.OnStopRunning(_ClearTagAction);
			break;
		case JobTagsClearType.OnCleanup:
			job.Cleanup(delegate
			{
				ClearTag(job);
			});
			break;
		default:
			throw new ArgumentOutOfRangeException("clearTagType", clearTagType, null);
		case JobTagsClearType.Manual:
			break;
		}
	}

	public static void ClearTag(Job job)
	{
		_Tags.Remove(job);
	}
}
