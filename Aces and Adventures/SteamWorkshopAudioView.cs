using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SteamWorkshopAudioView : SteamWorkshopItemView
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "UI/Content/SteamWorkshopAudioView";

	private static PooledAudioPlayer.ActivePooledAudioSource _ActiveAudioSource;

	public AudioClipEvent onAudioClipChange;

	public Vector2ArrayEvent onAudioVertexDataChanged;

	public StringEvent onNameChange;

	public StringEvent onLengthChange;

	public BoolEvent onIsPlayingChange;

	private AudioClip _audioClip;

	private bool _hasBegunLoadingClip;

	private bool _isPlaying;

	private PooledAudioPlayer.ActivePooledAudioSource _audioSource;

	public bool isPlaying => _isPlaying;

	private void _PlayAudioClip(AudioClip audioClip)
	{
		if (!this.IsActiveAndEnabled())
		{
			this.DestroySafe(ref audioClip);
			return;
		}
		_audioClip = audioClip;
		onAudioClipChange.Invoke(audioClip);
		int num = Mathf.RoundToInt(audioClip.length);
		onLengthChange.Invoke((num < 1) ? "<1s" : ((num < 60) ? (num + "s") : (Mathf.RoundToInt((float)num / 60f) + "m")));
		_audioSource = (_ActiveAudioSource = AudioPool.Instance.Play(_audioClip, MasterMixManager.UI));
	}

	private void OnEnable()
	{
		onIsPlayingChange.Invoke(arg0: false);
	}

	private void OnDisable()
	{
		if (_audioSource == _ActiveAudioSource && (bool)_ActiveAudioSource)
		{
			_ActiveAudioSource.Stop();
		}
		this.DestroySafe(ref _audioClip);
	}

	private void Update()
	{
		if (SetPropertyUtility.SetStruct(ref _isPlaying, _audioSource == _ActiveAudioSource && (bool)_ActiveAudioSource))
		{
			onIsPlayingChange.Invoke(isPlaying);
		}
	}

	protected override void _OnResultSetUnique()
	{
		onNameChange.Invoke(base.result.name);
		if (!base.result.metaData.IsNullOrEmpty())
		{
			onAudioVertexDataChanged.Invoke(AudioUtil.CreateAudioVertexDataFromBytes(base.result.metaData));
		}
	}

	public void PlayButtonClick()
	{
		_ActiveAudioSource.Stop();
		if (isPlaying)
		{
			return;
		}
		if (!_hasBegunLoadingClip && (_hasBegunLoadingClip = true))
		{
			Job.Process(Steam.Ugc.DownloadAsync(base.result).AsEnumerator()).If((Steam.Ugc.InstallInfo i) => (bool)i && this.IsActiveAndEnabled()).Immediately()
				.ResultProcess((Steam.Ugc.InstallInfo i) => UnityWebRequestMultimedia.GetAudioClip(IOUtil.ToURI(Path.Combine(i.filepath, "c.ogg")), AudioType.OGGVORBIS).GetAudioClipAsync().AsEnumerator())
				.Immediately()
				.ResultAction<AudioClip>(_PlayAudioClip);
		}
		else if ((bool)_audioClip)
		{
			_PlayAudioClip(_audioClip);
		}
	}
}
