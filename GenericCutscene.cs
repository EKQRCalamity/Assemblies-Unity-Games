using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GenericCutscene : Cutscene
{
	private CupheadInput.AnyPlayerInput input;

	[SerializeField]
	private bool specialCase;

	[SerializeField]
	private Image arrow;

	[SerializeField]
	private int numScreens;

	private float arrowTransparency;

	private bool arrowVisible;

	protected override void Start()
	{
		base.Start();
		input = new CupheadInput.AnyPlayerInput();
		CutsceneGUI.Current.pause.pauseAllowed = false;
		StartCoroutine(main_cr());
		StartCoroutine(skip_cr());
	}

	private IEnumerator main_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		for (int i = 0; i < numScreens; i++)
		{
			if (specialCase && i == 3)
			{
				yield return CupheadTime.WaitForSeconds(this, 2.25f);
			}
			else
			{
				yield return CupheadTime.WaitForSeconds(this, 1.25f);
			}
			arrowVisible = true;
			while (input.GetButtonDown(CupheadButton.Pause) || !input.GetAnyButtonDown())
			{
				yield return null;
			}
			arrowVisible = false;
			if (i < numScreens - 1)
			{
				base.animator.SetTrigger("Continue");
			}
			AudioManager.Play("ui_confirm_generic");
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		Skip();
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

	private void Update()
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

	protected override void OnDestroy()
	{
		base.OnDestroy();
		arrow = null;
	}
}
