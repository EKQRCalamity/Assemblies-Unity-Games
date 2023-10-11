using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/ToggleAnimator3DSoundPack")]
public class ToggleAnimator3DSoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	[SerializeField]
	protected PooledAudioPack _onSound;

	[SerializeField]
	protected PooledAudioPack _offSound;

	public PooledAudioPack onSound => _onSound ?? (_onSound = new PooledAudioPack());

	public PooledAudioPack offSound => _offSound ?? (_offSound = new PooledAudioPack());

	public PooledAudioPack GetAudioPack(bool isOn)
	{
		if (!isOn)
		{
			return offSound;
		}
		return onSound;
	}
}
