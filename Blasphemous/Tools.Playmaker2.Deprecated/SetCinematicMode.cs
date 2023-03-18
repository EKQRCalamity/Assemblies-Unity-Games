using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Deprecated;

[ActionCategory("Blasphemous Deprecated")]
[Tooltip("Starts/Ends the cinematic mode.")]
public class SetCinematicMode : FsmStateAction
{
	public FsmBool cinematic;

	public override void OnEnter()
	{
		Core.UI.Cinematic.CinematicMode(cinematic.Value);
		Finish();
	}
}
