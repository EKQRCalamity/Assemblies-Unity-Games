using UnityEngine;
using UnityEngine.Audio;

public class MasterMixManager : MonoBehaviour
{
	private static MasterMixManager _Instance;

	private static readonly ResourceBlueprint<GameObject> _ResourceController = "Audio/MasterMixData";

	private MasterMixController _controller;

	public static MasterMixManager Instance => ManagerUtil.GetSingletonInstance(ref _Instance);

	public static bool IsValid => _Instance;

	public static AudioMixerGroup SoundEffects => Instance.controller.SoundEffects;

	public static AudioMixerGroup Ambient => Instance.controller.AmbientTracks;

	public static AudioMixerGroup UI => Instance.controller.UI;

	public static AudioMixerGroup Narration => Instance.controller.Narration;

	public static MasterMixController ResourceController => _ResourceController?.value?.GetComponent<MasterMixController>();

	public MasterMixController controller => _controller;

	private void Awake()
	{
		_controller = Object.Instantiate(Resources.Load<GameObject>("Audio/MasterMixData").GetComponent<MasterMixController>());
		_controller.transform.parent = base.transform;
	}

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		ProfileOptions.AudioOptions audio = ProfileManager.Profile.options.audio;
		_controller.soundEffectSpeed = Time.timeScale;
		AudioListener.volume = audio.masterVolume * _controller.masterVolume.valueMultiplier;
		_controller.soundEffectVolume.value = AudioUtil.LoudnessToDB(audio.soundEffectVolume);
		_controller.musicVolume.value = AudioUtil.LoudnessToDB(audio.musicVolume);
		_controller.ambientTrackVolume.value = AudioUtil.LoudnessToDB(audio.ambientVolume);
		_controller.uiVolume.value = AudioUtil.LoudnessToDB(audio.uiVolume);
		_controller.narrationVolume.value = AudioUtil.LoudnessToDB(audio.narrationVolume);
	}
}
