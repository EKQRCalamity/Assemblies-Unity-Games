using System;
using System.Linq;
using UnityEngine;

public class UIListGeneratorDerived : MonoBehaviour
{
	public string baseClassName;

	public bool includeBaseClass;

	public string defaultSelected;

	public void OnGenerate(UIList list)
	{
		Generate(list, baseClassName, defaultSelected, includeBaseClass);
	}

	public void SetBaseClassName(string name)
	{
		baseClassName = name;
	}

	public static UIListItemData[] GenerateUIListItemData(string baseClassName, bool includeBaseClass, Func<Type, bool> excludeType = null, bool allowInheritedUIField = false)
	{
		excludeType = excludeType ?? ((Func<Type, bool>)((Type t) => false));
		Type type = Type.GetType(baseClassName);
		Type[] array = (from t in includeBaseClass ? type.GetAssignableClasses() : type.GetDerivedClasses()
			where t.GetAttribute<UIFieldAttribute>(allowInheritedUIField) != null && t.IsConcrete() && !excludeType(t)
			orderby t.GetUIOrder()
			select t).ToArray();
		UIListItemData[] array2 = new UIListItemData[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			UIFieldAttribute attribute = array[i].GetAttribute<UIFieldAttribute>(allowInheritedUIField);
			array2[i] = new UIListItemData(attribute.label ?? array[i].GetUILabel(), array[i], attribute.category, null, attribute.tooltip);
		}
		return array2;
	}

	public static void Generate(UIList list, string baseClassName, string defaultSelected, bool includeBaseClass, Func<Type, bool> excludeType = null, bool allowInheritedUIField = false)
	{
		UICategorySortAttribute attribute = EnumGenerator.GetType(baseClassName).GetAttribute<UICategorySortAttribute>();
		if (attribute != null)
		{
			list.categorySorting = attribute.sorting;
		}
		list.Set(GenerateUIListItemData(baseClassName, includeBaseClass, excludeType, allowInheritedUIField));
		if (defaultSelected != "" && list is ComboBox comboBox)
		{
			comboBox.defaultSelected = defaultSelected;
		}
	}
}
