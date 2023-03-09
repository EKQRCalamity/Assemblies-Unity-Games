using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LevelReviveCardGUI : AbstractMonoBehaviour
{
	private enum State
	{
		Init,
		Ready,
		Exiting
	}

	public static LevelReviveCardGUI Current;

	[Space(10f)]
	[SerializeField]
	private TowerOfPowerContinueCardGUI playerOneCard;

	[Space(10f)]
	[SerializeField]
	private TowerOfPowerContinueCardGUI playerTwoCard;

	[Space(10f)]
	[SerializeField]
	private CanvasGroup helpCanvasGroup;

	private State state;

	private CupheadInput.AnyPlayerInput input;

	private CanvasGroup canvasGroup;

	private PlayerId deadPlayer;

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		canvasGroup = GetComponent<CanvasGroup>();
		base.gameObject.SetActive(value: false);
		input = new CupheadInput.AnyPlayerInput();
		helpCanvasGroup.alpha = 0f;
		ignoreGlobalTime = true;
		timeLayer = CupheadTime.Layer.UI;
		state = State.Init;
	}

	private void OnDestroy()
	{
		Current = null;
	}

	private void Update()
	{
		if (state == State.Ready && PlayerManager.GetPlayerInput(deadPlayer).GetButtonDown(13))
		{
			RevivePlayer();
			state = State.Exiting;
		}
	}

	private void RevivePlayer()
	{
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)deadPlayer].HP = 3;
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)deadPlayer].BonusHP = 0;
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)deadPlayer].SuperCharge = 0f;
		TowerOfPowerLevelGameInfo.PLAYER_STATS[(int)deadPlayer].tokenCount--;
		playerOneCard.SetTokenCount();
		playerTwoCard.SetTokenCount();
		SceneLoader.ContinueTowerOfPower();
	}

	public void In()
	{
		base.gameObject.SetActive(value: true);
		playerOneCard.Init(PlayerId.PlayerOne);
		playerTwoCard.Init(PlayerId.PlayerTwo);
		if (TowerOfPowerLevelGameInfo.PLAYER_STATS[0].HP == 0)
		{
			deadPlayer = PlayerId.PlayerOne;
		}
		else
		{
			deadPlayer = PlayerId.PlayerTwo;
		}
		StartCoroutine(in_cr());
	}

	private IEnumerator in_cr()
	{
		AudioManager.Play("level_menu_card_up");
		yield return TweenValue(0f, 1f, 0.05f, EaseUtils.EaseType.linear, SetAlpha);
		yield return new WaitForSeconds(1f);
		AudioManager.Play("player_die_vinylscratch");
		AudioManager.HandleSnapshot(AudioManager.Snapshots.Death.ToString(), 4f);
		if (!Level.IsChessBoss)
		{
			AudioManager.ChangeBGMPitch(0.7f, 6f);
		}
		TweenValue(0f, 1f, 0.3f, EaseUtils.EaseType.easeOutCubic, SetCardValue);
		yield return null;
		state = State.Ready;
	}

	private void SetAlpha(float value)
	{
		canvasGroup.alpha = value;
	}

	private void SetCardValue(float value)
	{
		playerOneCard.SetAlpha(value);
		playerTwoCard.SetAlpha(value);
		helpCanvasGroup.alpha = value;
		playerOneCard.transform.SetLocalEulerAngles(null, null, Mathf.Lerp(15f, 4f, value));
		playerTwoCard.transform.SetLocalEulerAngles(null, null, Mathf.Lerp(-15f, -4f, value));
	}
}
