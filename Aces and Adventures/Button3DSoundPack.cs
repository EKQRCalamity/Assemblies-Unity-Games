using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/Button3DSoundPack")]
public class Button3DSoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	[SerializeField]
	protected PooledAudioPack _onClick;

	[SerializeField]
	protected PooledAudioPack _onDown;

	[SerializeField]
	protected PooledAudioPack _onUp;

	[SerializeField]
	protected PooledAudioPack _onEnter;

	[SerializeField]
	protected PooledAudioPack _onExit;

	[SerializeField]
	protected PooledAudioPack _onClickFinished;

	public PooledAudioPack onClick => _onClick ?? (_onClick = new PooledAudioPack());

	public PooledAudioPack onDown => _onDown ?? (_onDown = new PooledAudioPack());

	public PooledAudioPack onUp => _onUp ?? (_onUp = new PooledAudioPack());

	public PooledAudioPack onEnter => _onEnter ?? (_onEnter = new PooledAudioPack());

	public PooledAudioPack onExit => _onExit ?? (_onExit = new PooledAudioPack());

	public PooledAudioPack onClickFinished => _onClickFinished ?? (_onClickFinished = new PooledAudioPack());
}
