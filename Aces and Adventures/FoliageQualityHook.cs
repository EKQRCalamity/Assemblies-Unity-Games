using UnityEngine;

public class FoliageQualityHook : MonoBehaviour
{
	public BoolEvent onFoliageEnabledChange;

	private void _OnFoliageQualityChange(ProfileOptions.VideoOptions.QualityOptions.FoliageQuality quality)
	{
		onFoliageEnabledChange?.Invoke(quality != ProfileOptions.VideoOptions.QualityOptions.FoliageQuality.Off);
	}

	private void Awake()
	{
		_OnFoliageQualityChange(ProfileManager.options.video.quality.foliageQuality);
	}

	private void OnEnable()
	{
		ProfileOptions.VideoOptions.QualityOptions.OnFoliageQualityChange += _OnFoliageQualityChange;
	}

	private void OnDisable()
	{
		ProfileOptions.VideoOptions.QualityOptions.OnFoliageQualityChange -= _OnFoliageQualityChange;
	}
}
