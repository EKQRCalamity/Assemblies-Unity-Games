using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.Utilities;

namespace Framework.Managers;

public class CameraShakeManager
{
	public static Core.SimpleEvent OnCameraShake;

	public static Core.SimpleEvent OnCameraShakeOverthrow;

	public void Shake(bool ignoreTimeScale = false)
	{
		ProCamera2DShake proCamera2DShake = Core.Logic.CameraManager.ProCamera2DShake;
		proCamera2DShake.IgnoreTimeScale = ignoreTimeScale;
	}

	public void ShakeUsingPreset(string preset, bool shakeOverthrow = false)
	{
		ProCamera2DShake proCamera2DShake = Core.Logic.CameraManager.ProCamera2DShake;
		if (!(proCamera2DShake == null) && !preset.IsNullOrWhitespace())
		{
			proCamera2DShake.ShakeUsingPreset(preset);
			if (shakeOverthrow && OnCameraShakeOverthrow != null)
			{
				OnCameraShakeOverthrow();
			}
		}
	}
}
