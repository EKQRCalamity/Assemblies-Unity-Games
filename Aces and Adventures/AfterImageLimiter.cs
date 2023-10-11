using System.Collections.Generic;
using UnityEngine;

public class AfterImageLimiter : MonoBehaviour
{
	[Range(10f, 200f)]
	public int maxActive = 100;

	private HashSet<AfterImageGenerator> _generators = new HashSet<AfterImageGenerator>();

	private void _UpdateMultipliers()
	{
		float num = 0f;
		foreach (AfterImageGenerator generator in _generators)
		{
			num += generator.activeImageMax;
		}
		float num2 = ((num < (float)maxActive) ? 1f : Mathf.Sqrt((float)maxActive / num));
		float timeBetweenImagesMultiplier = 1f / num2;
		float imageLifetimeMultiplier = num2;
		foreach (AfterImageGenerator generator2 in _generators)
		{
			generator2.SetMultipliers(timeBetweenImagesMultiplier, imageLifetimeMultiplier);
		}
	}

	private void Update()
	{
		if (_generators.Count == 0)
		{
			return;
		}
		foreach (AfterImageGenerator item in _generators.EnumerateSafe())
		{
			if (!item.isActiveAndEnabled)
			{
				StopTrackingGenerator(item);
			}
		}
	}

	private void OnDisable()
	{
		_generators.Clear();
	}

	public void TrackGenerator(AfterImageGenerator generator)
	{
		if (_generators.Add(generator))
		{
			_UpdateMultipliers();
		}
	}

	public void StopTrackingGenerator(AfterImageGenerator generator)
	{
		if (_generators.Remove(generator))
		{
			_UpdateMultipliers();
		}
	}
}
