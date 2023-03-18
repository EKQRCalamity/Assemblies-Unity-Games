using Framework.Managers;
using UnityEngine;

public class CrisantaTrueExecutionEvents : MonoBehaviour
{
	public AnimationCurve slowTimeCurve;

	public void BWEffect()
	{
		Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeBW(0.4f);
	}

	public void Paradinha()
	{
		Core.Logic.ScreenFreeze.Freeze(0.1f, 1f, 0f, slowTimeCurve);
	}
}
