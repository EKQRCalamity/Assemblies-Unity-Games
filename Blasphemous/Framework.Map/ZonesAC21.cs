using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Map;

[CreateAssetMenu(menuName = "Blasphemous/Map Zones config")]
public class ZonesAC21 : SerializedScriptableObject
{
	public Dictionary<string, bool> ZonesConfig = new Dictionary<string, bool>();

	public bool AllowZoneForAc21(ZoneKey zone)
	{
		return AllowZoneForAc21(zone.District, zone.Zone);
	}

	public bool AllowZoneForAc21(string district, string zone)
	{
		bool result = false;
		string key = district.ToUpper() + zone.ToUpper();
		if (ZonesConfig.ContainsKey(key))
		{
			result = ZonesConfig[key];
		}
		return result;
	}
}
