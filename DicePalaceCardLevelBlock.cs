using UnityEngine;

public class DicePalaceCardLevelBlock : LevelPlatform
{
	public enum Suit
	{
		Hearts = 1,
		Spades,
		Clubs,
		Diamonds
	}

	public Suit suit;

	public int stopOffsetX;

	public DicePalaceCardLevelGridBlock[,] gridBlock;

	private float YCheck;

	private DamageDealer damageDealer;

	public override void AddChild(Transform player)
	{
	}

	public void DestroyBlock()
	{
		Object.Destroy(base.gameObject);
	}
}
