using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Marks a Boss Rush Course As Unlocked.")]
public class MarkCourseAsUnlocked : FsmStateAction
{
	public FsmString courseId;

	public override void OnEnter()
	{
		string id = courseId.Value + "_UNLOCKED";
		Core.Events.SetFlag(id, b: true, forcePreserve: true);
		Finish();
	}
}
