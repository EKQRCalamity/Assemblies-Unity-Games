using System;
using UnityEngine;
using UnityEngine.Audio;

public class MasterMixController : MonoBehaviour
{
	[Header("Mixers")]
	public AudioMixer Master;

	public AudioMixer WorldMixer;

	[Header("Audio Mixer Groups")]
	public AudioMixerGroup Music;

	public AudioMixerGroup AmbientTracks;

	public AudioMixerGroup UI;

	public AudioMixerGroup Narration;

	[Header("World Groups")]
	public AudioMixerGroup World;

	public AudioMixerGroup SoundEffects;

	[NonSerialized]
	public AudioMixerParameter masterVolume;

	[NonSerialized]
	public AudioMixerParameter worldVolume;

	[NonSerialized]
	public AudioMixerParameter musicVolume;

	[NonSerialized]
	public AudioMixerParameter musicDuckRatio;

	[NonSerialized]
	public AudioMixerParameter uiVolume;

	[NonSerialized]
	public AudioMixerParameter soundEffectVolume;

	[NonSerialized]
	public AudioMixerParameter ambientTrackVolume;

	[NonSerialized]
	public AudioMixerParameter narrationVolume;

	private AudioMixerParameter _soundEffectPitch;

	private AudioMixerParameter _soundEffectPitchShift;

	private AudioMixerParameter _soundEffectPitchShiftWet;

	private AudioMixerParameter _soundEffectPitchShiftOverlap;

	private AudioMixerParameter _soundEffectPitchShiftFFT;

	public float soundEffectSpeed
	{
		set
		{
			float num = Mathf.Max(0f, value).InsureNonZero();
			_soundEffectPitch.value = num;
			_soundEffectPitchShift.value = 1f / num;
			int num2 = ((num != 1f) ? 1 : 0);
			_soundEffectPitchShiftWet.valueNormalized = num2;
			_soundEffectPitchShiftOverlap.valueNormalized = num2;
			_soundEffectPitchShiftFFT.valueNormalized = num2;
			soundEffectVolume.value = AudioUtil.LoudnessToDB(ProfileManager.Profile.options.audio.soundEffectVolume * soundEffectVolume.valueMultiplier);
		}
	}

	protected void Awake()
	{
		masterVolume = new AudioMixerParameter(Master, "MasterVolume");
		worldVolume = new AudioMixerParameter(Master, "WorldVolume");
		musicVolume = new AudioMixerParameter(Master, "MusicVolume");
		musicDuckRatio = new AudioMixerParameter(Master, "MusicDuckRatio", 1f, 3f);
		ambientTrackVolume = new AudioMixerParameter(Master, "AmbientTrackVolume");
		uiVolume = new AudioMixerParameter(Master, "UIVolume");
		narrationVolume = new AudioMixerParameter(Master, "NarrationVolume");
		soundEffectVolume = new AudioMixerParameter(WorldMixer, "SoundEffectVolume");
		_soundEffectPitch = new AudioMixerParameter(WorldMixer, "SoundEffectPitch", 0f, 10f);
		_soundEffectPitchShift = new AudioMixerParameter(WorldMixer, "SoundEffectPitchShift", 0.5f, 2f);
		_soundEffectPitchShiftWet = new AudioMixerParameter(WorldMixer, "SoundEffectPitchShiftWet");
		_soundEffectPitchShiftOverlap = new AudioMixerParameter(WorldMixer, "SoundEffectPitchShiftOverlap", 1f, 8f);
		_soundEffectPitchShiftFFT = new AudioMixerParameter(WorldMixer, "SoundEffectPitchShiftFFT", 256f, 1024f);
	}
}
