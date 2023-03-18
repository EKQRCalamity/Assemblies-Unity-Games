using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Checks if the teleport is active.")]
public class TeleportIsActive : FsmStateAction
{
	public FsmString teleportId;

	public FsmEvent teleportAvailable;

	public FsmEvent teleportUnavailable;

	public override void Reset()
	{
		if (teleportId == null)
		{
			teleportId = new FsmString();
		}
		teleportId.Value = string.Empty;
	}

	public override void OnEnter()
	{
		string value = ((teleportId == null) ? string.Empty : teleportId.Value);
		if (string.IsNullOrEmpty(value))
		{
			LogWarning("PlayMaker Action TeleportIsActive - teleportId is blank");
		}
		else if (Core.SpawnManager.IsTeleportActive(value))
		{
			base.Fsm.Event(teleportAvailable);
		}
		else
		{
			base.Fsm.Event(teleportUnavailable);
		}
		Finish();
	}
}
