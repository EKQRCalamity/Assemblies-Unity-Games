using System;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Audio;

[ProtoContract]
[UIField]
public class SimpleSoundPack
{
	private const float MIN_VOLUME_RANGE = 0.75f;

	private const float MAX_VOLUME_RANGE = 1f;

	private static readonly RangeF VOLUME_RANGE = new RangeF(0.75f, 1f, 0.25f, 1f, 0f, float.MaxValue);

	private const float MIN_PITCH_RANGE = 0.75f;

	private const float MAX_PITCH_RANGE = 1.25f;

	private static readonly RangeF PITCH_RANGE = new RangeF(0.75f, 1.25f, 0.1f, 10f, 0f, float.MaxValue);

	[ProtoMember(1)]
	[UIField(validateOnChange = true, dynamicInitMethod = "_InitAudio")]
	private AudioRef _audio;

	[ProtoMember(2)]
	[UIField("Volume (Range)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
	private RangeF _volumeRange = VOLUME_RANGE;

	[ProtoMember(3)]
	[UIField("Pitch (Range)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
	private RangeF _pitchRange = PITCH_RANGE;

	[ProtoMember(14)]
	private AudioCategoryTypeFlags? _additionalCategories;

	[ProtoMember(15, IsRequired = true)]
	private AudioCategoryType _category;

	private Vector3 _position;

	private AudioMixerGroup _group;

	private float _volume;

	private float _pitch;

	private Action<AudioClip> _PlaySoundAction;

	private AudioRef audioRef => _audio ?? (_audio = new AudioRef(_category));

	public AudioCategoryType category
	{
		get
		{
			return _category;
		}
		set
		{
			_category = value;
		}
	}

	public AudioCategoryTypeFlags? additionalCategories
	{
		get
		{
			return _additionalCategories;
		}
		set
		{
			_additionalCategories = value;
		}
	}

	private Action<AudioClip> PlaySoundAction => _PlaySound;

	private bool _audioSpecified => _audio.ShouldSerialize();

	private bool _volumeRangeSpecified => _volumeRange != VOLUME_RANGE;

	private bool _pitchRangeSpecified => _pitchRange != PITCH_RANGE;

	public SimpleSoundPack()
	{
	}

	public SimpleSoundPack(AudioCategoryType category)
	{
		_category = category;
	}

	public SimpleSoundPack(AudioCategoryType category, AudioCategoryTypeFlags additionalCategories)
		: this(category)
	{
		_additionalCategories = additionalCategories;
	}

	private void _PlaySound(AudioClip clip)
	{
		AudioPool.Instance.Play(clip, _position, _group, _volume, _pitch);
	}

	public bool PlaySound(System.Random random, Vector3 position, AudioMixerGroup group = null, PooledAudioCategory? category = null, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
	{
		if (!this || !AudioPool.Instance.ShouldPlay(category, audioRef.loadedAudioClip, position))
		{
			return false;
		}
		_position = position;
		_group = group;
		_volume = random.Range(_volumeRange) * volumeMultiplier;
		_pitch = random.Range(_pitchRange) * pitchMultiplier;
		audioRef.GetAudioClip(PlaySoundAction);
		return true;
	}

	private void OnValidateUI()
	{
		if (!_additionalCategories.HasValue)
		{
			audioRef.category = _category;
		}
	}

	private void _InitAudio(UIFieldAttribute uiField)
	{
		uiField.filter = _category;
		if (_additionalCategories.HasValue)
		{
			uiField.stepSize = _additionalCategories.Value;
		}
	}

	public override string ToString()
	{
		if (!audioRef.IsValid())
		{
			return "N/A";
		}
		return audioRef.friendlyName;
	}

	public static implicit operator bool(SimpleSoundPack soundPack)
	{
		return soundPack?._audio.IsValid() ?? false;
	}
}
