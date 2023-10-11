using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SelectLocaleDropdown : MonoBehaviour
{
	public SelectLocaleItem itemBlueprint;

	public GameObject workInProgressBlueprint;

	public RectTransform selectedItemContainer;

	public RectTransform itemContainer;

	public UnityEvent onItemsGenerated;

	public BoolEvent onIsWorkInProgressChange;

	private void _GenerateItem(Locale locale, RectTransform container)
	{
		SelectLocaleItem component = Object.Instantiate(itemBlueprint.gameObject, container, worldPositionStays: false).GetComponent<SelectLocaleItem>();
		component.locale = locale;
		if (!(container == selectedItemContainer))
		{
			component.button.onClick.AddListener(delegate
			{
				ProfileManager.prefs.localeOverride = locale;
				ProfileManager.Profile.SavePreferences();
				_GenerateItems();
			});
		}
	}

	private void _GenerateItems(bool signalItemsGenerated = true)
	{
		_GenerateSelectedItem();
		_GenerateUnselectedItems(signalItemsGenerated);
	}

	private void _GenerateSelectedItem()
	{
		selectedItemContainer?.gameObject.DestroyChildren();
		_GenerateItem(LocalizationSettings.SelectedLocale, selectedItemContainer);
		onIsWorkInProgressChange?.Invoke(LocalizationSettings.SelectedLocale.IsWorkInProgress());
	}

	private void _GenerateUnselectedItems(bool signalItemsGenerated = true)
	{
		int childCount = itemContainer.childCount;
		itemContainer?.gameObject.DestroyChildren();
		foreach (Locale item in LocalizationSettings.AvailableLocales.Locales.OrderByDescending((Locale l) => l.SortPriority()))
		{
			if (item != LocalizationSettings.SelectedLocale && !item.ExcludeFromSelection())
			{
				_GenerateItem(item, itemContainer);
			}
		}
		Object.Instantiate(workInProgressBlueprint, itemContainer, worldPositionStays: false);
		if (signalItemsGenerated)
		{
			if (childCount == 0)
			{
				StartCoroutine(_InvokeItemsGeneratedDelayed());
			}
			else
			{
				onItemsGenerated?.Invoke();
			}
		}
	}

	private IEnumerator _InvokeItemsGeneratedDelayed()
	{
		for (int x = 0; x < 10; x++)
		{
			yield return new WaitForEndOfFrame();
		}
		onItemsGenerated?.Invoke();
	}

	private void _OnSelectedLocaleChanges(Locale selectedLocale)
	{
		PauseMenu instance = PauseMenu.Instance;
		if ((object)instance != null && instance.optionsActive)
		{
			_GenerateItems(signalItemsGenerated: false);
		}
	}

	private void Start()
	{
		_GenerateSelectedItem();
		LocalizationSettings.SelectedLocaleChanged += _OnSelectedLocaleChanges;
	}

	private void OnDestroy()
	{
		LocalizationSettings.SelectedLocaleChanged -= _OnSelectedLocaleChanges;
	}

	public void GenerateItems()
	{
		if (itemContainer.childCount == 0)
		{
			_GenerateUnselectedItems();
		}
		else
		{
			onItemsGenerated?.Invoke();
		}
	}

	public void ShowHelpLocalizePopup()
	{
		Transform parent = GetComponentInParent<Canvas>().transform;
		UIUtil.CreatePopup(MessageData.UIPopupTitle.HelpLocalize.GetTitle().Localize(), UIUtil.CreateMessageBox(MessageData.UIPopupMessage.HelpLocalizeBody.GetMessage().Localize(), TextAlignmentOptions.MidlineLeft, 24, 800, 300, 24f), null, parent: parent, buttons: new string[2]
		{
			MessageData.UIPopupButton.Cancel.GetButton().Localize(),
			MessageData.UIPopupButton.HelpLocalize.GetButton().Localize()
		}, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s == MessageData.UIPopupButton.HelpLocalize.GetButton().Localize())
			{
				UIUtil.JoinDiscord(parent);
			}
		});
	}
}
