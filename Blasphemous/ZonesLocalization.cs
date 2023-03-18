using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;

public class ZonesLocalization : MonoBehaviour
{
	public LanguageSource source;

	[Button(ButtonSizes.Small)]
	public void GenerateLocalization()
	{
		List<Transform> list = (from gObj in base.gameObject.scene.GetRootGameObjects()
			select gObj.transform).ToList();
		string pattern = "D(?<ID>[0-9][0-9]) - (?<name>[a-zA-Z0-9_ ()]*)";
		string pattern2 = "Z(?<ID>[0-9][0-9]) - (?<name>[a-zA-Z0-9_ ()]*)";
		Regex regex = new Regex(pattern);
		Regex regex2 = new Regex(pattern2);
		foreach (Transform item in list)
		{
			Match match = regex.Match(item.name);
			if (!match.Success)
			{
				continue;
			}
			string value = match.Groups["ID"].Value;
			string value2 = match.Groups["name"].Value;
			CreateTermIfNeeded("Map/D" + value, value2);
			foreach (Transform item2 in item)
			{
				Match match2 = regex2.Match(item2.name);
				if (match2.Success)
				{
					string value3 = match2.Groups["ID"].Value;
					string value4 = match2.Groups["name"].Value;
					CreateTermIfNeeded("Map/D" + value + "_Z" + value3, value4);
				}
			}
		}
	}

	private void CreateTermIfNeeded(string key, string cad)
	{
		TermData termData = source.GetTermData(key);
		if (termData == null)
		{
			termData = source.AddTerm(key);
		}
		for (int i = 0; i < source.GetLanguages().Count(); i++)
		{
			termData.Languages[i] = cad;
		}
	}
}
