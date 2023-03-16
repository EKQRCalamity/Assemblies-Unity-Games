using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class AbstractPauseGUI : AbstractMonoBehaviour
{
	public enum State
	{
		Unpaused,
		Paused,
		Animating
	}

	public enum InputActionSet
	{
		LevelInput,
		UIInput
	}

	private delegate void AnimationDelegate(float i);

	[SerializeField]
	private bool isWorldMap;

	protected CanvasGroup canvasGroup;

	private CupheadInput.AnyPlayerInput input;

	public State state { get; protected set; }

	protected virtual CupheadButton LevelInputButton => CupheadButton.Pause;

	protected virtual CupheadButton UIInputButton => CupheadButton.EquipMenu;

	protected virtual InputActionSet CheckedActionSet => InputActionSet.LevelInput;

	protected abstract bool CanPause { get; }

	protected virtual bool CanUnpause => false;

	protected virtual bool RespondToDeadPlayer => false;

	protected virtual float InTime => 0.15f;

	protected virtual float OutTime => 0.15f;

	protected override void Awake()
	{
		base.Awake();
		canvasGroup = GetComponent<CanvasGroup>();
		HideImmediate();
	}

	protected virtual void Update()
	{
		UpdateInput();
	}

	private void UpdateInput()
	{
		if (CanPause && ((CheckedActionSet != 0) ? GetButtonDown(UIInputButton) : GetButtonDown(LevelInputButton)))
		{
			StartCoroutine(ShowPauseMenu());
		}
	}

	public IEnumerator ShowPauseMenu()
	{
		if (MapEventNotification.Current != null)
		{
			while (MapEventNotification.Current.showing)
			{
				yield return null;
			}
		}
		if (state == State.Unpaused && PauseManager.state == PauseManager.State.Unpaused)
		{
			Pause();
		}
		else if (state == State.Paused && CanUnpause)
		{
			Unpause();
		}
	}

	public virtual void Init(bool checkIfDead, OptionsGUI options, AchievementsGUI achievements, RestartTowerConfirmGUI restartTowerConfirmGUI)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	public virtual void Init(bool checkIfDead, OptionsGUI options, AchievementsGUI achievements)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	public virtual void Init(bool checkIfDead)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	protected virtual void Pause()
	{
		if (state == State.Unpaused && PauseManager.state == PauseManager.State.Unpaused)
		{
			StartCoroutine(pause_cr());
		}
	}

	protected virtual void Unpause()
	{
		if (state == State.Paused)
		{
			StartCoroutine(unpause_cr());
		}
	}

	protected virtual void OnPause()
	{
		OnPauseSound();
		if (PlatformHelper.GarbageCollectOnPause)
		{
			GC.Collect();
		}
	}

	protected virtual void OnPauseComplete()
	{
	}

	protected virtual void OnUnpause()
	{
		if (PlatformHelper.GarbageCollectOnPause)
		{
			GC.Collect();
		}
		OnUnpauseSound();
	}

	protected virtual void OnUnpauseComplete()
	{
	}

	protected virtual void OnPauseSound()
	{
		AudioManager.HandleSnapshot(AudioManager.Snapshots.Paused.ToString(), 0.15f);
		AudioManager.PauseAllSFX();
	}

	protected virtual void OnUnpauseSound()
	{
		AudioManager.SnapshotReset((!isWorldMap) ? Level.Current.CurrentScene.ToString() : PlayerData.Data.CurrentMap.ToString(), 0.1f);
		AudioManager.UnpauseAllSFX();
	}

	protected virtual void HideImmediate()
	{
		canvasGroup.alpha = 0f;
		SetInteractable(interactable: false);
	}

	protected virtual void ShowImmediate()
	{
		canvasGroup.alpha = 1f;
		SetInteractable(interactable: true);
	}

	private void SetInteractable(bool interactable)
	{
		canvasGroup.interactable = interactable;
		canvasGroup.blocksRaycasts = interactable;
	}

	protected IEnumerator pause_cr()
	{
		Vibrator.StopVibrating(PlayerId.PlayerOne);
		Vibrator.StopVibrating(PlayerId.PlayerTwo);
		OnPause();
		PauseGameplay();
		SetInteractable(interactable: true);
		yield return StartCoroutine(animate_cr(InTime, InAnimation, 0f, 1f));
		state = State.Paused;
		OnPauseComplete();
	}

	protected IEnumerator unpause_cr()
	{
		OnUnpause();
		SetInteractable(interactable: true);
		UnpauseGameplay();
		yield return StartCoroutine(animate_cr(OutTime, OutAnimation, 1f, 0f));
		state = State.Unpaused;
		SetInteractable(interactable: false);
		OnUnpauseComplete();
	}

	protected virtual void PauseGameplay()
	{
		PauseManager.Pause();
	}

	protected virtual void UnpauseGameplay()
	{
		PauseManager.Unpause();
	}

	private IEnumerator animate_cr(float time, AnimationDelegate anim, float start, float end)
	{
		anim(0f);
		state = State.Animating;
		canvasGroup.alpha = start;
		float t = 0f;
		while (t < time)
		{
			float val = t / time;
			canvasGroup.alpha = Mathf.Lerp(start, end, val);
			anim(val);
			t += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = end;
		anim(1f);
	}

	protected abstract void InAnimation(float i);

	protected abstract void OutAnimation(float i);

	protected bool GetButtonDown(CupheadButton button)
	{
		if (AbstractEquipUI.Current != null && AbstractEquipUI.Current.CurrentState == AbstractEquipUI.ActiveState.Active && button == CupheadButton.EquipMenu)
		{
			return false;
		}
		if (input.GetButtonDown(button))
		{
			return true;
		}
		return false;
	}

	protected void MenuSelectSound()
	{
		AudioManager.Play("level_menu_select");
	}

	protected void MenuMoveSound()
	{
		AudioManager.Play("level_menu_move");
	}

	protected bool GetButton(CupheadButton button)
	{
		if (input.GetButton(button))
		{
			return true;
		}
		return false;
	}
}
