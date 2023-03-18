using System.Collections;
using UnityEngine;

namespace Framework.Managers;

public class ScreenFreezeManager : MonoBehaviour
{
	private Coroutine currentFreeze;

	public bool IsScreenFreeze { get; private set; }

	public void Freeze(float timeScale, float duration, float lapse = 0f, AnimationCurve curve = null)
	{
		if (currentFreeze != null)
		{
			StopCoroutine(currentFreeze);
			IsScreenFreeze = false;
			Core.Logic.CurrentLevelConfig.TimeScale = 1f;
		}
		currentFreeze = StartCoroutine(FreezeCoroutine(timeScale, duration, lapse, curve));
	}

	private IEnumerator FreezeCoroutine(float timeScale, float duration, float lapse = 0f, AnimationCurve easingCurve = null)
	{
		float realTimeCounter = 0f;
		float targetTimeScale = timeScale;
		float originTimeScale = Time.timeScale;
		if (IsScreenFreeze)
		{
			yield break;
		}
		IsScreenFreeze = true;
		while (Core.Logic.CurrentLevelConfig.TimeScale <= 0f)
		{
			yield return null;
		}
		yield return new WaitForSecondsRealtime(lapse);
		while (realTimeCounter <= duration)
		{
			if (easingCurve != null)
			{
				float t = easingCurve.Evaluate(realTimeCounter / duration);
				timeScale = Mathf.Lerp(originTimeScale, targetTimeScale, t);
			}
			float clampedTimeScale = Mathf.Clamp(timeScale, 0.1f, 1f);
			Core.Logic.CurrentLevelConfig.TimeScale = clampedTimeScale;
			realTimeCounter += Time.unscaledDeltaTime;
			yield return null;
		}
		Core.Logic.CurrentLevelConfig.TimeScale = 1f;
		IsScreenFreeze = false;
		Core.Logic.CurrentLevelConfig.IsSleeping = false;
	}
}
