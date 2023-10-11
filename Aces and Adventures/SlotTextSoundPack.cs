using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/SlotTextSoundPack")]
public class SlotTextSoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	public PooledAudioPack onBeginAnimation;

	public PooledAudioPack onTick;

	public PooledAudioPack onEndAnimation;
}
