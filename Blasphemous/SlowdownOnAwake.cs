using Framework.Managers;
using UnityEngine;

public class SlowdownOnAwake : MonoBehaviour
{
	public AnimationCurve slowTimeCurve;

	public float duration = 2f;

	private void Awake()
	{
		Core.Logic.ScreenFreeze.Freeze(0.1f, duration, 0f, slowTimeCurve);
	}
}
