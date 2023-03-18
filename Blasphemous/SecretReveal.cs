using System;
using Framework.Managers;

[Serializable]
public class SecretReveal
{
	public bool RevealSecret;

	public string MapId = string.Empty;

	public string SecretId = string.Empty;

	public bool UseMapIdInsteadOfCurrentMap;

	public void Reveal()
	{
		if (UseMapIdInsteadOfCurrentMap)
		{
			if (RevealSecret && MapId != string.Empty && SecretId != string.Empty)
			{
				Core.NewMapManager.SetSecret(MapId, SecretId, enabled: true);
			}
		}
		else if (RevealSecret && SecretId != string.Empty)
		{
			Core.NewMapManager.SetSecret(SecretId, enable: true);
		}
	}
}
