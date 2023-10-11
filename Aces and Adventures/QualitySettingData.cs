using UnityEngine;

public struct QualitySettingData
{
	public readonly int antiAliasing;

	public readonly ShadowQuality shadows;

	public readonly ShadowResolution shadowResolution;

	public readonly int pixelLightCount;

	public readonly float shadowDistance;

	public QualitySettingData(int qualityIndex)
	{
		int qualityLevel = QualitySettings.GetQualityLevel();
		QualitySettings.SetQualityLevel(qualityIndex);
		antiAliasing = QualitySettings.antiAliasing;
		shadows = QualitySettings.shadows;
		shadowResolution = QualitySettings.shadowResolution;
		pixelLightCount = QualitySettings.pixelLightCount;
		shadowDistance = QualitySettings.shadowDistance;
		QualitySettings.SetQualityLevel(qualityLevel);
	}

	public int GetAntiAliasing(byte? antiAliasOverride)
	{
		return ((int?)antiAliasOverride) ?? antiAliasing;
	}

	public ShadowQuality GetShadows(ShadowQuality? shadowsOverride)
	{
		return shadowsOverride ?? shadows;
	}

	public ShadowResolution GetShadowResolution(ShadowResolution? shadowResolutionOverride)
	{
		return shadowResolutionOverride ?? shadowResolution;
	}

	public int GetPixelLightCount(byte? pixelLightCountOverride)
	{
		return ((int?)pixelLightCountOverride) ?? pixelLightCount;
	}
}
