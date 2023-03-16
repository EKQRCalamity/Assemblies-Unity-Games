using System.Collections;

public class DevilLevelHole : AbstractCollidableObject
{
	public static bool PHASE_1_COMPLETE { get; private set; }

	private void Start()
	{
		PHASE_1_COMPLETE = false;
		StartCoroutine(check_player_cr());
	}

	private IEnumerator check_player_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		while (true)
		{
			if (PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null && !PlayerManager.GetPlayer(PlayerId.PlayerTwo).IsDead)
			{
				if (PlayerManager.GetPlayer(PlayerId.PlayerOne).IsDead)
				{
					if (PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform.position.y < base.transform.position.y)
					{
						PHASE_1_COMPLETE = true;
						yield break;
					}
				}
				else if (PlayerManager.GetPlayer(PlayerId.PlayerTwo).transform.position.y < base.transform.position.y && PlayerManager.GetPlayer(PlayerId.PlayerOne).transform.position.y < base.transform.position.y)
				{
					PHASE_1_COMPLETE = true;
					yield break;
				}
			}
			else if (PlayerManager.GetPlayer(PlayerId.PlayerOne).transform.position.y < base.transform.position.y)
			{
				break;
			}
			yield return null;
		}
		PHASE_1_COMPLETE = true;
	}
}
