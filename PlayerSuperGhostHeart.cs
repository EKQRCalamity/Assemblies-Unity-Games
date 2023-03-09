using UnityEngine;

public class PlayerSuperGhostHeart : AbstractLevelEntity
{
	private float gravityMultiplier;

	[SerializeField]
	private Effect spark;

	private void FixedUpdate()
	{
		base.transform.AddPosition(0f, WeaponProperties.LevelSuperGhost.heartSpeed * CupheadTime.FixedDelta * gravityMultiplier);
		if (!CupheadLevelCamera.Current.ContainsPoint(base.transform.position, new Vector2(50f, 50f)))
		{
			Object.Destroy(base.gameObject);
		}
	}

	public PlayerSuperGhostHeart Create(Vector2 pos, float gravityMultiplier)
	{
		PlayerSuperGhostHeart playerSuperGhostHeart = InstantiatePrefab<PlayerSuperGhostHeart>();
		playerSuperGhostHeart.transform.position = pos;
		playerSuperGhostHeart.transform.localScale = new Vector3(playerSuperGhostHeart.transform.localScale.x, gravityMultiplier, playerSuperGhostHeart.transform.localScale.z);
		playerSuperGhostHeart.gravityMultiplier = gravityMultiplier;
		return playerSuperGhostHeart;
	}

	public override void OnParry(AbstractPlayerController player)
	{
		base.OnParry(player);
		player.stats.AddEx();
		spark.Create(base.transform.position);
		Object.Destroy(base.gameObject);
	}
}
