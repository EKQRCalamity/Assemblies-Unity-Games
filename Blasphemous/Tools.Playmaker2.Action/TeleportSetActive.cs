using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Activate or deactivate a TeleportLocation.")]
public class TeleportSetActive : FsmStateAction
{
	[RequiredField]
	public FsmString teleportId;

	public FsmBool active;

	public override void Reset()
	{
		if (teleportId == null)
		{
			teleportId = new FsmString();
		}
		teleportId.Value = string.Empty;
		if (active == null)
		{
			active = new FsmBool();
		}
		active.Value = true;
	}

	public override void OnEnter()
	{
		string value = ((teleportId == null) ? string.Empty : teleportId.Value);
		bool flag = active != null && active.Value;
		if (string.IsNullOrEmpty(value))
		{
			LogWarning("PlayMaker Action TeleportSetActive - teleportId is blank");
			return;
		}
		Core.SpawnManager.SetTeleportActive(value, flag);
		Finish();
	}
}
