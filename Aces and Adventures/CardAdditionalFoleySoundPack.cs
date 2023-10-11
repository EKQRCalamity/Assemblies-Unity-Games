using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/SoundPack/CardAdditionalFoleySoundPack")]
public class CardAdditionalFoleySoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	[Header("Decks")]
	public PooledAudioPack openDeck;

	public PooledAudioPack closeDeck;
}
