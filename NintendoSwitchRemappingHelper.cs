using System;
using System.Collections.Generic;

public static class NintendoSwitchRemappingHelper
{
	private static readonly string P1Identifier = "r2|0";

	private static readonly string P2Identifier = "r2|1";

	private static readonly Dictionary<string, string> DualControllers = new Dictionary<string, string>
	{
		{ "521b808c-0248-4526-bc10-f1d16ee76bf1", "Joy-Con (Dual)" },
		{ "7bf3154b-9db8-4d52-950f-cd0eed8a5819", "Pro Controller" },
		{ "1fbdd13b-0795-4173-8a95-a2a75de9d204", "Handheld" }
	};

	private static readonly Dictionary<string, string> SingleControllers = new Dictionary<string, string>
	{
		{ "3eb01142-da0e-4a86-8ae8-a15c2b1f2a04", "Joy-Con (L)" },
		{ "605dc720-1b38-473d-a459-67d5857aa6ea", "Joy-Con (R)" }
	};

	public static void DuplicateXML(Dictionary<string, string> updatedMappings, Dictionary<string, string> existingMappings)
	{
		if (updatedMappings.Count > 1)
		{
			throw new Exception("More than one mapping found!");
		}
		foreach (KeyValuePair<string, string> updatedMapping in updatedMappings)
		{
			string text = null;
			if (updatedMapping.Key.Contains(P1Identifier))
			{
				text = P1Identifier;
			}
			else if (updatedMapping.Key.Contains(P2Identifier))
			{
				text = P2Identifier;
			}
			if (text == null)
			{
				throw new Exception("Unable to determine controller mapping origin");
			}
			Dictionary<string, string> dictionary = null;
			string oldValue = null;
			string oldValue2 = null;
			foreach (KeyValuePair<string, string> dualController in DualControllers)
			{
				if (updatedMapping.Key.Contains(dualController.Key))
				{
					oldValue = dualController.Key;
					oldValue2 = dualController.Value;
					dictionary = DualControllers;
					break;
				}
			}
			foreach (KeyValuePair<string, string> singleController in SingleControllers)
			{
				if (updatedMapping.Key.Contains(singleController.Key))
				{
					oldValue = singleController.Key;
					oldValue2 = singleController.Value;
					dictionary = SingleControllers;
					break;
				}
			}
			if (dictionary == null)
			{
				throw new Exception("Unable to determine controller search values");
			}
			string value = updatedMapping.Value;
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				string key = item.Key;
				string value2 = item.Value;
				string key2 = updatedMapping.Key.Replace(oldValue, key);
				string text2 = value.Replace(oldValue, key);
				text2 = text2.Replace(oldValue2, value2);
				existingMappings[key2] = text2;
			}
		}
	}
}
