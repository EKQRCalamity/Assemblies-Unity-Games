using UnityEngine;

public class ChessKingLevelGroundTrigger : AbstractCollidableObject
{
	private bool checkingPlayer;

	public bool PLAYER_FALLEN { get; private set; }

	public void CheckPlayer(bool checkPlayer)
	{
		checkingPlayer = checkPlayer;
		PLAYER_FALLEN = false;
	}

	private void Update()
	{
		if (checkingPlayer)
		{
			if (PlayerManager.GetPlayer(PlayerId.PlayerOne).transform.position.y < base.transform.position.y)
			{
				PLAYER_FALLEN = true;
			}
			else
			{
				PLAYER_FALLEN = false;
			}
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawLine(new Vector3(-800f, base.transform.position.y), new Vector3(800f, base.transform.position.y));
	}
}
