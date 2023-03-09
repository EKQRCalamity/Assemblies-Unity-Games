using System;
using UnityEngine;

public abstract class AbstractEquipUI : AbstractPauseGUI
{
	public enum ActiveState
	{
		Inactive,
		Active
	}

	[SerializeField]
	private MapEquipUICard playerOne;

	private MapEquipUICard playerTwo;

	public static AbstractEquipUI Current { get; private set; }

	public ActiveState CurrentState { get; private set; }

	protected override InputActionSet CheckedActionSet => InputActionSet.UIInput;

	protected override bool CanPause => false;

	protected override bool CanUnpause => false;

	protected override void InAnimation(float i)
	{
	}

	protected override void OutAnimation(float i)
	{
	}

	protected override void Awake()
	{
		base.Awake();
		Current = this;
		playerTwo = UnityEngine.Object.Instantiate(playerOne);
		playerTwo.transform.SetParent(playerOne.transform.parent, worldPositionStays: false);
		playerTwo.Init(PlayerId.PlayerTwo, this);
		playerTwo.name = "PlayerTwo";
		playerOne.transform.SetSiblingIndex(playerTwo.transform.GetSiblingIndex());
		playerOne.Init(PlayerId.PlayerOne, this);
	}

	private void Start()
	{
		PlayerManager.OnPlayerJoinedEvent += OnPlayerJoined;
		PlayerManager.OnPlayerLeaveEvent += OnPlayerLeft;
		if (PlayerManager.Multiplayer)
		{
			Vector2 anchoredPosition = playerOne.container.anchoredPosition;
			playerOne.container.anchoredPosition = anchoredPosition;
			playerTwo.container.anchoredPosition = anchoredPosition;
		}
	}

	private void OnDestroy()
	{
		PlayerManager.OnPlayerJoinedEvent -= OnPlayerJoined;
		PlayerManager.OnPlayerLeaveEvent -= OnPlayerLeft;
		if (Current == this)
		{
			Current = null;
		}
	}

	private void OnPlayerJoined(PlayerId playerId)
	{
		Vector2 anchoredPosition = playerOne.container.anchoredPosition;
		anchoredPosition.y += 10f;
		playerOne.container.anchoredPosition = anchoredPosition;
		playerTwo.container.anchoredPosition = anchoredPosition;
	}

	private void OnPlayerLeft(PlayerId playerId)
	{
		Vector2 anchoredPosition = playerOne.container.anchoredPosition;
		anchoredPosition.y -= 10f;
		playerOne.container.anchoredPosition = anchoredPosition;
		playerTwo.container.anchoredPosition = anchoredPosition;
	}

	protected override void OnPause()
	{
		if (PlatformHelper.GarbageCollectOnPause)
		{
			GC.Collect();
		}
		OnPauseAudio();
		FrameDelayedCallback(SetStateActive, 1);
		playerOne.CanRotate = false;
		playerTwo.CanRotate = false;
		AudioManager.Play("menu_cardup");
		if (PlayerManager.Multiplayer)
		{
			playerOne.SetActive(active: true);
			playerTwo.SetActive(active: true);
			playerOne.SetMultiplayerOut(instant: true);
			playerTwo.SetMultiplayerOut(instant: true);
			playerOne.SetMultiplayerIn();
			playerTwo.SetMultiplayerIn();
		}
		else
		{
			playerOne.SetActive(active: true);
			playerTwo.SetActive(active: false);
			playerOne.SetSinglePlayerOut(instant: true);
			playerOne.SetSinglePlayerIn();
		}
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		PlayerData.Data.ResetHasNewPurchase(PlayerId.Any);
	}

	private void SetStateActive()
	{
		CurrentState = ActiveState.Active;
	}

	protected override void OnPauseComplete()
	{
		base.OnPauseComplete();
		playerOne.CanRotate = true;
		playerTwo.CanRotate = true;
	}

	protected override void OnUnpause()
	{
		OnUnpauseAudio();
		base.OnUnpause();
		playerOne.CanRotate = false;
		playerTwo.CanRotate = false;
		if (PlayerManager.Multiplayer)
		{
			playerOne.SetMultiplayerOut();
			playerTwo.SetMultiplayerOut();
		}
		else
		{
			playerOne.SetSinglePlayerOut();
		}
	}

	protected virtual void OnPauseAudio()
	{
	}

	protected virtual void OnUnpauseAudio()
	{
	}

	protected override void OnUnpauseComplete()
	{
		base.OnUnpauseComplete();
		CurrentState = ActiveState.Inactive;
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
	}

	public bool Close()
	{
		if (PlayerManager.Multiplayer && !playerOne.ReadyAndWaiting && !playerTwo.ReadyAndWaiting)
		{
			return false;
		}
		if (Map.Current != null)
		{
			Map.Current.OnCloseEquipMenu();
		}
		AudioManager.Play("menu_carddown");
		Unpause();
		return true;
	}
}
