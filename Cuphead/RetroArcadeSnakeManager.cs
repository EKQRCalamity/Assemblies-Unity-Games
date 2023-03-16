using System.Collections;
using UnityEngine;

public class RetroArcadeSnakeManager : LevelProperties.RetroArcade.Entity
{
	[SerializeField]
	private RetroArcadeSnakeBodyPart bodyPrefab;

	private RetroArcadeSnakeBodyPart[] snakeFull;

	private const int BODYPARTS = 8;

	private const float SPACING = 60f;

	private const float OFFSCREEN_Y = 300f;

	public void StartSnake()
	{
		StartCoroutine(spawn_snake_cr());
	}

	private IEnumerator spawn_snake_cr()
	{
		AbstractPlayerController player = PlayerManager.GetNext();
		snakeFull = new RetroArcadeSnakeBodyPart[8];
		RetroArcadeSnakeBodyPart.Direction direction = RetroArcadeSnakeBodyPart.Direction.Down;
		Vector3 startPos = new Vector3(0f - player.transform.position.x, 200f);
		snakeFull[0] = bodyPrefab.Create(new Vector2(startPos.x, startPos.y), isHead: true, direction, this, snakeFull[0], base.properties.CurrentState.snake.moveSpeed);
		for (int i = 1; i < 8; i++)
		{
			snakeFull[i] = bodyPrefab.Create(new Vector2(startPos.x, startPos.y + (float)i * 60f), (i == 0) ? true : false, direction, this, (i != 0) ? snakeFull[i - 1] : snakeFull[i], base.properties.CurrentState.snake.moveSpeed);
		}
		snakeFull[0].GetPartBehind(snakeFull[1]);
		yield return null;
	}

	public void EndPhase()
	{
		StartCoroutine(end_phase_cr());
	}

	private IEnumerator end_phase_cr()
	{
		for (int j = 0; j < snakeFull.Length; j++)
		{
			snakeFull[j].Die();
		}
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		for (int i = snakeFull.Length - 1; i >= 0; i--)
		{
			Object.Destroy(snakeFull[i].gameObject);
			yield return CupheadTime.WaitForSeconds(this, 0.1f);
		}
		base.properties.DealDamageToNextNamedState();
		yield return null;
	}
}
