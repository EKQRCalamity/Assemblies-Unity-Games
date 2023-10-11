using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/UILayout3DSoundPack")]
public class UILayout3DSoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	[SerializeField]
	protected PooledAudioPack _onRest;

	[SerializeField]
	protected PooledAudioPack _onPointerOver;

	[SerializeField]
	protected PooledAudioPack _onPointerDown;

	[SerializeField]
	protected PooledAudioPack _onPointerClick;

	[SerializeField]
	protected PooledAudioPack _onBeginDrag;

	public PooledAudioPack onRest => _onRest ?? (_onRest = new PooledAudioPack());

	public PooledAudioPack onPointerOver => _onPointerOver ?? (_onPointerOver = new PooledAudioPack());

	public PooledAudioPack onPointerDown => _onPointerDown ?? (_onPointerDown = new PooledAudioPack());

	public PooledAudioPack onPointerClick => _onPointerClick ?? (_onPointerClick = new PooledAudioPack());

	public PooledAudioPack onBeginDrag => _onBeginDrag ?? (_onBeginDrag = new PooledAudioPack());
}
