using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Reveal a secret from a map.")]
public class MapRevealSecret : FsmStateAction
{
	[RequiredField]
	public FsmString mapId;

	[RequiredField]
	public FsmString secretId;

	public FsmBool enableSecret;

	public FsmBool useMapIdInsteadOfCurrentMap;

	public override void Reset()
	{
		if (mapId == null)
		{
			mapId = new FsmString();
		}
		mapId.Value = string.Empty;
		if (secretId == null)
		{
			secretId = new FsmString();
		}
		secretId.Value = string.Empty;
		if (enableSecret == null)
		{
			enableSecret = new FsmBool();
		}
		enableSecret.Value = true;
	}

	public override void OnEnter()
	{
		string value = ((mapId == null) ? string.Empty : mapId.Value);
		string value2 = ((secretId == null) ? string.Empty : secretId.Value);
		bool enable = enableSecret == null || enableSecret.Value;
		bool flag = useMapIdInsteadOfCurrentMap != null && useMapIdInsteadOfCurrentMap.Value;
		if (string.IsNullOrEmpty(value))
		{
			LogWarning("PlayMaker Action MapRevealSecret - mapId is blank");
			return;
		}
		if (string.IsNullOrEmpty(value2))
		{
			LogWarning("PlayMaker Action MapRevealSecret - secretId is blank");
			return;
		}
		if (flag)
		{
			Core.NewMapManager.SetSecret(value, value2, enable);
		}
		else
		{
			Core.NewMapManager.SetSecret(value2, enable);
		}
		Finish();
	}
}
