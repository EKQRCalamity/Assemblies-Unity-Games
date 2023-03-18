using System.Collections;
using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Play fullmessage on screen.")]
public class ShowFullMessage : FsmStateAction
{
	public UIController.FullMensages type;

	public float totalTime = 2f;

	public float fadeInTime = 0.5f;

	public float fadeOutTime = 0.5f;

	public override void OnEnter()
	{
		StartCoroutine(ShowCourrutine());
	}

	private IEnumerator ShowCourrutine()
	{
		yield return UIController.instance.ShowFullMessageCourrutine(type, totalTime, fadeInTime, fadeOutTime);
		Finish();
	}
}
