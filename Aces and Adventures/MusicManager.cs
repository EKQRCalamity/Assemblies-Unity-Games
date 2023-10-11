using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	private static MusicManager _Instance;

	private static readonly ResourceBlueprint<GameObject> _MusicSourceBlueprint = "Audio/MusicSource";

	public float fadeInTime;

	public float fadeOutTime = 1f;

	private MusicSource _activeSource;

	private Dictionary<AudioClip, double> _playTimes = new Dictionary<AudioClip, double>();

	public static MusicManager Instance => ManagerUtil.GetSingletonInstance(ref _Instance, createSeparateGameObject: true);

	private double _GetPlayTime(AudioClip clip, MusicPlayType playType = MusicPlayType.Resume)
	{
		if (playType == MusicPlayType.Play)
		{
			return 0.0;
		}
		return _playTimes.GetValueOrDefault(clip);
	}

	private void Update()
	{
		foreach (MusicSource item in base.gameObject.GetComponentsInChildrenPooled<MusicSource>())
		{
			if (item.finished)
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	public MusicManager Play(MusicData musicData, MusicPlayType playType = MusicPlayType.Play, float volume = 1f, float? fadeInTimeOverride = null, float? fadeOutTimeOverride = null)
	{
		return Play(musicData.music.audioClip, musicData.introLoopBoundary, musicData.loopBoundary, playType, volume, fadeInTimeOverride, fadeOutTimeOverride);
	}

	public MusicManager Play(AudioClip clip, double introLoopBoundary, double loopBoundary, MusicPlayType playType = MusicPlayType.Play, float volume = 1f, float? fadeInTimeOverride = null, float? fadeOutTimeOverride = null, double? playFromOverride = null)
	{
		if (playType != MusicPlayType.Stop && (bool)_activeSource && _activeSource.clip == clip)
		{
			_activeSource.volume = volume;
			return this;
		}
		Stop(fadeOutTimeOverride);
		if (playType == MusicPlayType.Stop)
		{
			return this;
		}
		double num = playFromOverride ?? _GetPlayTime(clip, playType);
		_activeSource = Pools.Unpool(_MusicSourceBlueprint, base.transform).GetComponent<MusicSource>();
		_activeSource.volume = volume;
		_activeSource.fadeInTime = fadeInTimeOverride ?? ((num > 0.0) ? fadeOutTime : fadeInTime);
		_activeSource.introBoundary = introLoopBoundary;
		_activeSource.loopBoundary = loopBoundary;
		_activeSource.clip = clip;
		_activeSource.PlayFrom(num);
		return this;
	}

	public MusicManager Stop(float? fadeOutTimeOverride = null)
	{
		if (!_activeSource)
		{
			return this;
		}
		_playTimes[_activeSource.clip] = _activeSource.time;
		_activeSource.fadeOutTime = fadeOutTimeOverride ?? fadeOutTime;
		_activeSource.Stop();
		_activeSource = null;
		return this;
	}

	public MusicManager ClearPlayTimes()
	{
		_playTimes.Clear();
		return this;
	}
}
