using System.Collections;

public static class VoiceSourceExtensions
{
	public static void InterruptSafe(this VoiceSource source)
	{
		if ((bool)source)
		{
			source.Interrupt();
		}
	}

	public static void StopAndClear(this VoiceSource source, ref VoiceSource stopAndClear)
	{
		stopAndClear.InterruptSafe();
		stopAndClear = null;
	}

	public static bool Finished(this VoiceSource source)
	{
		if ((bool)source)
		{
			return source.finished;
		}
		return true;
	}

	public static IEnumerator Wait(this VoiceSource source, float completeRatio)
	{
		if (!(completeRatio <= 0f) && (bool)source)
		{
			while (!source.Finished() && source.source.time < source.source.clip.length * completeRatio)
			{
				yield return null;
			}
		}
	}
}
