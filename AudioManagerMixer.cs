using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManagerMixer : MonoBehaviour
{
	[Serializable]
	public class Groups
	{
		[SerializeField]
		private AudioMixerGroup _master;

		[SerializeField]
		private AudioMixerGroup _master_Options;

		[SerializeField]
		private AudioMixerGroup _bgm_Options;

		[SerializeField]
		private AudioMixerGroup _sfx_Options;

		[Space(10f)]
		[Header("BGM")]
		[SerializeField]
		private AudioMixerGroup _bgm;

		[SerializeField]
		private AudioMixerGroup _levelBgm;

		[SerializeField]
		private AudioMixerGroup _musicSting;

		[Space(10f)]
		[Header("SFX")]
		[SerializeField]
		private AudioMixerGroup _sfx;

		[SerializeField]
		private AudioMixerGroup _levelSfx;

		[SerializeField]
		private AudioMixerGroup _ambience;

		[SerializeField]
		private AudioMixerGroup _creatures;

		[SerializeField]
		private AudioMixerGroup _announcer;

		[SerializeField]
		private AudioMixerGroup _super;

		[Space(10f)]
		[Header("Noise")]
		[SerializeField]
		private AudioMixerGroup _noise;

		[SerializeField]
		private AudioMixerGroup _noiseConstant;

		[SerializeField]
		private AudioMixerGroup _noiseShortterm;

		[SerializeField]
		private AudioMixerGroup _noise1920s;

		public AudioMixerGroup master => _master;

		public AudioMixerGroup master_Options => _master_Options;

		public AudioMixerGroup bgm_Options => _bgm_Options;

		public AudioMixerGroup sfx_Options => _sfx_Options;

		public AudioMixerGroup bgm => _bgm;

		public AudioMixerGroup levelBgm => _levelBgm;

		public AudioMixerGroup musicSting => _musicSting;

		public AudioMixerGroup sfx => _sfx;

		public AudioMixerGroup levelSfx => _levelSfx;

		public AudioMixerGroup ambience => _ambience;

		public AudioMixerGroup creatures => _creatures;

		public AudioMixerGroup announcer => _announcer;

		public AudioMixerGroup super => _super;

		public AudioMixerGroup noise => _noise;

		public AudioMixerGroup noiseConstant => _noiseConstant;

		public AudioMixerGroup noiseShortterm => _noiseShortterm;

		public AudioMixerGroup noise1920s => _noise1920s;
	}

	private const string PATH = "Audio/AudioMixer";

	private static AudioManagerMixer Manager;

	[SerializeField]
	private AudioMixer mixer;

	[SerializeField]
	private Groups audioGroups;

	private static void Init()
	{
		if (Manager == null)
		{
			Manager = Resources.Load<AudioManagerMixer>("Audio/AudioMixer");
		}
	}

	public static AudioMixer GetMixer()
	{
		Init();
		return Manager.mixer;
	}

	public static Groups GetGroups()
	{
		Init();
		return Manager.audioGroups;
	}
}
