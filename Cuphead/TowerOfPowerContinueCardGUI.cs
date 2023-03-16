using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class TowerOfPowerContinueCardGUI : AbstractMonoBehaviour
{
	[SerializeField]
	private SpriteRenderer PlayerName;

	[SerializeField]
	private Sprite CupheadNameData;

	[SerializeField]
	private Sprite CupmanNameData;

	[Space(10f)]
	[SerializeField]
	private Text TokenLeft_text;

	[SerializeField]
	private Text Continue;

	[SerializeField]
	private Text CountDown_text;

	[SerializeField]
	private List<UIImageAnimationLoop> CardCupheadAnimation = new List<UIImageAnimationLoop>();

	[SerializeField]
	private List<UIImageAnimationLoop> CardMugmanAnimation = new List<UIImageAnimationLoop>();

	[SerializeField]
	private CanvasGroup canvas;

	private bool yourPlayerIsMugman;

	private int countDown = 10;

	private PlayersStatsBossesHub player;

	protected override void Awake()
	{
		base.Awake();
		foreach (UIImageAnimationLoop item in CardMugmanAnimation)
		{
			item.gameObject.SetActive(value: false);
		}
		foreach (UIImageAnimationLoop item2 in CardCupheadAnimation)
		{
			item2.gameObject.SetActive(value: false);
		}
	}

	public void Init(PlayerId playerId)
	{
		canvas.alpha = 0f;
		SetPlayer(playerId);
		player = TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)playerId];
		SetTitlePlayersName();
		SetTokenCount();
		Continue.gameObject.SetActive(player.HP == 0);
		CountDown_text.gameObject.SetActive(player.HP == 0);
		SetAnimation();
		if (player.HP == 0)
		{
			StartCoroutine(update_countdown_cr());
		}
	}

	private void SetAnimation()
	{
		if (yourPlayerIsMugman)
		{
			CardMugmanAnimation[(player.HP == 0) ? 1 : 0].gameObject.SetActive(value: true);
		}
		else
		{
			CardCupheadAnimation[(player.HP == 0) ? 1 : 0].gameObject.SetActive(value: true);
		}
	}

	private void SetPlayer(PlayerId playerId)
	{
		if (playerId == PlayerId.PlayerOne)
		{
			yourPlayerIsMugman = PlayerManager.player1IsMugman;
		}
		else
		{
			yourPlayerIsMugman = !PlayerManager.player1IsMugman;
		}
	}

	private void SetTitlePlayersName()
	{
		if (yourPlayerIsMugman)
		{
			string text = "Mugman";
		}
		else
		{
			string text = "Cuphead";
		}
	}

	public void SetTokenCount()
	{
		int tokenCount = player.tokenCount;
		TokenLeft_text.text = "Token: " + tokenCount;
	}

	public IEnumerator update_countdown_cr()
	{
		while (countDown > 0)
		{
			yield return CupheadTime.WaitForSeconds(this, 1f);
			countDown--;
			UpdateCountDownText();
		}
		yield return null;
		SceneLoader.ContinueTowerOfPower();
	}

	private void UpdateCountDownText()
	{
		CountDown_text.text = countDown.ToString();
	}

	private void OnDestroy()
	{
		Continue = null;
		CountDown_text = null;
		CardCupheadAnimation.Clear();
		CardMugmanAnimation.Clear();
	}

	public void SetAlpha(float value)
	{
		canvas.alpha = value;
	}
}
