using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Tables;

public class UIListGeneratorEnum : MonoBehaviour
{
	public string enumClassName;

	public string defaultSelected;

	public ObjectEvent OnRequestCategoryInfo;

	public void OnGenerate(UIList list)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		OnRequestCategoryInfo?.Invoke(dictionary);
		Generate(list, enumClassName, defaultSelected, dictionary, null);
	}

	private static UIListItemData[] _GenerateUIListItemData(UIList list, string enumClassName, Dictionary<string, string> categoryData, HashSet<string> exclude, StringTable table)
	{
		if (exclude == null)
		{
			exclude = new HashSet<string>();
		}
		Type type = EnumGenerator.GetType(enumClassName);
		string[] names = EnumUtil.GetNames(type);
		UIListItemData[] array = new UIListItemData[names.Length - exclude.Count];
		bool flag = categoryData == null;
		bool flag2 = type.HasAttribute<UIFieldAttribute>();
		if (categoryData == null)
		{
			categoryData = new Dictionary<string, string>(0);
		}
		list.SetPrefixText(names[0].FriendlyFromCamelOrPascalCasePrefix());
		int num = 0;
		foreach (string text in names)
		{
			if (!exclude.Contains(text))
			{
				object obj = Enum.Parse(type, text);
				Enum value = obj as Enum;
				UICategoryAttribute uICategoryAttribute = (flag ? value.GetAttribute<UICategoryAttribute>() : null);
				if (uICategoryAttribute != null)
				{
					categoryData.Add(text, uICategoryAttribute.category);
				}
				string text2 = ((!flag2) ? null : value.GetAttribute<UIFieldAttribute>()?.label) ?? text.FriendlyFromCamelOrPascalCase();
				int num2 = num++;
				string text3 = table.AutoLocalize(text2);
				string category = (categoryData.ContainsKey(text) ? table.AutoLocalize(categoryData[text]) : null);
				UITooltipAttribute attribute = value.GetAttribute<UITooltipAttribute>();
				array[num2] = new UIListItemData(text3, obj, category, null, (attribute == null) ? null : (table?.TryAutoLocalize(text2 + "-tooltip") ?? ((string)attribute)));
			}
		}
		return array;
	}

	public static void Generate(UIList list, string enumClassName, string defaultSelected, Dictionary<string, string> categoryData, HashSet<string> exclude, StringTable table = null)
	{
		UICategorySortAttribute attribute = EnumGenerator.GetType(enumClassName).GetAttribute<UICategorySortAttribute>();
		if (attribute != null)
		{
			list.categorySorting = attribute.sorting;
		}
		list.Set(_GenerateUIListItemData(list, enumClassName, categoryData, exclude, table));
		if (defaultSelected != "" && list is ComboBox comboBox)
		{
			comboBox.defaultSelected = defaultSelected;
		}
	}
}
