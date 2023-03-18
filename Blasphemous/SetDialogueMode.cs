using Framework.Managers;
using HutongGames.PlayMaker;
using UnityEngine;

public class SetDialogueMode : FsmStateAction
{
	public FsmBool IsDialogueMode;

	public FsmBool PlaySheathedAnimDirectly;

	private readonly int _sheathedAnim = Animator.StringToHash("SheathedToIdle");

	public override void OnEnter()
	{
		base.OnEnter();
		if (PlaySheathedAnimDirectly.Value)
		{
			SetSheathedDirectly();
		}
		else
		{
			Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", IsDialogueMode.Value);
		}
		Finish();
	}

	private void SetSheathedDirectly()
	{
		Core.Logic.Penitent.Animator.Play(_sheathedAnim);
		Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", value: true);
	}
}
