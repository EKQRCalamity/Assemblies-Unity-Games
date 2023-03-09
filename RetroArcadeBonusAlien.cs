using UnityEngine;

public class RetroArcadeBonusAlien : RetroArcadeEnemy
{
	public enum Direction
	{
		Left,
		Right
	}

	private const float SPAWN_X = 400f;

	private const float SPAWN_Y = 270f;

	private LevelProperties.RetroArcade.Aliens properties;

	private Direction direction;

	public RetroArcadeBonusAlien Create(Direction direction, LevelProperties.RetroArcade.Aliens properties)
	{
		RetroArcadeBonusAlien retroArcadeBonusAlien = InstantiatePrefab<RetroArcadeBonusAlien>();
		retroArcadeBonusAlien.transform.position = new Vector2((direction != 0) ? (-400f) : 400f, 270f);
		retroArcadeBonusAlien.properties = properties;
		retroArcadeBonusAlien.direction = direction;
		retroArcadeBonusAlien.hp = 1f;
		return retroArcadeBonusAlien;
	}

	protected override void FixedUpdate()
	{
		base.transform.AddPosition((float)((direction == Direction.Right) ? 1 : (-1)) * properties.bonusMoveSpeed * CupheadTime.FixedDelta);
		if ((direction != 0) ? (base.transform.position.x > 400f) : (base.transform.position.x < -400f))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
