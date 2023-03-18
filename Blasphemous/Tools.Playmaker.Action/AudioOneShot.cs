using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Plays an audio.")]
public class AudioOneShot : FsmStateAction
{
	public FsmString audioId;

	public override void OnEnter()
	{
		Core.Audio.PlaySfx(audioId.Value);
		Finish();
	}
}
