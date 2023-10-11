using System;

[AttributeUsage(AttributeTargets.All)]
public class UICategorySortAttribute : Attribute
{
	public CategorySortType sorting { get; set; }

	public UICategorySortAttribute(CategorySortType sorting)
	{
		this.sorting = sorting;
	}
}
