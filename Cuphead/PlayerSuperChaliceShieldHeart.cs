using System;
using UnityEngine;

public class PlayerSuperChaliceShieldHeart : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	public Transform player;

	private Vector3 offset;

	private float hoverTime = (float)Math.PI / 2f;

	private float hoverWidth = 100f;

	private bool popped;

	private float lerpSpeed;

	public void Destroy()
	{
		popped = true;
		animator.Play("HeartDie");
		AudioManager.Play("player_super_chalice_shield_end");
	}

	private void HeartDie()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void FixedUpdate()
	{
		if (!popped && player != null)
		{
			offset = new Vector3(hoverWidth * Mathf.Cos(hoverTime) / (1f + Mathf.Sin(hoverTime) * Mathf.Sin(hoverTime)), hoverWidth * Mathf.Sin(hoverTime) * Mathf.Cos(hoverTime) / (1f + Mathf.Sin(hoverTime) * Mathf.Sin(hoverTime)));
			hoverTime += CupheadTime.FixedDelta * 2f;
			lerpSpeed = Mathf.Min(lerpSpeed + CupheadTime.FixedDelta, 3f);
			base.transform.position = Vector3.Lerp(base.transform.position, player.transform.position + offset, CupheadTime.FixedDelta * lerpSpeed);
			base.transform.localScale = new Vector3(player.transform.localScale.x, 1f);
		}
	}
}
