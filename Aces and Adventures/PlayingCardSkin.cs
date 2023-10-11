using UnityEngine;

public class PlayingCardSkin : MonoBehaviour
{
	public Material cardBack;

	public Material transparent;

	public Material joker;

	[SerializeField]
	private Material[] _materials;

	public Material this[PlayingCardType card] => _materials[(int)(card - 2)];

	public Material GetMaterial(PlayingCardType card)
	{
		return this[card];
	}
}
