using UnityEngine;

public class DeferredAudioSource : MonoBehaviour
{
	[SerializeField]
	private string audioClipName;

	[SerializeField]
	private bool playOnInitialize;

	public void Initialize()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (!DLCManager.DLCEnabled() && AssetLoader<AudioClip>.IsDLCAsset(audioClipName))
		{
			component.clip = null;
		}
		else
		{
			component.clip = AssetLoader<AudioClip>.GetCachedAsset(audioClipName);
		}
		if (playOnInitialize)
		{
			component.Play();
		}
	}
}
