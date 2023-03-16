using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManagerComponent : AbstractMonoBehaviour
{
	[Serializable]
	public class SoundGroup
	{
		[Serializable]
		public class Source
		{
			[SerializeField]
			internal AudioSource audio;

			public float originalVolume;

			public bool wasJustPlayed;

			public bool isFadedOut;

			public bool noLoop;

			internal void Init(bool initializeDeferrals)
			{
				if (initializeDeferrals)
				{
					DeferredAudioSource component = audio.GetComponent<DeferredAudioSource>();
					if (component != null)
					{
						component.Initialize();
					}
				}
				if (audio != null && audio.clip != null)
				{
					audio.ignoreListenerPause = true;
					originalVolume = audio.volume;
				}
			}

			public void SetVolume(float v)
			{
				if (audio != null && audio.clip != null)
				{
					audio.volume = v * originalVolume;
				}
			}

			public void Play()
			{
				if (audio != null && audio.clip != null)
				{
					audio.PlayOneShot(audio.clip);
					if (ShowAudioPlaying && !ShowAudioVariations)
					{
					}
				}
			}

			public void PlayLooped()
			{
				if (audio != null && audio.clip != null)
				{
					audio.loop = true;
					audio.Play();
					if (ShowAudioPlaying && !ShowAudioVariations)
					{
					}
				}
			}

			public IEnumerator change_pitch_cr(float end, float time)
			{
				float t = 0f;
				if (audio != null && audio.clip != null)
				{
					while (t < time)
					{
						float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / time);
						audio.pitch = Mathf.Lerp(audio.pitch, end, val);
						t += Time.deltaTime;
						yield return null;
					}
					audio.pitch = end;
				}
			}

			public IEnumerator change_volume_cr(float endVolume, float time, bool onFadeOut)
			{
				float t = 0f;
				float startVol = ((!onFadeOut) ? 0f : audio.volume);
				float endVol = ((!onFadeOut) ? audio.volume : endVolume);
				if (!onFadeOut)
				{
					audio.Play();
					isFadedOut = false;
				}
				else
				{
					isFadedOut = true;
				}
				if (audio != null && audio.clip != null)
				{
					while (t < time)
					{
						float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / time);
						audio.volume = Mathf.Lerp(startVol, endVol, val);
						t += Time.deltaTime;
						yield return null;
					}
					if (onFadeOut)
					{
						audio.Stop();
						audio.volume = originalVolume;
						isFadedOut = true;
					}
				}
				yield return null;
			}

			public IEnumerator warble_pitch_cr(int warbles, float[] minValue, float[] maxValue, float[] incrementAmount, float[] playTime)
			{
				bool isDecreasing = Rand.Bool();
				float t = 0f;
				float startPitch = 1f;
				if (!(audio != null) || !(audio.clip != null))
				{
					yield break;
				}
				for (int i = 0; i < warbles; i++)
				{
					while (t < playTime[i])
					{
						t += (float)CupheadTime.Delta;
						if (isDecreasing)
						{
							if (audio.pitch > minValue[i])
							{
								audio.pitch -= incrementAmount[i];
							}
							else
							{
								isDecreasing = false;
							}
						}
						else if (audio.pitch < maxValue[i])
						{
							audio.pitch += incrementAmount[i];
						}
						else
						{
							isDecreasing = true;
						}
						yield return null;
					}
					t = 0f;
					yield return null;
				}
				audio.pitch = startPitch;
			}

			public void Stop()
			{
				if (audio != null && audio.clip != null)
				{
					audio.loop = false;
					audio.Stop();
				}
			}

			public void Pause()
			{
				if (audio != null && audio.clip != null)
				{
					audio.Pause();
				}
			}

			public void UnPause()
			{
				if (audio != null && audio.clip != null)
				{
					audio.UnPause();
				}
			}

			public void Pan(float pan)
			{
				if (audio != null && audio.clip != null)
				{
					audio.panStereo = pan;
				}
			}

			public float ClipLength()
			{
				if (audio != null && audio.clip != null)
				{
					return audio.clip.length;
				}
				Debug.LogError("Clip is null");
				return 0f;
			}

			public void FollowObject(Vector3 position)
			{
				if (audio != null)
				{
					audio.transform.position = position;
				}
			}

			public bool isPlaying()
			{
				if (audio != null && audio.clip != null)
				{
					return audio.isPlaying;
				}
				return false;
			}

			public void OnAttenuate(bool attenuating, float volumeChange)
			{
				if (audio != null && audio.clip != null)
				{
					if (attenuating)
					{
						audio.volume = volumeChange;
					}
					else
					{
						audio.volume = originalVolume;
					}
				}
			}
		}

		[SerializeField]
		private List<Source> sources = new List<Source>
		{
			new Source()
		};

		public Sfx trigger;

		public string key;

		private bool isPlaying;

		public Transform emissionTransform;

		public bool activatedManually;

		public bool isFadedOut;

		private float volume;

		internal void Init(bool initializeDeferrals = false)
		{
			key = key.ToLowerIfNecessary();
			for (int i = 0; i < sources.Count; i++)
			{
				if (sources[i].audio == null)
				{
					sources.RemoveAt(i);
					i--;
				}
			}
			foreach (Source source in sources)
			{
				source.Init(initializeDeferrals);
			}
		}

		internal void SetMixerGroup(AudioMixerGroup group)
		{
			foreach (Source source in sources)
			{
				if (source.audio != null && source.audio.outputAudioMixerGroup == null)
				{
					source.audio.outputAudioMixerGroup = group;
				}
			}
		}

		public void SetVolume(float v)
		{
			foreach (Source source in sources)
			{
				source.SetVolume(v);
			}
		}

		public void Play()
		{
			Source source = GetSource();
			if (sources.Count > 1)
			{
				foreach (Source source2 in sources)
				{
					if (source.wasJustPlayed)
					{
						source = GetSource();
						continue;
					}
					break;
				}
			}
			source.wasJustPlayed = true;
			source.Play();
			foreach (Source source3 in sources)
			{
				if (source3 != source)
				{
					source3.wasJustPlayed = false;
				}
			}
		}

		public void PlayLoop()
		{
			GetSource().PlayLooped();
		}

		public void Pan(float pan)
		{
			foreach (Source source in sources)
			{
				source.Pan(pan);
			}
		}

		public void FollowObject(Vector3 position)
		{
			for (int i = 0; i < sources.Count; i++)
			{
				Source source = sources[i];
				source.FollowObject(position);
			}
		}

		public bool CheckIfPlaying()
		{
			isPlaying = false;
			foreach (Source source in sources)
			{
				source.isPlaying();
				if (source.isPlaying())
				{
					isPlaying = true;
				}
			}
			return isPlaying;
		}

		public float ClipLength()
		{
			float result = 0f;
			foreach (Source source in sources)
			{
				result = source.ClipLength();
			}
			return result;
		}

		public void OnAttenuate(bool attentuating, float endVolume)
		{
			foreach (Source source in sources)
			{
				source.OnAttenuate(attentuating, endVolume);
			}
		}

		public IEnumerator warble_pitch_cr(int warbles, float[] minValue, float[] maxValue, float[] incrementAmount, float[] playTime)
		{
			bool isDecreasing = Rand.Bool();
			float t = 0f;
			float startPitch = 1f;
			foreach (Source s in sources)
			{
				if (s == null || !(s.audio.clip != null))
				{
					continue;
				}
				for (int i = 0; i < warbles; i++)
				{
					while (t < playTime[i])
					{
						t += (float)CupheadTime.Delta;
						if (isDecreasing)
						{
							if (s.audio.pitch > minValue[i])
							{
								s.audio.pitch -= incrementAmount[i];
							}
							else
							{
								isDecreasing = false;
							}
						}
						else if (s.audio.pitch < maxValue[i])
						{
							s.audio.pitch += incrementAmount[i];
						}
						else
						{
							isDecreasing = true;
						}
						yield return null;
					}
					t = 0f;
					yield return null;
				}
				s.audio.pitch = startPitch;
			}
		}

		public IEnumerator change_pitch_sfx(float end, float time)
		{
			foreach (Source s in sources)
			{
				float t = 0f;
				if (s != null && s.audio.clip != null)
				{
					while (t < time)
					{
						float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / time);
						s.audio.pitch = Mathf.Lerp(s.audio.pitch, end, val);
						t += Time.deltaTime;
						yield return null;
					}
					s.audio.pitch = end;
				}
				yield return null;
			}
		}

		public IEnumerator change_volume_sfx(float start, float end, float time, bool linear)
		{
			foreach (Source s in sources)
			{
				float t = 0f;
				if (s != null && s.audio.clip != null)
				{
					float initialVolume = ((!(start >= 0f)) ? s.audio.volume : start);
					if (!linear && start >= 0f)
					{
						s.audio.volume = initialVolume;
					}
					while (t < time)
					{
						float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / time);
						if (linear)
						{
							s.audio.volume = Mathf.Lerp(initialVolume, end, val);
						}
						else
						{
							s.audio.volume = Mathf.Lerp(s.audio.volume, end, val);
						}
						t += Time.deltaTime;
						yield return null;
					}
					s.audio.volume = end;
					if (end == 0f)
					{
						s.audio.Stop();
					}
				}
				yield return null;
			}
		}

		public IEnumerator change_volume_cr(float endVolume, float time, bool onFadeOut)
		{
			foreach (Source s in sources)
			{
				float t = 0f;
				float startVol = ((!onFadeOut) ? 0f : s.audio.volume);
				float endVol = ((!onFadeOut) ? s.audio.volume : endVolume);
				if (!onFadeOut)
				{
					s.audio.Play();
					isFadedOut = false;
				}
				else
				{
					isFadedOut = true;
				}
				if (s.audio != null && s.audio.clip != null)
				{
					while (t < time)
					{
						float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / time);
						s.audio.volume = Mathf.Lerp(startVol, endVol, val);
						t += Time.deltaTime;
						yield return null;
					}
					if (onFadeOut)
					{
						s.audio.Stop();
						s.audio.volume = s.originalVolume;
						isFadedOut = true;
					}
				}
				yield return null;
			}
		}

		public void Stop()
		{
			foreach (Source source in sources)
			{
				source.Stop();
			}
		}

		public void Pause()
		{
			foreach (Source source in sources)
			{
				source.Pause();
			}
		}

		public void Unpause()
		{
			foreach (Source source in sources)
			{
				source.UnPause();
			}
		}

		private Source GetSource()
		{
			return sources[UnityEngine.Random.Range(0, sources.Count)];
		}
	}

	[SerializeField]
	private AudioManager.Channel channel;

	[SerializeField]
	private List<SoundGroup.Source> bgmSources;

	[SerializeField]
	private List<SoundGroup.Source> bgmAlternates;

	[SerializeField]
	private List<SoundGroup> sounds = new List<SoundGroup>();

	[SerializeField]
	private List<SoundGroup> bgmPlaylist = new List<SoundGroup>();

	[SerializeField]
	private bool autoplayBGM = true;

	[SerializeField]
	private bool autoplayBGMPlaylist = true;

	[SerializeField]
	private float[] minValue;

	private Dictionary<string, SoundGroup> dict;

	public static bool ShowAudioPlaying;

	public static bool ShowAudioVariations;

	protected override void Awake()
	{
		base.Awake();
		SetChannels();
		dict = new Dictionary<string, SoundGroup>();
		foreach (SoundGroup sound in sounds)
		{
			sound.Init(initializeDeferrals: true);
			dict[sound.key.ToLowerIfNecessary()] = sound;
		}
		foreach (SoundGroup item in bgmPlaylist)
		{
			item.Init(initializeDeferrals: true);
			dict[item.key.ToLowerIfNecessary()] = item;
		}
		foreach (SoundGroup.Source bgmAlternate in bgmAlternates)
		{
			bgmAlternate.Init(initializeDeferrals: true);
		}
		foreach (SoundGroup.Source bgmSource in bgmSources)
		{
			bgmSource.Init(initializeDeferrals: true);
		}
		AddEvents();
	}

	private void OnDestroy()
	{
		RemoveEvents();
	}

	private void OnValidate()
	{
		foreach (SoundGroup sound in sounds)
		{
			if (string.IsNullOrEmpty(sound.key))
			{
				sound.key = sound.trigger.ToString();
			}
			sound.key = sound.key.ToLower();
		}
		foreach (SoundGroup item in bgmPlaylist)
		{
			if (string.IsNullOrEmpty(item.key))
			{
				item.key = item.trigger.ToString();
			}
			item.key = item.key.ToLower();
		}
	}

	private void AddEvents()
	{
		AudioManager.OnPlayBGMEvent += StartBGM;
		AudioManager.OnPlayBGMPlaylistEvent += StartBGMPlaylist;
		AudioManager.OnSnapshotEvent += SnapshotTransition;
		AudioManager.OnCheckEvent.Add(OnIsPlaying);
		AudioManager.OnPlayEvent += OnPlay;
		AudioManager.OnPlayLoopEvent += OnPlayLoop;
		AudioManager.OnStopEvent += OnStop;
		AudioManager.OnPauseEvent += OnPause;
		AudioManager.OnUnpauseEvent += OnUnpause;
		AudioManager.OnFollowObject += OnFollowOject;
		AudioManager.OnPanEvent += OnPan;
		AudioManager.OnStopAllEvent += OnStopAll;
		AudioManager.OnStopBGMEvent += OnStopBGM;
		AudioManager.OnPauseAllSFXEvent += OnPauseAllSFX;
		AudioManager.OnUnpauseAllSFXEvent += OnUnpauseAllSFX;
		AudioManager.OnBGMSlowdown += OnBGMSlowdown;
		AudioManager.OnSFXSlowDown += OnSFXSlowDown;
		AudioManager.OnSFXFadeVolume += OnSFXVolume;
		AudioManager.OnSFXFadeVolumeLinear += OnSFXVolumeLinear;
		AudioManager.OnBGMPitchWarble += OnBGMWarblePitch;
		AudioManager.OnAttenuation += OnAttenuation;
		AudioManager.OnPlayManualBGM += PlayManualBGMTrack;
		AudioManager.OnStopManualBGMTrackEvent += StopManualBGMTrack;
		AudioManager.OnBGMFadeVolume += OnBGMVolumeFade;
		AudioManager.OnStartBGMAlternate += StartBGMAlternate;
		if (autoplayBGM)
		{
			SceneLoader.OnLoaderCompleteEvent += StartBGM;
		}
		if (autoplayBGMPlaylist)
		{
			SceneLoader.OnLoaderCompleteEvent += StartBGMPlaylist;
		}
	}

	private void RemoveEvents()
	{
		AudioManager.OnPlayBGMEvent -= StartBGM;
		AudioManager.OnPlayBGMPlaylistEvent -= StartBGMPlaylist;
		AudioManager.OnSnapshotEvent -= SnapshotTransition;
		AudioManager.OnCheckEvent.Remove(OnIsPlaying);
		AudioManager.OnPlayEvent -= OnPlay;
		AudioManager.OnPlayLoopEvent -= OnPlayLoop;
		AudioManager.OnStopEvent -= OnStop;
		AudioManager.OnPauseEvent -= OnPause;
		AudioManager.OnUnpauseEvent -= OnUnpause;
		AudioManager.OnFollowObject -= OnFollowOject;
		AudioManager.OnPanEvent -= OnPan;
		AudioManager.OnStopAllEvent -= OnStopAll;
		AudioManager.OnStopBGMEvent -= OnStopBGM;
		AudioManager.OnPauseAllSFXEvent -= OnPauseAllSFX;
		AudioManager.OnUnpauseAllSFXEvent -= OnUnpauseAllSFX;
		AudioManager.OnBGMSlowdown -= OnBGMSlowdown;
		AudioManager.OnSFXSlowDown -= OnSFXSlowDown;
		AudioManager.OnSFXFadeVolume -= OnSFXVolume;
		AudioManager.OnSFXFadeVolumeLinear -= OnSFXVolumeLinear;
		AudioManager.OnBGMPitchWarble -= OnBGMWarblePitch;
		AudioManager.OnAttenuation -= OnAttenuation;
		AudioManager.OnPlayManualBGM -= PlayManualBGMTrack;
		AudioManager.OnStopManualBGMTrackEvent -= StopManualBGMTrack;
		AudioManager.OnBGMFadeVolume -= OnBGMVolumeFade;
		AudioManager.OnStartBGMAlternate -= StartBGMAlternate;
		if (autoplayBGM)
		{
			SceneLoader.OnLoaderCompleteEvent -= StartBGM;
		}
		if (autoplayBGMPlaylist)
		{
			SceneLoader.OnLoaderCompleteEvent -= StartBGMPlaylist;
		}
	}

	private void Update()
	{
		for (int i = 0; i < sounds.Count; i++)
		{
			SoundGroup soundGroup = sounds[i];
			if (soundGroup.emissionTransform != null)
			{
				soundGroup.FollowObject(soundGroup.emissionTransform.position);
			}
		}
	}

	private void StartBGM()
	{
		StopBGM();
		foreach (SoundGroup.Source bgmSource in bgmSources)
		{
			if (bgmSource.noLoop)
			{
				bgmSource.Play();
			}
			else
			{
				bgmSource.PlayLooped();
			}
		}
	}

	private void StartBGMAlternate(int index)
	{
		StopBGM();
		if (bgmAlternates.Count > index && bgmAlternates[index] != null)
		{
			if (bgmAlternates[index].noLoop)
			{
				bgmAlternates[index].Play();
			}
			else
			{
				bgmAlternates[index].PlayLooped();
			}
		}
	}

	private void StopBGM()
	{
		foreach (SoundGroup.Source bgmSource in bgmSources)
		{
			bgmSource.Stop();
		}
		foreach (SoundGroup.Source bgmAlternate in bgmAlternates)
		{
			bgmAlternate.Stop();
		}
	}

	private void OnLevelStart()
	{
		StartBGM();
	}

	private void OnStopBGM()
	{
		StopBGM();
	}

	private void OnBGMSlowdown(float end, float time)
	{
		foreach (SoundGroup.Source bgmSource in bgmSources)
		{
			StartCoroutine(bgmSource.change_pitch_cr(end, time));
		}
		foreach (SoundGroup.Source bgmAlternate in bgmAlternates)
		{
			StartCoroutine(bgmAlternate.change_pitch_cr(end, time));
		}
		for (int i = 0; i < bgmPlaylist.Count; i++)
		{
			if (bgmPlaylist[i].CheckIfPlaying())
			{
				StartCoroutine(bgmPlaylist[i].change_pitch_sfx(end, time));
			}
		}
	}

	private void OnBGMVolumeFade(float end, float time, bool onFadeout)
	{
		foreach (SoundGroup.Source bgmSource in bgmSources)
		{
			if ((bgmSource.isPlaying() && onFadeout) || (!onFadeout && bgmSource.isFadedOut))
			{
				StartCoroutine(bgmSource.change_volume_cr(end, time, onFadeout));
			}
		}
		foreach (SoundGroup.Source bgmAlternate in bgmAlternates)
		{
			if ((bgmAlternate.isPlaying() && onFadeout) || (!onFadeout && bgmAlternate.isFadedOut))
			{
				StartCoroutine(bgmAlternate.change_volume_cr(end, time, onFadeout));
			}
		}
		for (int i = 0; i < bgmPlaylist.Count; i++)
		{
			if ((bgmPlaylist[i].CheckIfPlaying() && onFadeout) || (!onFadeout && bgmPlaylist[i].isFadedOut))
			{
				StartCoroutine(bgmPlaylist[i].change_volume_cr(end, time, onFadeout));
			}
		}
	}

	private void OnBGMWarblePitch(int warbles, float[] minValue, float[] maxValue, float[] warbleTime, float[] playTime)
	{
		foreach (SoundGroup.Source bgmSource in bgmSources)
		{
			StartCoroutine(bgmSource.warble_pitch_cr(warbles, minValue, maxValue, warbleTime, playTime));
		}
		for (int i = 0; i < bgmPlaylist.Count; i++)
		{
			if (bgmPlaylist[i].CheckIfPlaying())
			{
				StartCoroutine(bgmPlaylist[i].warble_pitch_cr(warbles, minValue, maxValue, warbleTime, playTime));
			}
		}
	}

	public void PlayManualBGMTrack(bool loopPlayListAfter)
	{
		for (int i = 0; i < bgmPlaylist.Count; i++)
		{
			if (bgmPlaylist[i].activatedManually)
			{
				if (loopPlayListAfter)
				{
					bgmPlaylist[i].Play();
					StartCoroutine(handle_cr(bgmPlaylist[i].ClipLength()));
				}
				else
				{
					bgmPlaylist[i].PlayLoop();
				}
			}
		}
	}

	private IEnumerator handle_cr(float clipLength)
	{
		yield return new WaitForSeconds(clipLength);
		StartBGMPlaylist();
		yield return null;
	}

	public void StopManualBGMTrack()
	{
		for (int i = 0; i < bgmPlaylist.Count; i++)
		{
			if (bgmPlaylist[i].activatedManually)
			{
				bgmPlaylist[i].Stop();
			}
		}
	}

	private void StartBGMPlaylist()
	{
		bool flag = true;
		for (int i = 0; i < bgmPlaylist.Count; i++)
		{
			if (!bgmPlaylist[i].activatedManually)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			StopBGM();
			if (bgmPlaylist.Count > 0)
			{
				PlayerData.PlayerLevelDataObject levelData = PlayerData.Data.GetLevelData(SceneLoader.CurrentLevel);
				levelData.bgmPlayListCurrent = (levelData.bgmPlayListCurrent + 1) % bgmPlaylist.Count;
				StartCoroutine(play_track_cr());
			}
		}
	}

	private IEnumerator play_track_cr()
	{
		PlayerData.PlayerLevelDataObject levelData = PlayerData.Data.GetLevelData(SceneLoader.CurrentLevel);
		while (true)
		{
			if (bgmPlaylist[levelData.bgmPlayListCurrent].activatedManually)
			{
				levelData.bgmPlayListCurrent = (levelData.bgmPlayListCurrent + 1) % bgmPlaylist.Count;
				yield return new WaitForFixedUpdate();
				continue;
			}
			bgmPlaylist[levelData.bgmPlayListCurrent].Play();
			yield return new WaitForSeconds(bgmPlaylist[levelData.bgmPlayListCurrent].ClipLength());
			levelData.bgmPlayListCurrent = (levelData.bgmPlayListCurrent + 1) % bgmPlaylist.Count;
			yield return new WaitForFixedUpdate();
		}
	}

	private void OnPlay(string key)
	{
		if (dict.ContainsKey(key))
		{
			if (ShowAudioVariations || ShowAudioPlaying)
			{
			}
			dict[key].Play();
		}
	}

	private void OnPlayLoop(string key)
	{
		if (dict.ContainsKey(key))
		{
			if (ShowAudioVariations || ShowAudioPlaying)
			{
			}
			dict[key].PlayLoop();
		}
	}

	private void OnStop(string key)
	{
		if (dict.ContainsKey(key))
		{
			dict[key].Stop();
		}
	}

	private void OnPause(string key)
	{
		if (dict.ContainsKey(key))
		{
			dict[key].Pause();
		}
	}

	private void OnUnpause(string key)
	{
		if (dict.ContainsKey(key))
		{
			dict[key].Unpause();
		}
	}

	private void OnPauseAllSFX()
	{
		foreach (SoundGroup sound in sounds)
		{
			sound.Pause();
		}
	}

	private void OnUnpauseAllSFX()
	{
		foreach (SoundGroup sound in sounds)
		{
			sound.Unpause();
		}
	}

	private void OnFollowOject(string key, Transform transform)
	{
		if (dict.ContainsKey(key))
		{
			dict[key].emissionTransform = transform;
			dict[key].FollowObject(dict[key].emissionTransform.position);
		}
	}

	private bool OnIsPlaying(string key)
	{
		if (dict.ContainsKey(key))
		{
			return dict[key].CheckIfPlaying();
		}
		return false;
	}

	private void OnAttenuation(string key, bool attenuating, float endVolume)
	{
		if (dict.ContainsKey(key))
		{
			dict[key].OnAttenuate(attenuating, endVolume);
		}
	}

	private void OnPan(string key, float value)
	{
		if (dict.ContainsKey(key))
		{
			dict[key].Pan(value);
		}
	}

	private void OnStopAll()
	{
		foreach (SoundGroup sound in sounds)
		{
			sound.Stop();
		}
	}

	private void OnSFXSlowDown(string key, float end, float time)
	{
		if (dict.ContainsKey(key))
		{
			StartCoroutine(dict[key].change_pitch_sfx(end, time));
		}
	}

	private void OnSFXVolume(string key, float start, float end, float time)
	{
		if (dict.ContainsKey(key))
		{
			StartCoroutine(dict[key].change_volume_sfx(start, end, time, linear: false));
		}
	}

	private void OnSFXVolumeLinear(string key, float start, float end, float time)
	{
		if (dict.ContainsKey(key))
		{
			StartCoroutine(dict[key].change_volume_sfx(start, end, time, linear: true));
		}
	}

	private void SnapshotTransition(string[] snapshotNames, float[] weights, float time)
	{
		AudioManagerMixer.Groups groups = AudioManagerMixer.GetGroups();
		List<AudioMixerGroup> list = new List<AudioMixerGroup>();
		List<AudioMixerSnapshot> list2 = new List<AudioMixerSnapshot>();
		list.Add(groups.master);
		list.Add(groups.bgm_Options);
		list.Add(groups.sfx_Options);
		list.Add(groups.master_Options);
		list.Add(groups.sfx);
		list.Add(groups.levelSfx);
		list.Add(groups.ambience);
		list.Add(groups.creatures);
		list.Add(groups.announcer);
		list.Add(groups.super);
		list.Add(groups.bgm);
		list.Add(groups.levelBgm);
		list.Add(groups.musicSting);
		list.Add(groups.noise);
		list.Add(groups.noiseConstant);
		list.Add(groups.noiseShortterm);
		list.Add(groups.noise1920s);
		for (int i = 0; i < weights.Length; i++)
		{
			if (list[i].audioMixer.FindSnapshot(snapshotNames[i]) != null)
			{
				list2.Add(list[0].audioMixer.FindSnapshot(snapshotNames[i]));
			}
			else
			{
				Debug.LogError("Snapshot string is invalid");
			}
		}
		foreach (AudioMixerGroup item in list)
		{
			item.audioMixer.TransitionToSnapshots(list2.ToArray(), weights, time);
		}
	}

	private void SetChannels()
	{
		AudioManagerMixer.Groups groups = AudioManagerMixer.GetGroups();
		AudioManager.Channel channel = this.channel;
		AudioMixerGroup audioMixerGroup;
		AudioMixerGroup mixerGroup;
		if (channel == AudioManager.Channel.Default || channel != AudioManager.Channel.Level)
		{
			audioMixerGroup = groups.bgm;
			mixerGroup = groups.sfx;
			AudioMixerGroup noiseConstant = groups.noiseConstant;
			AudioMixerGroup noiseShortterm = groups.noiseShortterm;
		}
		else
		{
			audioMixerGroup = groups.levelBgm;
			mixerGroup = groups.levelSfx;
		}
		foreach (SoundGroup.Source bgmSource in bgmSources)
		{
			if (bgmSource.audio.outputAudioMixerGroup == null)
			{
				bgmSource.audio.outputAudioMixerGroup = audioMixerGroup;
			}
		}
		foreach (SoundGroup sound in sounds)
		{
			sound.SetMixerGroup(mixerGroup);
		}
		foreach (SoundGroup item in bgmPlaylist)
		{
			item.SetMixerGroup(audioMixerGroup);
		}
	}
}
