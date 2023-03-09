using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DevilCutsceneOptionSelector : AbstractMonoBehaviour
{
	public Transform[] bakedOptions;

	public Transform[] options;

	public Transform cursor;

	public DevilCutscene cutscene;

	private int currentOption;

	private CupheadInput.AnyPlayerInput input;

	public Image cursorImage;

	private void Start()
	{
		input = new CupheadInput.AnyPlayerInput();
		cursor.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		StartCoroutine(main_cr());
		StartCoroutine(fadeIn_cr());
	}

	private IEnumerator main_cr()
	{
		cursor.gameObject.SetActive(value: true);
		if (cutscene.IsTranslationTextActive())
		{
			cursor.transform.position = options[currentOption].position;
		}
		else
		{
			cursor.transform.position = bakedOptions[currentOption].position;
		}
		while (true)
		{
			int prevOption = currentOption;
			if (input.GetButtonDown(CupheadButton.MenuLeft))
			{
				currentOption = Mathf.Max(0, currentOption - 1);
			}
			if (input.GetButtonDown(CupheadButton.MenuRight))
			{
				currentOption = Mathf.Min(options.Length - 1, currentOption + 1);
			}
			if (cutscene.IsTranslationTextActive())
			{
				cursor.transform.position = options[currentOption].position;
			}
			else
			{
				cursor.transform.position = bakedOptions[currentOption].position;
			}
			if (currentOption > prevOption)
			{
				ToggleSFX();
				base.animator.SetTrigger("MoveRight");
			}
			if (currentOption < prevOption)
			{
				ToggleSFX();
				base.animator.SetTrigger("MoveLeft");
			}
			if (input.GetButtonDown(CupheadButton.Accept))
			{
				break;
			}
			yield return null;
		}
		if (currentOption == 0)
		{
			cutscene.RefuseDevil();
		}
		else
		{
			cutscene.JoinDevil();
		}
		cursor.gameObject.SetActive(value: false);
	}

	private IEnumerator fadeIn_cr()
	{
		float t = 0f;
		while (t < 0.75f)
		{
			cursorImage.color = new Color(1f, 1f, 1f, t / 0.75f);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		cursorImage.color = new Color(1f, 1f, 1f, 1f);
	}

	private void ToggleSFX()
	{
		AudioManager.Play("ui_toggle");
	}
}
