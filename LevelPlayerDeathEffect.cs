using UnityEngine;

public class LevelPlayerDeathEffect : Effect
{
	[SerializeField]
	private SpriteRenderer cuphead;

	[SerializeField]
	private SpriteRenderer mugman;

	[SerializeField]
	private SpriteRenderer chalice;

	[SerializeField]
	private SpriteRenderer shadow;

	public void Init(Vector2 pos, PlayerId id, bool playerGrounded)
	{
		base.transform.position = pos;
		if (id == PlayerId.PlayerOne || id != PlayerId.PlayerTwo)
		{
			if (PlayerManager.GetPlayer(id).stats.isChalice)
			{
				chalice.enabled = true;
			}
			else if (PlayerManager.player1IsMugman)
			{
				mugman.enabled = true;
			}
			else
			{
				cuphead.enabled = true;
			}
		}
		else if (PlayerManager.GetPlayer(id).stats.isChalice)
		{
			chalice.enabled = true;
		}
		else if (PlayerManager.player1IsMugman)
		{
			cuphead.enabled = true;
		}
		else
		{
			mugman.enabled = true;
		}
		if (playerGrounded)
		{
			shadow.enabled = true;
		}
	}

	public void Init(Vector2 pos)
	{
		base.transform.position = pos;
	}
}
