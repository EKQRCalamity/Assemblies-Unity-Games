using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class SoundPack
{
	[ProtoContract]
	[UIField]
	public class SoundData
	{
		[ProtoContract(EnumPassthru = true)]
		public enum SoundVolume : byte
		{
			Whisper,
			Soft,
			Full
		}

		[ProtoContract(EnumPassthru = true)]
		public enum SoundRarity : byte
		{
			Rare,
			Uncommon,
			Normal,
			Common,
			VeryCommon
		}

		[ProtoMember(1)]
		[UIField]
		private AudioRef _audio;

		[ProtoMember(5)]
		[UIField]
		[DefaultValue(AudioVolumeType.Max)]
		private AudioVolumeType _volume = AudioVolumeType.Max;

		[ProtoMember(4)]
		[UIField("Chance", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		[DefaultValue(SoundRarity.Normal)]
		private SoundRarity _chanceToPlay = SoundRarity.Normal;

		public AudioRef audioRef
		{
			get
			{
				return _audio ?? (_audio = new AudioRef(EnumUtil<AudioCategoryType>.Min));
			}
			set
			{
				_audio = value;
			}
		}

		public AudioCategoryType category
		{
			get
			{
				return audioRef.category;
			}
			set
			{
				audioRef.category = value;
			}
		}

		public float volume => _volume.Volume();

		public AudioVolumeType volumeType
		{
			set
			{
				_volume = value;
			}
		}

		public float playWeight => _chanceToPlay switch
		{
			SoundRarity.Rare => 0.111f, 
			SoundRarity.Uncommon => 0.333f, 
			SoundRarity.Normal => 1f, 
			SoundRarity.Common => 3f, 
			SoundRarity.VeryCommon => 9f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};

		private bool _audioSpecified => _audio.ShouldSerialize();

		public void GetClip(Action<AudioClip> onClip)
		{
			audioRef.GetAudioClip(onClip);
		}

		public SoundData SetAudioRef(AudioRef audioRefToSet)
		{
			_audio = audioRefToSet;
			return this;
		}

		public override string ToString()
		{
			if (!_audio.Exists())
			{
				return "New Sound Data";
			}
			return _audio.friendlyName;
		}

		public static implicit operator bool(SoundData soundData)
		{
			return soundData?._audio.IsValid() ?? false;
		}
	}

	private static System.Random _Random = new System.Random();

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open)]
	[UIFieldCollectionItem(dynamicInitMethod = "_InitSounds")]
	private List<SoundData> _sounds;

	[ProtoMember(2, IsRequired = true)]
	private AudioCategoryType _category;

	private Dictionary<SoundData, uint> _alreadyPlayed;

	private SoundData _lastPlayed;

	[ProtoMember(4, DataFormat = DataFormat.FixedSize)]
	private AudioCategoryTypeFlags? _additionalCategories;

	public string name
	{
		get
		{
			foreach (SoundData sound in sounds)
			{
				if (sound.audioRef.IsValid())
				{
					return sound.audioRef.friendlyName;
				}
			}
			return "[NO SOUND]";
		}
	}

	protected List<SoundData> sounds => _sounds ?? (_sounds = new List<SoundData>());

	public int count => sounds.Count;

	public int validCount => sounds.Count((SoundData sd) => sd.audioRef.IsValid());

	protected Dictionary<SoundData, uint> alreadyPlayed => _alreadyPlayed ?? (_alreadyPlayed = new Dictionary<SoundData, uint>());

	private bool _hidePlaySound => count == 0;

	private SoundPack()
	{
	}

	public SoundPack(AudioCategoryType category)
	{
		_category = category;
	}

	public SoundPack(AudioCategoryType category, AudioCategoryTypeFlags? additionalCategories)
		: this(category)
	{
		_additionalCategories = additionalCategories;
	}

	public SoundData GetSound(System.Random random, bool skipValidityCheck = true)
	{
		if (sounds.Count == 0)
		{
			return null;
		}
		using PoolKeepItemDictionaryHandle<SoundData, double> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<SoundData, double>();
		if (alreadyPlayed.Count == sounds.Count)
		{
			alreadyPlayed.Clear();
		}
		Dictionary<SoundData, double> value = poolKeepItemDictionaryHandle.value;
		foreach (SoundData sound in sounds)
		{
			if (skipValidityCheck || (bool)sound)
			{
				value.Add(sound, sound.playWeight / Mathf.Pow(3f, (alreadyPlayed.ContainsKey(sound) ? alreadyPlayed[sound] : 0) + ((sound == _lastPlayed) ? 2 : 0)));
			}
		}
		if (value.Count == 0)
		{
			return null;
		}
		SoundData soundData = value.RandomKey(random);
		if (!alreadyPlayed.ContainsKey(soundData))
		{
			alreadyPlayed.Add(soundData, 0u);
		}
		alreadyPlayed[soundData]++;
		_lastPlayed = soundData;
		return soundData;
	}

	public SoundData GetShortestSound()
	{
		if (!_sounds.IsNullOrEmpty())
		{
			return _sounds.MinBy((SoundData s) => s.audioRef.audioClip.length);
		}
		return null;
	}

	public AudioRef GetFirstValidAudioRef()
	{
		return sounds.Select((SoundData s) => s.audioRef).FirstOrDefault((AudioRef a) => a);
	}

	public void PrepareDataForSave()
	{
		if (!_sounds.IsNullOrEmpty())
		{
			_sounds.RemoveAll((SoundData data) => !data.audioRef.IsValid());
		}
	}

	public static implicit operator bool(SoundPack soundPack)
	{
		if (soundPack == null)
		{
			return false;
		}
		return soundPack.count > 0;
	}

	public override string ToString()
	{
		return _sounds.ToStringSmart((SoundData s) => s.audioRef.GetFriendlyName());
	}

	private void OnValidateUI()
	{
		alreadyPlayed.Clear();
		if (_additionalCategories.HasValue)
		{
			return;
		}
		foreach (SoundData sound in sounds)
		{
			sound.category = _category;
		}
	}

	private void _InitSounds(UIFieldAttribute uiField)
	{
		if (_additionalCategories.HasValue)
		{
			uiField.filter = _category;
			uiField.stepSize = _additionalCategories.Value;
		}
	}

	[UIField]
	[UIHideIf("_hidePlaySound")]
	public void PlaySound()
	{
		VoiceManager.Instance.Play(GetSound(_Random, skipValidityCheck: false), interrupt: true);
	}
}
