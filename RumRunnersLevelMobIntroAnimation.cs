using System.Collections;
using UnityEngine;

public class RumRunnersLevelMobIntroAnimation : MonoBehaviour
{
	private static readonly float IntroTimeoutDuration = 2f;

	[SerializeField]
	private Transform bugGirlTransform;

	[SerializeField]
	private float bugGirlWalkDistance;

	[SerializeField]
	private float bugGirlWalkDuration;

	[SerializeField]
	private Animator barrelAnimator;

	[SerializeField]
	private GameObject grub;

	private Coroutine bugWalkCoroutine;

	public float bugGirlDamage { get; set; }

	private void Start()
	{
		if (Level.Current.mode == Level.Mode.Easy)
		{
			grub.SetActive(value: false);
		}
	}

	private IEnumerator bugWalk()
	{
		float walkSpeed = bugGirlWalkDistance / bugGirlWalkDuration;
		while (true)
		{
			yield return null;
			bugGirlTransform.position += new Vector3(walkSpeed * (float)CupheadTime.Delta, 0f);
		}
	}

	private IEnumerator timeout_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, IntroTimeoutDuration);
		base.gameObject.SetActive(value: false);
	}

	public void StartBugWalk()
	{
		bugWalkCoroutine = StartCoroutine(bugWalk());
	}

	public void StopBugWalk()
	{
		StopCoroutine(bugWalkCoroutine);
	}

	public void BarrelExit()
	{
		barrelAnimator.SetTrigger("Exit");
		SpriteRenderer component = barrelAnimator.GetComponent<SpriteRenderer>();
		component.sortingLayerName = "Foreground";
		component.sortingOrder = 100;
		StartCoroutine(timeout_cr());
	}
}
