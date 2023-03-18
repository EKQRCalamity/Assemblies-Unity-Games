using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Start a playmaker cinematics in other scene.")]
public class StartCinematicsInOtherScene : FsmStateAction
{
	public FsmString LevelName;

	public FsmString PlayMakerEventName;

	public FsmBool HidePlayer;

	public override void Reset()
	{
		if (LevelName == null)
		{
			LevelName = new FsmString();
			LevelName.Value = string.Empty;
		}
		if (PlayMakerEventName == null)
		{
			PlayMakerEventName = new FsmString();
			PlayMakerEventName.Value = string.Empty;
		}
		if (HidePlayer == null)
		{
			HidePlayer = new FsmBool();
			HidePlayer.Value = true;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		string levelName = ((LevelName == null) ? string.Empty : LevelName.Value);
		string eventName = ((PlayMakerEventName == null) ? string.Empty : PlayMakerEventName.Value);
		bool hideplayer = HidePlayer == null || HidePlayer.Value;
		Core.LevelManager.ChangeLevelAndPlayEvent(levelName, eventName, hideplayer);
		Finish();
	}
}
