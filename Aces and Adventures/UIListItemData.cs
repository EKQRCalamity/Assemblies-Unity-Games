using System;

public class UIListItemData : IComparable<UIListItemData>
{
	public string text;

	public object value;

	public string category;

	public string tooltip;

	private string _searchText;

	public string searchText => _searchText ?? text;

	public UIListItemData(string text, object value, string category = null, string searchString = null, string tooltip = null)
	{
		this.text = text;
		this.value = value;
		this.category = category;
		this.tooltip = tooltip;
		_searchText = searchString;
	}

	public int CompareTo(UIListItemData other)
	{
		return string.Compare(text, other.text, StringComparison.OrdinalIgnoreCase);
	}
}
