using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class VoiceManager : MonoBehaviour
{
	public struct QueuedVoice
	{
		public Transform sentient;

		public SoundPack.SoundData soundData;

		public float timeAtQueue;

		public float queueDuration;

		public QueuedVoice(Transform sentient, SoundPack.SoundData soundData, float queueDuration)
		{
			this.sentient = sentient;
			this.soundData = soundData;
			timeAtQueue = Time.unscaledTime;
			this.queueDuration = queueDuration;
		}

		public bool ShouldPlay()
		{
			if ((bool)sentient)
			{
				return Time.unscaledTime - timeAtQueue <= queueDuration;
			}
			return false;
		}
	}

	private static VoiceManager _Instance;

	private static GameObject _VoiceSourceBlueprint;

	private static System.Random _Random;

	private VoiceSource _globalActiveSource;

	private Queue<QueuedVoice> _globalQueue = new Queue<QueuedVoice>();

	private Dictionary<Transform, VoiceSource> _activeSources = new Dictionary<Transform, VoiceSource>();

	private Dictionary<Transform, Queue<QueuedVoice>> _queues = new Dictionary<Transform, Queue<QueuedVoice>>();

	public static VoiceManager Instance => ManagerUtil.GetSingletonInstance(ref _Instance);

	public static GameObject VoiceSourceBlueprint
	{
		get
		{
			if (!(_VoiceSourceBlueprint != null))
			{
				return _VoiceSourceBlueprint = Resources.Load<GameObject>("Audio/Voice/VoiceSource");
			}
			return _VoiceSourceBlueprint;
		}
	}

	static VoiceManager()
	{
		_Random = new System.Random();
		Pools.CreatePoolQueue<QueuedVoice>();
	}

	private void Awake()
	{
		SceneRef.OnSceneUnloaded += _Clear;
	}

	private void Update()
	{
		if ((bool)_globalActiveSource && _globalActiveSource.Finished())
		{
			_globalActiveSource = null;
		}
		if (_globalActiveSource == null)
		{
			while (_globalQueue.Count > 0)
			{
				QueuedVoice queuedVoice = _globalQueue.Dequeue();
				if (queuedVoice.ShouldPlay())
				{
					Play(queuedVoice.sentient, queuedVoice.soundData, interrupt: false, 0f, isGlobal: true);
					break;
				}
			}
		}
		foreach (KeyValuePair<Transform, VoiceSource> item in _activeSources.EnumeratePairsSafe())
		{
			if (item.Value.Finished())
			{
				_activeSources.Remove(item.Key);
			}
		}
		foreach (KeyValuePair<Transform, Queue<QueuedVoice>> item2 in _queues.EnumeratePairsSafe())
		{
			if (_activeSources.ContainsKey(item2.Key))
			{
				continue;
			}
			while (item2.Value.Count > 0)
			{
				QueuedVoice queuedVoice2 = item2.Value.Dequeue();
				if (queuedVoice2.ShouldPlay())
				{
					Play(queuedVoice2.sentient, queuedVoice2.soundData, interrupt: false, 0f);
					break;
				}
			}
			if (item2.Value.Count <= 0)
			{
				Pools.Repool(item2.Value);
				_queues.Remove(item2.Key);
			}
		}
	}

	private void OnDestroy()
	{
		SceneRef.OnSceneUnloaded -= _Clear;
	}

	private void _Clear(Scene unloadedScene)
	{
		if (SceneRef.Game != unloadedScene)
		{
			return;
		}
		Stop();
		DictionaryValueEnumerator<Transform, VoiceSource>.Enumerator enumerator = _activeSources.EnumerateValues().GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.InterruptSafe();
		}
		_activeSources.Clear();
		foreach (KeyValuePair<Transform, Queue<QueuedVoice>> item in _queues.EnumeratePairsSafe())
		{
			Pools.Repool(item.Value);
			_queues.Remove(item.Key);
		}
	}

	private bool _ShouldPlay(Transform sentientView, SoundPack.SoundData soundData, bool interrupt, float queueDuration, bool placeInGlobalQueue)
	{
		if (placeInGlobalQueue)
		{
			if ((bool)_globalActiveSource)
			{
				if (!interrupt)
				{
					if (queueDuration > 0f)
					{
						_globalQueue.Enqueue(new QueuedVoice(sentientView, soundData, queueDuration));
					}
					return false;
				}
				_globalActiveSource.InterruptSafe();
			}
		}
		else if ((bool)sentientView && _activeSources.ContainsKey(sentientView))
		{
			if (!interrupt)
			{
				if (queueDuration > 0f)
				{
					if (!_queues.ContainsKey(sentientView))
					{
						_queues.Add(sentientView, Pools.Unpool<Queue<QueuedVoice>>());
					}
					_queues[sentientView].Enqueue(new QueuedVoice(sentientView, soundData, queueDuration));
				}
				return false;
			}
			_activeSources[sentientView].InterruptSafe();
		}
		return true;
	}

	public VoiceSource Play(SoundPack.SoundData soundData, bool interrupt = false, float queueDuration = 1f, bool isGlobal = true, AudioMixerGroup mixerGroup = null)
	{
		if (!soundData)
		{
			return null;
		}
		if (!_ShouldPlay(null, soundData, interrupt, queueDuration, isGlobal))
		{
			return null;
		}
		VoiceSource component = Pools.Unpool(VoiceSourceBlueprint, base.transform).GetComponent<VoiceSource>();
		component.spatialBlend = 0f;
		component.outputGroup = mixerGroup ?? MasterMixManager.SoundEffects;
		component.Play(soundData, null);
		if (isGlobal)
		{
			_globalActiveSource = component;
		}
		return component;
	}

	public VoiceSource Play(Transform sentientView, SoundPack.SoundData soundData, bool interrupt = false, float queueDuration = 1f, bool isGlobal = false, AudioMixerGroup mixerGroup = null)
	{
		if (!soundData)
		{
			return null;
		}
		if (!_ShouldPlay(sentientView, soundData, interrupt, queueDuration, isGlobal))
		{
			return null;
		}
		VoiceSource component = Pools.Unpool(VoiceSourceBlueprint, sentientView.position, null, base.transform).GetComponent<VoiceSource>();
		component.spatialBlend = 1f;
		component.outputGroup = mixerGroup ?? MasterMixManager.SoundEffects;
		component.Play(soundData, sentientView);
		if (isGlobal)
		{
			_globalActiveSource = component;
		}
		else
		{
			_activeSources[sentientView] = component;
		}
		return component;
	}

	public VoiceSource Play(Transform sentientView, SoundPack soundPack, bool interrupt = false, float queueDuration = 1f, bool isGlobal = false, AudioMixerGroup mixerGroup = null)
	{
		if (!soundPack)
		{
			return null;
		}
		return Play(sentientView, soundPack.GetSound(_Random), interrupt, queueDuration, isGlobal, mixerGroup);
	}

	public void Stop()
	{
		if ((bool)_globalActiveSource)
		{
			_globalActiveSource.InterruptSafe();
		}
		_globalActiveSource = null;
		_globalQueue.Clear();
	}
}
