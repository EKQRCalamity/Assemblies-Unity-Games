using System;

[AttributeUsage(AttributeTargets.Enum)]
public class ResourceEnumAttribute : Attribute
{
	public string resourcePath;

	public bool categoriesAreSubFolders;

	public ResourceEnumAttribute(string resourcePath, bool categoriesAreSubFolders = false)
	{
		this.resourcePath = resourcePath;
		this.categoriesAreSubFolders = categoriesAreSubFolders;
	}
}
