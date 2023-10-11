using System.Collections.Generic;
using UnityEngine;

public class AmbientManager : MonoBehaviour
{
	private static AmbientManager _Instance;

	public float fadeInTime = 1f;

	public float fadeOutTime = 3f;

	private PooledAudioSource _activeSource;

	private Dictionary<AudioClip, float> _playTimes = new Dictionary<AudioClip, float>();

	public static AmbientManager Instance => ManagerUtil.GetSingletonInstance(ref _Instance);

	private float _GetPlayTime(AudioClip clip, MusicPlayType playType)
	{
		if (playType != MusicPlayType.Resume)
		{
			return 0f;
		}
		return _playTimes.GetValueOrDefault(clip);
	}

	public AmbientManager Play(AudioClip clip, MusicPlayType playType, float volume = 1f, float? fadeInOverride = null, float? fadeOutOverride = null)
	{
		if (playType == MusicPlayType.Resume && (bool)_activeSource && _activeSource.source.clip == clip)
		{
			_activeSource.volume = volume;
			return this;
		}
		Stop(fadeOutOverride);
		if (playType == MusicPlayType.Stop)
		{
			return this;
		}
		float num = _GetPlayTime(clip, playType);
		_activeSource = AudioPool.Instance.Play(clip, MasterMixManager.Ambient, volume, 1f, loop: true, fadeInOverride ?? ((!(num > 0f)) ? fadeInTime : (fadeOutOverride ?? fadeOutTime)), num);
		return this;
	}

	public AmbientManager Stop(float? fadeOutTimeOverride = null)
	{
		if (!_activeSource)
		{
			return this;
		}
		if ((bool)_activeSource?.source?.clip)
		{
			_playTimes[_activeSource.source.clip] = _activeSource.source.time;
		}
		_activeSource = _activeSource.StopAndClear(fadeOutTimeOverride ?? fadeOutTime);
		return this;
	}

	public void ClearPlayTimes()
	{
		_playTimes.Clear();
	}
}
