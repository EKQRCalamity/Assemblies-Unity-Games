using System.Collections;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Show the popup in screen and block player.")]
public class ShowHowToPlayPopup : FsmStateAction
{
	public FsmString popupId;

	public FsmBool blockPlayer;

	public override void Reset()
	{
		popupId = new FsmString();
		popupId.UseVariable = false;
		popupId.Value = string.Empty;
		blockPlayer = new FsmBool();
		blockPlayer.UseVariable = false;
		blockPlayer.Value = true;
	}

	public override void OnEnter()
	{
		string id = ((popupId == null) ? string.Empty : popupId.Value);
		bool block = blockPlayer == null || blockPlayer.Value;
		StartCoroutine(ShowCourrutine(id, block));
	}

	private IEnumerator ShowCourrutine(string id, bool block)
	{
		yield return Core.TutorialManager.ShowTutorial(id, block);
		Finish();
	}
}
