using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DLCGenericCutscene : Cutscene
{
	protected enum TrappedChar
	{
		None = -1,
		Cuphead,
		Mugman,
		Chalice
	}

	private CupheadInput.AnyPlayerInput input;

	[SerializeField]
	private float cursorToVisableTime = 1.25f;

	[SerializeField]
	private float mainDelay = 0.25f;

	[SerializeField]
	private Image arrow;

	[SerializeField]
	protected GameObject[] text;

	[SerializeField]
	protected Animator[] screens;

	private int activeScreen;

	protected bool allowScreenSkip;

	private float arrowTransparency;

	private bool arrowVisible;

	private int textCounter = -1;

	private int curPathHash;

	protected int curScreen;

	protected bool fastForwardActive;

	public Image fader;

	protected override void Start()
	{
		base.Start();
		input = new CupheadInput.AnyPlayerInput();
		CutsceneGUI.Current.pause.pauseAllowed = false;
		StartCoroutine(main_cr());
		StartCoroutine(skip_cr());
	}

	protected virtual void Update()
	{
		if (arrowVisible)
		{
			arrowTransparency = Mathf.Clamp01(arrowTransparency + Time.deltaTime / 0.25f);
		}
		else
		{
			arrowTransparency = 0f;
		}
		arrow.color = new Color(1f, 1f, 1f, arrowTransparency);
	}

	protected virtual void OnScreenAdvance(int which)
	{
	}

	protected virtual void OnContinue()
	{
	}

	protected virtual void OnScreenSkip()
	{
	}

	private IEnumerator main_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, mainDelay);
		for (curScreen = 0; curScreen < screens.Length; curScreen++)
		{
			screens[curScreen].gameObject.SetActive(value: true);
			int target = Animator.StringToHash(screens[curScreen].GetLayerName(0) + ".End");
			while (screens[curScreen].GetCurrentAnimatorStateInfo(0).fullPathHash != target)
			{
				yield return null;
				if (arrowVisible)
				{
					while ((input.GetButtonDown(CupheadButton.Pause) || !input.GetAnyButtonDown()) && !fastForwardActive)
					{
						yield return null;
					}
					curPathHash = screens[curScreen].GetCurrentAnimatorStateInfo(0).fullPathHash;
					screens[curScreen].SetTrigger("Continue");
					OnContinue();
					text[textCounter].SetActive(value: false);
					arrowVisible = false;
				}
				else if (allowScreenSkip && input.GetAnyButtonDown())
				{
					OnScreenSkip();
				}
			}
			OnScreenAdvance(curScreen);
			if (curScreen < screens.Length - 1)
			{
				screens[curScreen].gameObject.SetActive(value: false);
			}
			arrowVisible = false;
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		Skip();
	}

	public void IrisIn()
	{
		allowScreenSkip = false;
		Animator component = fader.GetComponent<Animator>();
		Color color = fader.color;
		color.a = 1f;
		fader.color = color;
		component.SetTrigger("Iris_In");
	}

	public virtual void IrisOut()
	{
		Animator component = fader.GetComponent<Animator>();
		Color color = fader.color;
		color.a = 1f;
		fader.color = color;
		component.SetTrigger("Iris_Out");
	}

	public void ShowText()
	{
		textCounter++;
		text[textCounter].SetActive(value: true);
	}

	public void ShowArrow()
	{
		if (curPathHash != screens[curScreen].GetCurrentAnimatorStateInfo(0).fullPathHash)
		{
			arrowVisible = true;
		}
	}

	private IEnumerator skip_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (true)
		{
			if (input.GetButtonDown(CupheadButton.Pause))
			{
				Skip();
			}
			yield return null;
		}
	}

	protected TrappedChar DetectCharacter()
	{
		if (PlayerManager.Multiplayer)
		{
			if (PlayerManager.playerWasChalice[0] || PlayerManager.playerWasChalice[1])
			{
				if (PlayerManager.playerWasChalice[0])
				{
					return PlayerManager.player1IsMugman ? TrappedChar.Mugman : TrappedChar.Cuphead;
				}
				return (!PlayerManager.player1IsMugman) ? TrappedChar.Mugman : TrappedChar.Cuphead;
			}
			return TrappedChar.Chalice;
		}
		if (PlayerManager.playerWasChalice[0])
		{
			return PlayerManager.player1IsMugman ? TrappedChar.Mugman : TrappedChar.Cuphead;
		}
		return (PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo).charm != Charm.charm_chalice) ? TrappedChar.Chalice : ((!PlayerManager.player1IsMugman) ? TrappedChar.Mugman : TrappedChar.Cuphead);
	}
}
