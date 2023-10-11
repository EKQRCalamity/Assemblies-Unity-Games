using System.Collections;
using UnityEngine;

public class GameStepAdjustDynamicResolutionToFrameRate : GameStep
{
	public readonly float decreaseResolutionFPSThreshold;

	public readonly float increaseResolutionFPSThreshold;

	public readonly float sampleSizeInSeconds;

	public readonly int numberOfSamples;

	public readonly byte resolutionAdjustmentIncrement;

	private float _waitBeforeFirstSample;

	private float _elapsedTimeThisSample;

	private int _framesThisSample;

	private int _sampleCount;

	private float _totalElapsedTime;

	private int _totalFrames;

	public GameStepAdjustDynamicResolutionToFrameRate(float decreaseResolutionFPSThreshold = 50f, float increaseResolutionFPSThreshold = 55f, float sampleSizeInSeconds = 1f, int numberOfSamples = 10, byte resolutionAdjustmentIncrement = 5, float waitBeforeFirstSample = 0f)
	{
		this.decreaseResolutionFPSThreshold = 1f / decreaseResolutionFPSThreshold;
		this.increaseResolutionFPSThreshold = 1f / increaseResolutionFPSThreshold;
		this.sampleSizeInSeconds = sampleSizeInSeconds;
		this.numberOfSamples = numberOfSamples;
		this.resolutionAdjustmentIncrement = resolutionAdjustmentIncrement;
		_waitBeforeFirstSample = waitBeforeFirstSample;
	}

	protected override void Awake()
	{
		QualitySettings.vSyncCount = 0;
		PauseMenu.BlockInput(this);
	}

	protected override IEnumerator Update()
	{
		while (_waitBeforeFirstSample > 0f)
		{
			_waitBeforeFirstSample -= Time.unscaledDeltaTime;
			yield return null;
		}
		while (_sampleCount < numberOfSamples)
		{
			while (_elapsedTimeThisSample < sampleSizeInSeconds)
			{
				_elapsedTimeThisSample += Time.unscaledDeltaTime;
				_framesThisSample++;
				_totalElapsedTime += Time.unscaledDeltaTime;
				_totalFrames++;
				yield return null;
			}
			float num = _elapsedTimeThisSample / (float)_framesThisSample.InsureNonZero();
			if (num > decreaseResolutionFPSThreshold)
			{
				ProfileManager.options.video.display.resolutionScale -= resolutionAdjustmentIncrement;
			}
			else if (num < increaseResolutionFPSThreshold)
			{
				ProfileManager.options.video.display.resolutionScale += resolutionAdjustmentIncrement;
			}
			_elapsedTimeThisSample = 0f;
			_framesThisSample = 0;
			_sampleCount++;
		}
	}

	protected override void OnFinish()
	{
		int num = (int)(1f / (_totalElapsedTime / (float)_totalFrames.InsureNonZero()).InsureNonZero());
		Debug.Log($"Benchmark Average FPS: {num}");
		if (num <= 15)
		{
			ProfileManager.options.video.SetSafeModeOptions();
		}
		else if (num <= 30)
		{
			ProfileManager.options.video.SetPerformanceOptions();
		}
	}

	protected override void OnDestroy()
	{
		ProfileManager.Profile.SaveOptions(applyChanges: false);
		QualitySettings.vSyncCount = 1;
		PauseMenu.UnblockInput(this);
	}
}
