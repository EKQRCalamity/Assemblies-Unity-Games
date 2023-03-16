using System.Collections;
using UnityEngine;

public class KitchenSaltbakerCounter : DialogueInteractionPoint
{
	private const int DIALOGUER_VAR_ID = 23;

	[SerializeField]
	private MinMax blinkRange = new MinMax(2.5f, 4.5f);

	private float blinkTimer;

	protected override void Start()
	{
		base.Start();
		Dialoguer.events.onTextPhase += onDialogueAdvancedHandler;
		Dialoguer.events.onEnded += onDialogueEndedHandler;
		if (Dialoguer.GetGlobalFloat(23) == 0f)
		{
			StartCoroutine(dialogue_on_first_visit_cr());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Dialoguer.events.onTextPhase -= onDialogueAdvancedHandler;
		Dialoguer.events.onEnded -= onDialogueEndedHandler;
	}

	private void onDialogueAdvancedHandler(DialoguerTextData data)
	{
		if (!base.animator.GetCurrentAnimatorStateInfo(0).IsName("Talk"))
		{
			base.animator.SetTrigger("Talk");
		}
	}

	private void onDialogueEndedHandler()
	{
		base.animator.SetBool("PlayerClose", value: false);
	}

	protected override void Activate()
	{
		base.animator.SetBool("PlayerClose", value: true);
		base.animator.SetTrigger("Talk");
		base.Activate();
	}

	private IEnumerator dialogue_on_first_visit_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1.5f);
		speechBubble.waitForRealease = false;
		Activate();
		Hide(PlayerId.PlayerOne);
		if (PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null)
		{
			Hide(PlayerId.PlayerTwo);
		}
	}

	private void Update()
	{
		base.animator.SetBool("PlayerClose", conversationIsActive);
		blinkTimer -= CupheadTime.Delta;
		if (blinkTimer < 0f)
		{
			blinkTimer = blinkRange.RandomFloat();
			base.animator.SetTrigger("Blink");
		}
	}
}
