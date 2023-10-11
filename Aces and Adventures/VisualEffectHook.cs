using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VisualEffectHook : MonoBehaviour
{
	private static readonly int COLOR_ID = Shader.PropertyToID("Color");

	private static readonly int SECONDARY_COLOR_ID = Shader.PropertyToID("SecondaryColor");

	private VisualEffect _vfx;

	public VisualEffect vfx => this.CacheComponent(ref _vfx);

	private string _name => (vfx?.visualEffectAsset?.name ?? base.gameObject?.name ?? "N/A") + " (VFX Asset)";

	public void SetColor(Color color)
	{
		if (vfx.HasVector4(COLOR_ID))
		{
			vfx.SetVector4(COLOR_ID, color);
		}
	}

	public void SetSecondaryColor(Color color)
	{
		if (vfx.HasVector4(SECONDARY_COLOR_ID))
		{
			vfx.SetVector4(SECONDARY_COLOR_ID, color);
		}
	}

	public void SetEmissionMultiplier(float multiplier)
	{
		if (vfx.HasFloat(AttacherVFX.EMISSION_MULTIPLIER_ID))
		{
			vfx.SetFloat(AttacherVFX.EMISSION_MULTIPLIER_ID, multiplier);
		}
	}

	public void SetProperty(string propertyName, float value)
	{
		if (vfx.HasFloat(propertyName))
		{
			vfx.SetFloat(propertyName, value);
		}
	}

	public void SetProperty(string propertyName, Color color)
	{
		if (vfx.HasVector4(propertyName))
		{
			vfx.SetVector4(propertyName, color);
		}
	}
}
