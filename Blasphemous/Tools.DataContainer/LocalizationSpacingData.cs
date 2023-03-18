using System;
using System.Collections.Generic;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.DataContainer;

[CreateAssetMenu(fileName = "Localization Spacing Data", menuName = "Blasphemous/Localization Spacing Data", order = 0)]
public class LocalizationSpacingData : ScriptableObject
{
	[ValueDropdown("MyLanguages")]
	public string Language;

	public int extraSpacing;

	public int extraAfterSpacing;

	public float verticalSpacing;

	public bool addCharacterWidth;

	private IList<ValueDropdownItem<string>> MyLanguages()
	{
		ValueDropdownList<string> valueDropdownList = new ValueDropdownList<string>();
		string[] array = LocalizationManager.GetAllLanguages().ToArray();
		Array.Sort(array);
		string[] array2 = array;
		foreach (string text in array2)
		{
			valueDropdownList.Add(text, text);
		}
		return valueDropdownList;
	}
}
