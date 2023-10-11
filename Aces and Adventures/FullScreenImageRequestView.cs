using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[RequireComponent(typeof(CanvasInputFocus))]
public class FullScreenImageRequestView : MonoBehaviour
{
	private static PooledAudioPlayer.ActivePooledAudioSource _ActiveAudioSource;

	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/FullScreenImageRequestView";

	public RectTransform imageContainer;

	public Texture2DEvent onImageChange;

	public UnityEvent onClose;

	private bool _closing;

	private CanvasInputFocus _canvasFocus;

	private bool _wasPixelPerfect;

	private Canvas _canvas;

	private AudioClip _audioClip;

	private PooledAudioPlayer.ActivePooledAudioSource _audioSource;

	private CanvasInputFocus canvasFocus => this.CacheComponent(ref _canvasFocus);

	private Canvas canvas => this.CacheComponentInParent(ref _canvas);

	public static FullScreenImageRequestView Create(string url, Transform parent, ulong audioPublishedFileId = 0uL)
	{
		return DirtyPools.Unpool(_Blueprint, parent).GetComponent<FullScreenImageRequestView>()._SetData(url, audioPublishedFileId);
	}

	private void _PlayAudioClip(AudioClip audioClip)
	{
		if (!this.IsActiveAndEnabled())
		{
			this.DestroySafe(ref audioClip);
			return;
		}
		_audioClip = audioClip;
		_audioSource = (_ActiveAudioSource = AudioPool.Instance.Play(_audioClip, MasterMixManager.UI));
	}

	private FullScreenImageRequestView _SetData(string url, ulong audioPublishedFileId)
	{
		_closing = false;
		if (audioPublishedFileId != 0)
		{
			Job.Process(Steam.Ugc.DownloadAsync((PublishedFileId_t)audioPublishedFileId).AsEnumerator()).If((Steam.Ugc.InstallInfo i) => (bool)i && this.IsActiveAndEnabled()).Immediately()
				.ResultProcess((Steam.Ugc.InstallInfo i) => UnityWebRequestMultimedia.GetAudioClip(IOUtil.ToURI(Path.Combine(i.filepath, "c.ogg")), AudioType.OGGVORBIS).GetAudioClipAsync().AsEnumerator())
				.Immediately()
				.ResultAction<AudioClip>(_PlayAudioClip);
		}
		Job.Process(WebRequestTextureCache.RequestAsync(this, url).AsEnumerator()).Immediately().ResultAction(delegate(Texture2D t)
		{
			if (!_closing && this.IsActiveAndEnabled())
			{
				onImageChange.Invoke(t);
				float num = 1f / GetComponentInParent<Canvas>().scaleFactor;
				imageContainer.localScale = Vector3.one * Mathf.Clamp(num, 0.0001f, Mathf.Min((float)Screen.width * num / (float)t.width, (float)Screen.height * num / (float)t.height));
			}
		});
		return this;
	}

	private void OnEnable()
	{
		InputManager.RequestInput(this);
	}

	private void Start()
	{
		_wasPixelPerfect = canvas.pixelPerfect;
		canvas.pixelPerfect = true;
	}

	private void Update()
	{
		if (!_closing && canvasFocus.HasFocus() && (InputManager.I[this][KeyAction.Back][KState.Clicked] || InputManager.I[this][KeyAction.Pause][KState.Clicked] || InputManager.I[KeyCode.Escape][KState.Clicked]))
		{
			Close();
		}
	}

	private void OnDisable()
	{
		InputManager.ReleaseInput(this);
		this.DestroySafe(ref _audioClip);
		if (_audioSource == _ActiveAudioSource && (bool)_ActiveAudioSource)
		{
			_ActiveAudioSource.Stop();
		}
	}

	public void Close()
	{
		if (!_closing)
		{
			canvas.pixelPerfect = _wasPixelPerfect;
			_closing = true;
			onClose.Invoke();
		}
	}
}
