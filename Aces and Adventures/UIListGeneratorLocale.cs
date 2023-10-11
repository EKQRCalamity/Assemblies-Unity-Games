using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UIListGeneratorLocale : MonoBehaviour
{
	private static Locale[] _Locales;

	private static Locale[] Locales => _Locales ?? (_Locales = (from locale in LocalizationSettings.AvailableLocales.Locales
		where !locale.ExcludeFromSelection()
		orderby locale.SortPriority() descending
		select locale).ToArray());

	private static UIListItemData[] _GenerateUIListItemData()
	{
		UIListItemData[] array = new UIListItemData[Locales.Length];
		for (int i = 0; i < Locales.Length; i++)
		{
			Locale locale = Locales[i];
			array[i] = new UIListItemData(locale.Identifier.Code, locale.Identifier);
		}
		return array;
	}

	public static void Generate(UIList list, LocaleIdentifier selectedLocale)
	{
		list.Set(_GenerateUIListItemData());
		SelectLocaleItem[] componentsInChildren = list.listContainer.GetComponentsInChildren<SelectLocaleItem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].locale = Locales[i];
		}
		ComboBox box = list as ComboBox;
		if ((object)box == null)
		{
			return;
		}
		box.defaultSelected = selectedLocale.ToString();
		box.OnSelectedValueChanged.AddListener(delegate(object value)
		{
			if ((bool)box && (bool)box.selectedContainer)
			{
				SelectLocaleItem componentInChildren = box.selectedContainer.GetComponentInChildren<SelectLocaleItem>();
				if ((object)componentInChildren != null)
				{
					componentInChildren.locale = LocalizationSettings.AvailableLocales.GetLocale((LocaleIdentifier)value);
				}
			}
		});
	}
}
