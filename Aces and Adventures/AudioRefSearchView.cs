using UnityEngine;

public class AudioRefSearchView : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/AudioRefSearchView";

	private static PooledAudioPlayer.ActivePooledAudioSource _ActiveAudioSource;

	public AudioClipEvent onAudioClipChange;

	public Vector2ArrayEvent onAudioVertexDataChanged;

	public StringEvent onNameChange;

	public StringEvent onLengthChange;

	public BoolEvent onIsPlayingChange;

	private bool _isPlaying;

	private PooledAudioPlayer.ActivePooledAudioSource _audioSource;

	public AudioRef audioRef { get; private set; }

	public AudioClip clip { get; private set; }

	public bool isPlaying => _isPlaying;

	public static AudioRefSearchView Create(AudioRef audioRef, Transform parent = null)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<AudioRefSearchView>()._SetData(audioRef);
	}

	private void _PlayAudioClip(AudioClip audioClip)
	{
		if ((bool)this && base.isActiveAndEnabled)
		{
			clip = audioClip;
			onAudioClipChange.Invoke(audioClip);
			int num = Mathf.RoundToInt(audioClip.length);
			onLengthChange.Invoke((num < 1) ? "<1s" : ((num < 60) ? (num + "s") : (Mathf.RoundToInt((float)num / 60f) + "m")));
			_audioSource = (_ActiveAudioSource = AudioPool.Instance.Play(clip, MasterMixManager.UI));
		}
	}

	private AudioRefSearchView _SetData(AudioRef audio)
	{
		audioRef = audio;
		if (audio.waveFormData != null)
		{
			audio.GetWaveFormData(delegate(Vector2[] a)
			{
				onAudioVertexDataChanged.InvokeIfActive(this, a);
			});
		}
		onNameChange.Invoke(audio.name);
		return this;
	}

	private void OnDisable()
	{
		if (_audioSource == _ActiveAudioSource && (bool)_ActiveAudioSource)
		{
			_ActiveAudioSource.Stop();
		}
		audioRef = null;
	}

	private void OnEnable()
	{
		onLengthChange.Invoke("");
		onIsPlayingChange.Invoke(_isPlaying = false);
	}

	private void Update()
	{
		if (SetPropertyUtility.SetStruct(ref _isPlaying, _audioSource == _ActiveAudioSource && (bool)_ActiveAudioSource))
		{
			onIsPlayingChange.Invoke(isPlaying);
		}
	}

	public void RequestEditTags()
	{
		if ((bool)audioRef && audioRef.belongsToCurrentCreator)
		{
			UIUtil.CreateContentRefTagsPopup(audioRef, base.transform);
		}
	}

	public void RequestRename()
	{
		if ((bool)audioRef && audioRef.belongsToCurrentCreator)
		{
			UIUtil.CreateContentRefRenamePopup(audioRef, base.transform, onNameChange.Invoke);
		}
	}

	public void PlayButtonClick()
	{
		_ActiveAudioSource.Stop();
		if (!isPlaying)
		{
			audioRef.GetAudioClip(_PlayAudioClip);
		}
	}
}
