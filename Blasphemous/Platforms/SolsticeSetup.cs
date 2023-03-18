using System;
using UnityEngine;

namespace Platforms;

public static class SolsticeSetup
{
	private static readonly string SOLSTICE_MODE_ENVVAR = "SOLSTICE_LAUNCH_MODE";

	public static bool IsSolsticeRelease()
	{
		string environmentVariable = Environment.GetEnvironmentVariable(SOLSTICE_MODE_ENVVAR);
		Debug.Log("Solstice Mode: " + environmentVariable);
		return environmentVariable == "RELEASE";
	}
}
