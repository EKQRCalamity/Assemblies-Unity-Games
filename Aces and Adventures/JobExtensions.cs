using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class JobExtensions
{
	public static IEnumerator Or(this IEnumerator iEnumerator, IEnumerator otherIEnumerator)
	{
		while (iEnumerator.MoveNext() & otherIEnumerator.MoveNext())
		{
			yield return null;
		}
	}

	public static IEnumerator And(this IEnumerator iEnumerator, IEnumerator otherIEnumerator)
	{
		bool iEnumeratorHasNext = true;
		bool otherIEnumeratorHasNext = true;
		while (true)
		{
			int num;
			if (iEnumeratorHasNext)
			{
				bool flag;
				iEnumeratorHasNext = (flag = iEnumerator.MoveNext());
				num = (flag ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			int num2;
			if (otherIEnumeratorHasNext)
			{
				bool flag;
				otherIEnumeratorHasNext = (flag = otherIEnumerator.MoveNext());
				num2 = (flag ? 1 : 0);
			}
			else
			{
				num2 = 0;
			}
			if ((num | num2) != 0)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public static IEnumerator WaitForCompletion(this AsyncOperation asyncOperation, bool waitAdditionalFrameToInsureObjectsInitialized = true)
	{
		while (!asyncOperation.isDone)
		{
			yield return null;
		}
		if (waitAdditionalFrameToInsureObjectsInitialized)
		{
			yield return null;
		}
	}

	public static IEnumerator Then(this IEnumerator iEnumerator, Action thenDo)
	{
		while (iEnumerator.MoveNext())
		{
			yield return null;
		}
		thenDo();
	}

	public static IEnumerator AsEnumerator(this Task task)
	{
		while (!task.IsCompleted)
		{
			yield return null;
		}
		if (task.IsFaulted && task.Exception != null)
		{
			throw task.Exception;
		}
	}

	public static IEnumerator AsEnumerator<T>(this Task<T> task)
	{
		while (!task.IsCompleted)
		{
			yield return null;
		}
		if (task.IsFaulted && task.Exception != null)
		{
			throw task.Exception;
		}
		yield return task.Result;
	}

	public static IEnumerator AsEnumerator(this TaskAwaiter task)
	{
		while (!task.IsCompleted)
		{
			yield return null;
		}
	}

	public static IEnumerator AsEnumerator<T>(this TaskAwaiter<T> task)
	{
		while (!task.IsCompleted)
		{
			yield return null;
		}
		yield return task.GetResult();
	}
}
