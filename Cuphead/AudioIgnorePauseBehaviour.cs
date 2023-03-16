using UnityEngine;

public class AudioIgnorePauseBehaviour : AbstractMonoBehaviour
{
	private AudioSource audioSource;

	protected override void Awake()
	{
		base.Awake();
		audioSource = GetComponent<AudioSource>();
		if (audioSource != null)
		{
			audioSource.ignoreListenerPause = true;
		}
	}
}
