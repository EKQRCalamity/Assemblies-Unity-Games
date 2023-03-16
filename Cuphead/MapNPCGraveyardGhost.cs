using System.Collections;

public class MapNPCGraveyardGhost : MapDialogueInteraction
{
	private const int GRAVEYARD_GHOST_STATE_INDEX = 41;

	private float idleNormalizedTime;

	private int idleCycleCount;

	private int nextPuffMultiplier = 4;

	protected override bool ChangesDepth => false;

	protected override void Start()
	{
		base.Start();
		if (CharmCurse.IsMaxLevel(PlayerId.PlayerOne) || CharmCurse.IsMaxLevel(PlayerId.PlayerTwo))
		{
			Dialoguer.SetGlobalFloat(41, 2f);
		}
		else if (CharmCurse.CalculateLevel(PlayerId.PlayerOne) > -1 || CharmCurse.CalculateLevel(PlayerId.PlayerTwo) > -1)
		{
			Dialoguer.SetGlobalFloat(41, 1f);
		}
		else
		{
			Dialoguer.SetGlobalFloat(41, 0f);
		}
	}

	public void TalkAfterPlayerGotCharm()
	{
		StartCoroutine(got_charm_notification_cr());
	}

	private IEnumerator got_charm_notification_cr()
	{
		Dialoguer.SetGlobalFloat(41, 1f);
		while (Map.Current.players[0].state == MapPlayerController.State.Stationary || (Map.Current.players[1] != null && Map.Current.players[1].state == MapPlayerController.State.Stationary))
		{
			yield return null;
		}
		StartSpeechBubble();
		while (currentlySpeaking)
		{
			yield return null;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!base.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			return;
		}
		if (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f < idleNormalizedTime)
		{
			idleCycleCount++;
			if (idleCycleCount % 3 == 0)
			{
				base.animator.SetTrigger("Puff");
			}
			if (idleCycleCount % 7 == 3)
			{
				base.animator.SetTrigger("BlinkOnce");
			}
			if (idleCycleCount % 7 == 6)
			{
				base.animator.SetTrigger("BlinkTwice");
			}
		}
		idleNormalizedTime = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
	}
}
