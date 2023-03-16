using UnityEngine;

public class AirplaneLevelLeaderAnimation : MonoBehaviour
{
	[SerializeField]
	private Animator bulldogAnimation;

	private Vector3 rootPosition;

	private float wobbleTimer;

	[SerializeField]
	private float wobbleX = 10f;

	[SerializeField]
	private float wobbleY = 10f;

	[SerializeField]
	private float wobbleSpeed = 1f;

	private void Start()
	{
		rootPosition = base.transform.position;
	}

	private void AniEvent_StartBulldog()
	{
		bulldogAnimation.SetTrigger("Continue");
	}

	private void AniEvent_SFX_LeaderBark()
	{
		AudioManager.Play("sfx_dlc_dogfight_leadervocal_introbark");
	}

	private void Update()
	{
		base.transform.position = rootPosition + Mathf.Sin(wobbleTimer * 3f) * wobbleX * Vector3.right + Mathf.Sin(wobbleTimer * 2f) * wobbleY * Vector3.up;
		wobbleTimer += (float)CupheadTime.Delta * wobbleSpeed;
	}

	private void AnimationEvent_SFX_DOGFIGHT_Intro_LeaderCopterFlyby()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_leader_copterflybyexit");
	}
}
