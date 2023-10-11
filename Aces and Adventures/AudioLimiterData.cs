using System;
using UnityEngine;

public struct AudioLimiterData
{
	public static readonly Func<AudioLimiterData, bool> Alive = _Alive;

	public static readonly Func<AudioLimiterData, AudioLimiterData, bool> Alike = _Alike;

	private const float Lifetime = 0.05f;

	private const float DistanceThresholdSqrd = 1f;

	private readonly AudioClip _clip;

	private readonly Vector3 _position;

	private readonly float _timeOfCreation;

	private static bool _Alive(AudioLimiterData data)
	{
		return Time.time - data._timeOfCreation < 0.05f;
	}

	private static bool _Alike(AudioLimiterData a, AudioLimiterData b)
	{
		if (a._clip == b._clip)
		{
			return (a._position - b._position).sqrMagnitude < 1f;
		}
		return false;
	}

	public AudioLimiterData(AudioClip clip, Vector3 position, float timeOfCreation)
	{
		_clip = clip;
		_position = position;
		_timeOfCreation = timeOfCreation;
	}
}
