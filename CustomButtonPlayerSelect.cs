using System.Collections;
using Rewired.UI.ControlMapper;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButtonPlayerSelect : CustomButton
{
	public ControlMapper mapper;

	[SerializeField]
	private ButtonInfo myInfo;

	[SerializeField]
	private Image[] selectionTabs;

	protected override void Start()
	{
		base.Start();
		if (!PlayerManager.Multiplayer)
		{
			base.interactable = false;
		}
		StartCoroutine(update_cr());
	}

	private IEnumerator update_cr()
	{
		while (!mapper)
		{
			yield return null;
		}
		for (int i = 0; i < selectionTabs.Length; i++)
		{
			selectionTabs[i].rectTransform.anchoredPosition = new Vector3((associatedText.preferredWidth / 2f + 15f) * (float)(i * 2 - 1), selectionTabs[i].rectTransform.anchoredPosition.y, 0f);
		}
		while (true)
		{
			if (myInfo.intData == mapper.currentPlayerId)
			{
				associatedText.color = base.colors.highlightedColor;
			}
			else
			{
				associatedText.color = ((base.currentSelectionState != SelectionState.Highlighted) ? base.colors.normalColor : base.colors.highlightedColor);
			}
			for (int j = 0; j < selectionTabs.Length; j++)
			{
				selectionTabs[j].enabled = myInfo.intData == mapper.currentPlayerId;
			}
			yield return null;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (IsInteractable())
		{
			base.OnSelect(eventData);
		}
		else
		{
			StartCoroutine(move_selection_cr());
		}
	}

	private IEnumerator move_selection_cr()
	{
		yield return new WaitForEndOfFrame();
		EventSystem.current.SetSelectedGameObject(FindSelectableOnUp().gameObject);
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (!mapper || myInfo.intData != mapper.currentPlayerId)
		{
			switch ((int)state)
			{
			case 0:
				associatedText.color = base.colors.normalColor;
				break;
			case 1:
				associatedText.color = base.colors.highlightedColor;
				break;
			case 2:
				associatedText.color = base.colors.pressedColor;
				break;
			case 3:
				associatedText.color = base.colors.disabledColor;
				break;
			}
		}
	}
}
