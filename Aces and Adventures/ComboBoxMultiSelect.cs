using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboBoxMultiSelect : ComboBox
{
	public Sprite toggleSprite;

	public string textPrefix = "";

	[Range(0f, 100f)]
	public int maxCount;

	private bool[] selectedIndices;

	private string _selectedItemText;

	public int numSelected
	{
		get
		{
			int num = 0;
			bool[] array = selectedIndices;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i])
				{
					num++;
				}
			}
			return num;
		}
	}

	protected override void DoDefaultSelectedLogic()
	{
		_InitSelectedIndicesFromDefaultSelected();
		RefreshSelectedItem();
	}

	public override void Clear()
	{
		base.Clear();
		selectedIndices = null;
	}

	protected override void _SetUniqueLogic()
	{
		base._SetUniqueLogic();
		for (int i = 0; i < items.Count; i++)
		{
			items[i].SetToggleImageSprite(toggleSprite);
		}
		_UpdateToggles();
	}

	public override void SelectIndex(int index, bool toggleList = true)
	{
		if (maxCount <= 0 || selectedIndices[index] || numSelected < maxCount)
		{
			if (OnSelected != null)
			{
				OnSelected.Invoke();
			}
			selectedIndex = index;
			selectedIndices[index] = !selectedIndices[index];
			SignalSelectionChanged();
			RefreshSelectedItem();
		}
	}

	protected override void RefreshSelectedItem()
	{
		base.RefreshSelectedItem();
		_UpdateToggles();
		_selectedItemText = selectedItem.text.text;
	}

	protected override UIListItemData GetSelectedItemDataFromSelection()
	{
		UIListItemData result = null;
		List<object> selectedValues = GetSelectedValues();
		if (selectedValues.Count == selectedIndices.Length)
		{
			result = new UIListItemData("All", selectedValues);
		}
		else if (selectedValues.Count > 0)
		{
			result = new UIListItemData((from data in GetSelectedUIListItemData()
				select data.text).ToStringSmart(), selectedValues);
		}
		else if (selectedValues.Count == 0)
		{
			result = new UIListItemData("None", selectedValues);
		}
		return result;
	}

	protected override string GetSelectedText()
	{
		return (from data in GetSelectedUIListItemData()
			select textPrefix + data.text).ToStringSmart();
	}

	protected override object GetSelectedValue()
	{
		return GetSelectedValues();
	}

	private int GetNumberSelected()
	{
		int num = 0;
		for (int i = 0; i < selectedIndices.Length; i++)
		{
			if (selectedIndices[i])
			{
				num++;
			}
		}
		return num;
	}

	private List<object> GetSelectedValues()
	{
		List<object> list = new List<object>();
		for (int i = 0; i < selectedIndices.Length; i++)
		{
			if (selectedIndices[i])
			{
				list.Add(data[i].value);
			}
		}
		return list;
	}

	private List<UIListItemData> GetSelectedUIListItemData()
	{
		List<UIListItemData> list = new List<UIListItemData>();
		for (int i = 0; i < selectedIndices.Length; i++)
		{
			if (selectedIndices[i])
			{
				list.Add(data[i]);
			}
		}
		return list;
	}

	private void _InitSelectedIndicesFromDefaultSelected()
	{
		if (selectedIndices != null)
		{
			return;
		}
		HashSet<string> hashSet = (defaultSelected.IsNullOrEmpty() ? null : (from s in defaultSelected.Split(',')
			select s.TrimStart(' ')).ToHash());
		selectedIndices = new bool[data.Count];
		if (data.Count <= 0 || hashSet == null)
		{
			return;
		}
		for (int i = 0; i < data.Count; i++)
		{
			if (hashSet.Contains(data[i].value.ToString()))
			{
				selectedIndex = i;
				selectedIndices[i] = true;
			}
			else
			{
				selectedIndices[i] = false;
			}
		}
	}

	private void _UpdateToggles()
	{
		if (selectedIndices != null && items.Count == data.Count)
		{
			for (int i = 0; i < selectedIndices.Length; i++)
			{
				items[i].SetToggleImageState(selectedIndices[i]);
			}
		}
	}

	public override void SetPrefixText(string prefixText)
	{
		textPrefix = prefixText;
	}

	public override void SetMaxCount(int maxCount)
	{
		this.maxCount = maxCount;
	}
}
