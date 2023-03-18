using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Set if the Santos cutscene is playing.")]
public class SetSantosCutscenePlaying : FsmStateAction
{
	public FsmBool isPlaying;

	public override void OnEnter()
	{
		Core.Cinematics.InSantosCutscene = isPlaying.Value;
		Finish();
	}
}
