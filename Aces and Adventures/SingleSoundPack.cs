using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/SingleSoundPack")]
public class SingleSoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	[SerializeField]
	protected PooledAudioPack _sounds;

	public PooledAudioPack sounds => _sounds ?? (_sounds = new PooledAudioPack());
}
