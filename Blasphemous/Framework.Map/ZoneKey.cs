using System;
using UnityEngine;

namespace Framework.Map;

[Serializable]
public class ZoneKey
{
	[SerializeField]
	private string _district;

	[SerializeField]
	private string _zone;

	[SerializeField]
	private string _scene;

	public string District
	{
		get
		{
			return _district;
		}
		set
		{
			_district = value;
		}
	}

	public string Zone
	{
		get
		{
			return _zone;
		}
		set
		{
			_zone = value;
		}
	}

	public string Scene
	{
		get
		{
			return _scene;
		}
		set
		{
			_scene = value;
		}
	}

	public ZoneKey()
	{
		_district = string.Empty;
		_zone = string.Empty;
	}

	public ZoneKey(string district, string zone, string scene)
	{
		_district = district;
		_zone = zone;
		_scene = scene;
	}

	public ZoneKey(ZoneKey other)
	{
		_district = other.District;
		_zone = other.Zone;
		_scene = other.Scene;
	}

	public int GetSceneInt()
	{
		int result = 0;
		if (Scene != null && Scene != string.Empty)
		{
			int.TryParse(Scene.Substring(1), out result);
		}
		return result;
	}

	public string GetKey()
	{
		return _district + "_" + _zone + "_" + _scene;
	}

	public string GetLevelName()
	{
		return _district + _zone + _scene;
	}

	public string GetLocalizationKey()
	{
		return _district + "_" + _zone;
	}

	public override int GetHashCode()
	{
		string key = GetKey();
		return key.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ZoneKey);
	}

	public bool Equals(ZoneKey obj)
	{
		return obj != null && obj.Zone == Zone && obj.District == District && obj.Scene == Scene;
	}

	public bool IsEmpty()
	{
		return District == string.Empty || Zone == string.Empty || Scene == string.Empty;
	}
}
