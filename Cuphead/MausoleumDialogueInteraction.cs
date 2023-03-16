using System.Collections;
using UnityEngine;

public class MausoleumDialogueInteraction : DialogueInteractionPoint
{
	public Animator chaliceAnimator;

	public void BeginDialogue()
	{
		Activate();
		chaliceAnimator.SetBool("Talking", value: true);
		speechBubble.waitForRealease = false;
	}

	protected override void Start()
	{
		base.Start();
		Dialoguer.events.onTextPhase += OnDialogueTextSound;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Dialoguer.events.onTextPhase -= OnDialogueTextSound;
	}

	private void OnDialogueTextSound(DialoguerTextData data)
	{
		if (!string.IsNullOrEmpty("mausoleum_queen_ghost_speech"))
		{
			AudioManager.Stop("mausoleum_queen_ghost_speech");
		}
		AudioManager.Play("mausoleum_queen_ghost_speech");
	}

	protected override IEnumerator ReactivateInputsCoroutine(LevelPlayerMotor playerOneMotor, LevelPlayerMotor playerTwoMotor, LevelPlayerWeaponManager playerOneWeaponManager, LevelPlayerWeaponManager playerTwoWeaponManager, Animator animator)
	{
		speechBubble.preventQuit = true;
		AbstractPlayerController playercontroller = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		if (playercontroller != null && playercontroller.animator != null && playercontroller.animator.GetCurrentAnimatorStateInfo(0).IsName("Power_Up"))
		{
			yield return playercontroller.animator.WaitForAnimationToEnd(this, "Power_Up");
		}
		speechBubble.preventQuit = false;
		yield return StartCoroutine(base.ReactivateInputsCoroutine(playerOneMotor, playerTwoMotor, playerOneWeaponManager, playerTwoWeaponManager, animator));
		chaliceAnimator.SetBool("Talking", value: false);
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		SceneLoader.LoadLastMap();
	}
}
