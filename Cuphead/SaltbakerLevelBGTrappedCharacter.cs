using UnityEngine;

public class SaltbakerLevelBGTrappedCharacter : MonoBehaviour
{
	private enum Character
	{
		None = -1,
		Cuphead,
		Mugman,
		Chalice
	}

	[SerializeField]
	private GameObject[] characters;

	private Character charID = Character.None;

	private Character pOneID = Character.None;

	private Character pTwoID = Character.None;

	public void Setup()
	{
		if (PlayerManager.Multiplayer)
		{
			if (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.isChalice || PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.isChalice)
			{
				if (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.isChalice)
				{
					charID = (PlayerManager.player1IsMugman ? Character.Mugman : Character.Cuphead);
				}
				else
				{
					charID = ((!PlayerManager.player1IsMugman) ? Character.Mugman : Character.Cuphead);
				}
			}
			else
			{
				charID = Character.Chalice;
			}
		}
		else if (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.isChalice)
		{
			charID = (PlayerManager.player1IsMugman ? Character.Mugman : Character.Cuphead);
		}
		else
		{
			charID = ((PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo).charm != Charm.charm_chalice) ? Character.Chalice : ((!PlayerManager.player1IsMugman) ? Character.Mugman : Character.Cuphead));
		}
		for (int i = 0; i < 3; i++)
		{
			characters[i].SetActive(i == (int)charID);
		}
	}
}
