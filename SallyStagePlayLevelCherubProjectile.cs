using System.Collections;
using UnityEngine;

public class SallyStagePlayLevelCherubProjectile : BasicProjectile
{
	private const float DIST_TO_LAUNCH = -400f;

	private const float OFFSET = 100f;

	[SerializeField]
	private GameObject cherub;

	[SerializeField]
	private SallyStagePlayLevelFirewheel firewheel;

	private Animator selfAnimator;

	public SallyStagePlayLevelCherubProjectile Create(Vector3 position, float rotation, float speed)
	{
		return base.Create(position, rotation, speed) as SallyStagePlayLevelCherubProjectile;
	}

	protected override void Start()
	{
		base.Start();
		selfAnimator = GetComponent<Animator>();
		AudioManager.Play("sally_cherub_vocal_wheelin");
		emitAudioFromObject.Add("sally_cherub_vocal_wheelin");
		StartCoroutine(wait_to_launch());
	}

	private IEnumerator wait_to_launch()
	{
		while (base.transform.position.x < -400f)
		{
			yield return null;
		}
		selfAnimator.SetTrigger("OnLaunch");
		yield return selfAnimator.WaitForAnimationToEnd(this, "Cherub_Push");
		StartCoroutine(cherub_leaves_cr());
	}

	private IEnumerator cherub_leaves_cr()
	{
		float time = 1f;
		float t = 0f;
		float start = cherub.transform.position.x;
		float end = -740f;
		while (t < time)
		{
			TransformExtensions.SetPosition(x: Mathf.Lerp(start, end, EaseUtils.Ease(EaseUtils.EaseType.easeInSine, 0f, 1f, t / time)), transform: cherub.transform);
			t += (float)CupheadTime.Delta;
			yield return new WaitForFixedUpdate();
		}
	}

	private void CherubPush()
	{
		move = false;
		StartCoroutine(launch_firewheel_cr());
	}

	private IEnumerator launch_firewheel_cr()
	{
		firewheel.PlaySound();
		while (firewheel.transform.position.x < 740f)
		{
			firewheel.transform.position += Vector3.right * Speed * CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
	}
}
