using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Change the current map.")]
public class MapChange : FsmStateAction
{
	[RequiredField]
	public FsmString mapId;

	public override void Reset()
	{
		if (mapId == null)
		{
			mapId = new FsmString();
		}
		mapId.Value = string.Empty;
	}

	public override void OnEnter()
	{
		string text = ((mapId == null) ? string.Empty : mapId.Value);
		if (string.IsNullOrEmpty(text))
		{
			LogWarning("PlayMaker Action MapChange - mapId is blank");
			return;
		}
		Core.NewMapManager.SetCurrentMap(text);
		Finish();
	}
}
