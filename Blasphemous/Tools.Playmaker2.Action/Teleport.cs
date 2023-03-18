using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Teleport to a TeleportLocation.")]
public class Teleport : FsmStateAction
{
	[RequiredField]
	public FsmString teleportId;

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
			LogWarning("PlayMaker Action Teleport - teleportId is blank");
			return;
		}
		Core.SpawnManager.Teleport(value);
		Finish();
	}
}
