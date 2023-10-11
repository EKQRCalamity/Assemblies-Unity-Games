using UnityEngine;

[RequireComponent(typeof(PooledAudioPlayer))]
public class ToggleSoundPackPlayer : MonoBehaviour
{
	[SerializeField]
	private ToggleAnimator3DSoundPack _soundPack;

	[SerializeField]
	private bool _isOn;

	private PooledAudioPlayer _audioPlayer;

	private ToggleAnimator3DSoundPack soundPack => this.CacheScriptObject(ref _soundPack);

	private PooledAudioPlayer audioPlayer => this.CacheComponent(ref _audioPlayer);

	public bool isOn
	{
		get
		{
			return _isOn;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _isOn, value))
			{
				_OnIsOnChange();
			}
		}
	}

	private void _OnIsOnChange()
	{
		audioPlayer.Play(soundPack.GetAudioPack(_isOn));
	}
}
