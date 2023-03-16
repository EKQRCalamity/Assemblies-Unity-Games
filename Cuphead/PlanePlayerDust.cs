using System.Collections;
using UnityEngine;

public class PlanePlayerDust : AbstractMonoBehaviour
{
	private static readonly int SizeParameter = Animator.StringToHash("Size");

	private static readonly int ShadowLoopParameter = Animator.StringToHash("ShadowLoop");

	private static readonly int ManualState = Animator.StringToHash("Manual");

	private static readonly Vector3 PositionOffset = new Vector3(-70f, -20f);

	private static readonly int ManualShadowSpriteCount = 7;

	[SerializeField]
	private SpriteRenderer shadowRenderer;

	[SerializeField]
	private SpriteRenderer backRenderer;

	[SerializeField]
	private SpriteRenderer frontRenderer;

	private float smallY;

	private float bigY;

	private PlanePlayerController playerController;

	public void Initialize(AbstractPlayerController playerController, float smallY, float bigY)
	{
		this.playerController = (PlanePlayerController)playerController;
		this.smallY = smallY;
		this.bigY = bigY;
		if (playerController != null)
		{
			StartCoroutine(setupSorting_cr());
		}
	}

	private IEnumerator setupSorting_cr()
	{
		while (playerController.animationController.spriteRenderer == null)
		{
			yield return null;
		}
		int playerOrder = playerController.animationController.spriteRenderer.sortingOrder;
		shadowRenderer.sortingOrder += playerOrder;
		backRenderer.sortingOrder += playerOrder;
		frontRenderer.sortingOrder += playerOrder;
	}

	private void Update()
	{
		if (playerController == null)
		{
			return;
		}
		if (playerController.IsDead)
		{
			base.animator.SetInteger(SizeParameter, 0);
			base.animator.SetBool(ShadowLoopParameter, value: false);
			shadowRenderer.enabled = false;
			return;
		}
		float bottom = playerController.bottom;
		if (bottom < bigY)
		{
			base.animator.SetInteger(SizeParameter, 2);
			base.animator.SetBool(ShadowLoopParameter, value: true);
		}
		else if (bottom < smallY)
		{
			base.animator.SetInteger(SizeParameter, 1);
			setManualShadow(bottom);
		}
		else
		{
			base.animator.SetInteger(SizeParameter, 0);
			setManualShadow(bottom);
		}
		base.transform.position = new Vector3(playerController.center.x, bigY) + PositionOffset;
	}

	private void setManualShadow(float bottom)
	{
		base.animator.SetBool(ShadowLoopParameter, value: false);
		if (bottom >= smallY)
		{
			shadowRenderer.enabled = false;
			return;
		}
		shadowRenderer.enabled = true;
		float num = MathUtilities.LerpMapping(bottom, smallY, bigY, 0f, ManualShadowSpriteCount, clamp: true);
		base.animator.Play(ManualState, 2, num / (float)ManualShadowSpriteCount);
	}
}
