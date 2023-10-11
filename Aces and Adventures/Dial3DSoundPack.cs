using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/Dial3DSoundPack")]
public class Dial3DSoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	public PooledAudioPack onPointerEnter;

	public PooledAudioPack onClick;

	public PooledAudioPack onBeginDrag;

	public PooledAudioPack onEndDrag;

	public PooledAudioPack onTick;

	public PooledAudioPack onInvalidTick;
}
