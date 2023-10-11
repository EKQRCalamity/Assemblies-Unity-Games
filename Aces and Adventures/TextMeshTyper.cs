using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[RequireComponent(typeof(TextMeshProUGUI), typeof(PooledAudioPlayer), typeof(TextMeshProAnimatorTyper))]
public class TextMeshTyper : MonoBehaviour
{
	private static Dictionary<char, float> _SpecialCaseTimings;

	public const float SOFTEN_VOLUME = 0.5f;

	public bool setTextOnEnable;

	public bool useScaledTime = true;

	[SerializeField]
	[Range(1f, 100f)]
	protected float _charactersPerSecond = 40f;

	public bool soundEnabled;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _nonAlphaNumericTiming = 0.25f;

	public bool playSoundForAllCharacters;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _soundOverlap = 0.5f;

	[SerializeField]
	[Range(1f, 10f)]
	protected int _charactersPerSound = 1;

	private TextMeshProUGUI _textMesh;

	private PooledAudioPlayer _audioPlayer;

	private bool _soundEnabled;

	private int _characterIndex;

	private float _elapsedTime;

	private float _durationAdjustment;

	private int _characterSoundIndex;

	private Func<AudioSource> _getAudioSource;

	private bool _softenVolumeWhileAudioSourceIsPlaying;

	private AudioSource _audioSource;

	private TextMeshProAnimatorTyper _typer;

	private static Dictionary<char, float> SpecialCaseTimings
	{
		get
		{
			object obj = _SpecialCaseTimings;
			if (obj == null)
			{
				obj = new Dictionary<char, float>
				{
					{ '-', 1f },
					{ ' ', 2f },
					{ ',', 4f },
					{ ';', 5f },
					{ '.', 6f },
					{ '?', 6f },
					{ '!', 6f },
					{ '…', 12f },
					{ '"', 0.001f }
				};
				_SpecialCaseTimings = (Dictionary<char, float>)obj;
			}
			return (Dictionary<char, float>)obj;
		}
	}

	public float charactersPerSecond
	{
		get
		{
			return _charactersPerSecond;
		}
		set
		{
			_charactersPerSecond = Mathf.Max(1f, value);
		}
	}

	public string text
	{
		get
		{
			return textMesh.text;
		}
		set
		{
			if (text != value)
			{
				SetText(value);
			}
		}
	}

	public List<AudioClip> textSounds
	{
		get
		{
			return audioPlayer.audioPack.clips;
		}
		set
		{
			audioPlayer.audioPack.clips = value;
		}
	}

	public int maxActiveSounds
	{
		get
		{
			return audioPlayer.maxActiveSources;
		}
		set
		{
			audioPlayer.maxActiveSources = value;
		}
	}

	public float volume
	{
		get
		{
			return audioPlayer.volume;
		}
		set
		{
			audioPlayer.volume = value;
		}
	}

	public Vector2 volumeRange
	{
		get
		{
			return audioPlayer.audioPack.volumeRange;
		}
		set
		{
			audioPlayer.audioPack.volumeRange = value;
		}
	}

	public Vector2 pitchRange
	{
		get
		{
			return audioPlayer.audioPack.pitchRange;
		}
		set
		{
			audioPlayer.audioPack.pitchRange = value;
		}
	}

	public float soundOverlap
	{
		get
		{
			return _soundOverlap;
		}
		set
		{
			_soundOverlap = Mathf.Clamp01(value);
		}
	}

	public int charactersPerSound
	{
		get
		{
			return _charactersPerSound;
		}
		set
		{
			_charactersPerSound = Mathf.Clamp(value, 1, 10);
		}
	}

	public Func<AudioSource> getAudioSource
	{
		get
		{
			return _getAudioSource;
		}
		set
		{
			_getAudioSource = value;
			_audioSource = null;
		}
	}

	public bool syncCompletionWithAudioSource { get; set; }

	public bool softenVolumeWhileAudioSourceIsPlaying
	{
		get
		{
			return _softenVolumeWhileAudioSourceIsPlaying;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _softenVolumeWhileAudioSourceIsPlaying, value))
			{
				volume = value.ToFloat(0.5f, 1f);
			}
		}
	}

	private TextMeshProUGUI textMesh => this.CacheComponent(ref _textMesh);

	private PooledAudioPlayer audioPlayer => this.CacheComponentSafe(ref _audioPlayer);

	private TextMeshProAnimatorTyper typer => this.CacheComponentSafe(ref _typer);

	public bool isFinished
	{
		get
		{
			if (typer.maxVisibleCharacters < textMesh.textInfo.characterCount)
			{
				if (typer.maxVisibleCharacters > 0)
				{
					return _characterIndex < 0;
				}
				return false;
			}
			return true;
		}
	}

	private float _TimeForCharacter(TMP_CharacterInfo[] characters, int index)
	{
		char character = characters[index].character;
		float num;
		if (char.IsLetterOrDigit(character))
		{
			num = 1f;
		}
		else if (SpecialCaseTimings.ContainsKey(character))
		{
			num = SpecialCaseTimings[character];
			if (character == '.' || character == '?' || character == '!')
			{
				bool flag = false;
				for (int i = index + 1; i < characters.Length; i++)
				{
					if (flag = char.IsLetterOrDigit(characters[i].character))
					{
						break;
					}
				}
				if (!flag)
				{
					num = 1f;
				}
				else if ((index > 0 && characters[index - 1].character == character) || (index < characters.Length - 1 && characters[index + 1].character == character))
				{
					num = 4f;
				}
			}
		}
		else
		{
			num = _nonAlphaNumericTiming;
		}
		return num / charactersPerSecond;
	}

	private void _DoSyncWithLogic(TMP_CharacterInfo[] charInfo, int characterCount)
	{
		if (getAudioSource == null)
		{
			return;
		}
		_audioSource = (_audioSource ? _audioSource : getAudioSource());
		if (syncCompletionWithAudioSource && (bool)_audioSource && _audioSource.isPlaying)
		{
			float num = _elapsedTime;
			for (int i = _characterIndex; i < characterCount; i++)
			{
				num += _TimeForCharacter(charInfo, i);
			}
			charactersPerSecond *= Mathf.Max(0.0001f, num) / Mathf.Max(0.0001f, _audioSource.TimeRemaining() + _durationAdjustment);
			getAudioSource = null;
		}
	}

	private void _DoSoftenVolumeWhileAudioSourcePlayingLogic()
	{
		if (softenVolumeWhileAudioSourceIsPlaying && (bool)_audioSource)
		{
			volume = MathUtil.Ease(volume, _audioSource.isPlaying ? 0.5f : 1f, 3f, Time.unscaledDeltaTime);
		}
	}

	private void _OnLocaleChange(Locale locale)
	{
		GetComponent<LocalizeTMPFontEvent>()?.OnUpdateAsset.AddSingleFireListener(delegate
		{
			if ((bool)textMesh)
			{
				textMesh.RecalculateAutoFontSize();
			}
		});
	}

	private void Awake()
	{
		audioPlayer.is3D = false;
		audioPlayer.attachToTransform = false;
	}

	private void OnEnable()
	{
		if (setTextOnEnable)
		{
			SetText(textMesh.text);
		}
	}

	private void Start()
	{
		LocalizationSettings.Instance.OnSelectedLocaleChanged += _OnLocaleChange;
	}

	private void Update()
	{
		if (_characterIndex < 0)
		{
			return;
		}
		TMP_CharacterInfo[] characterInfo = textMesh.textInfo.characterInfo;
		int characterCount = textMesh.textInfo.characterCount;
		_elapsedTime -= GameUtil.GetDeltaTime(useScaledTime);
		_DoSoftenVolumeWhileAudioSourcePlayingLogic();
		while (_characterIndex < characterCount)
		{
			if (_elapsedTime >= 0f)
			{
				return;
			}
			_DoSyncWithLogic(characterInfo, characterCount);
			char character = characterInfo[_characterIndex].character;
			if (soundEnabled && (playSoundForAllCharacters || char.IsLetterOrDigit(character)) && (_characterSoundIndex = (_characterSoundIndex + 1) % _charactersPerSound) == 0 && (float)audioPlayer.maxActiveSources * soundOverlap >= (float)audioPlayer.activeSourceCount)
			{
				audioPlayer.Play();
			}
			_elapsedTime += _TimeForCharacter(characterInfo, _characterIndex);
			typer.maxVisibleCharacters = ++_characterIndex;
		}
		_characterIndex = -1;
	}

	private void OnDisable()
	{
		getAudioSource = null;
		syncCompletionWithAudioSource = (_softenVolumeWhileAudioSourceIsPlaying = false);
	}

	private void OnDestroy()
	{
		LocalizationSettings.Instance.OnSelectedLocaleChanged -= _OnLocaleChange;
	}

	public TextMeshTyper Stop(bool freezeFontSize = true)
	{
		if (textMesh.textInfo.characterCount == 0)
		{
			textMesh.ForceMeshUpdate();
		}
		_characterIndex = -1;
		typer.maxVisibleCharacters = 0;
		if (freezeFontSize)
		{
			textMesh.FreezeAutoFontSize();
		}
		return this;
	}

	public void StartTyping(Func<AudioSource> getAudioSourceToSynchWith = null, float startDelay = 0f, float durationAdjustment = 0f, bool syncWithAudio = true)
	{
		getAudioSource = getAudioSourceToSynchWith;
		syncCompletionWithAudioSource = syncWithAudio;
		_durationAdjustment = durationAdjustment;
		SetText(textMesh.text);
		_elapsedTime = (syncWithAudio ? startDelay : 0f);
		base.enabled = true;
		typer.enabled = true;
		audioPlayer.enabled = true;
		if (!syncWithAudio)
		{
			TMP_CharacterInfo[] characterInfo = textMesh.textInfo.characterInfo;
			int characterCount = textMesh.textInfo.characterCount;
			float num = _elapsedTime;
			for (int i = _characterIndex; i < characterCount; i++)
			{
				num += _TimeForCharacter(characterInfo, i);
			}
			charactersPerSecond *= num * 2f;
		}
	}

	public void SetText(string s)
	{
		_characterIndex = 0;
		_elapsedTime = 0f;
		_characterSoundIndex = _charactersPerSound - 1;
		textMesh.text = s;
		typer.maxVisibleCharacters = 0;
		typer.Play();
	}

	public void ForceRestart()
	{
		SetText(text);
	}

	public void Finish()
	{
		typer.maxVisibleCharacters = textMesh.textInfo.characterCount;
		_characterIndex = -1;
	}

	public void DisableAll()
	{
		base.enabled = false;
		typer.enabled = false;
		audioPlayer.enabled = false;
	}
}
