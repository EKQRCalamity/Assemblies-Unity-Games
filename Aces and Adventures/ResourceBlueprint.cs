using UnityEngine;

public class ResourceBlueprint<T> where T : Object
{
	private string _resourcePath;

	private T _value;

	public T value
	{
		get
		{
			if (!(Object)_value)
			{
				return _value = Resources.Load<T>(_resourcePath);
			}
			return _value;
		}
	}

	public string path => _resourcePath;

	public ResourceBlueprint(string resourcePath)
	{
		_resourcePath = resourcePath;
	}

	public static implicit operator T(ResourceBlueprint<T> blueprint)
	{
		if (blueprint == null)
		{
			return null;
		}
		return blueprint.value;
	}

	public static implicit operator ResourceBlueprint<T>(string resourcePath)
	{
		return new ResourceBlueprint<T>(resourcePath);
	}
}
