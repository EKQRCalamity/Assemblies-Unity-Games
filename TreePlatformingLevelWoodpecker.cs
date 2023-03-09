using System.Collections;
using UnityEngine;

public class TreePlatformingLevelWoodpecker : PlatformingLevelShootingEnemy
{
	[SerializeField]
	private Transform setEndPos;

	private Vector2 endPos;

	private Vector2 midPos;

	private Vector2 startPos;

	private bool isDown;

	protected override void Start()
	{
		base.Start();
		isDown = false;
		startPos = base.transform.position;
		endPos = setEndPos.transform.position;
		midPos = new Vector3(endPos.x, endPos.y + 200f);
		GetComponent<DamageReceiver>().enabled = false;
	}

	protected override void Shoot()
	{
		if (!isDown)
		{
			StartCoroutine(move_down_cr());
		}
	}

	private IEnumerator move_down_cr()
	{
		isDown = true;
		base.animator.SetBool("movingDown", value: true);
		float t3 = 0f;
		Vector2 start3 = base.transform.position;
		while (t3 < base.Properties.WoodpeckermoveDownTime)
		{
			float val = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, t3 / base.Properties.WoodpeckermoveDownTime);
			base.transform.position = Vector2.Lerp(start3, midPos, val);
			t3 += (float)CupheadTime.Delta;
			yield return null;
		}
		base.transform.position = midPos;
		start3 = base.transform.position;
		yield return CupheadTime.WaitForSeconds(this, base.Properties.WoodpeckerWarningDuration);
		base.animator.SetTrigger("Continue");
		t3 = 0f;
		while (t3 < 0.2f)
		{
			float val2 = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t3 / 0.5f);
			base.transform.position = Vector2.Lerp(start3, endPos, val2);
			t3 += (float)CupheadTime.Delta;
			yield return null;
		}
		t3 = 0f;
		base.transform.position = endPos;
		start3 = base.transform.position;
		base.animator.SetBool("isAttacking", value: true);
		CupheadLevelCamera.Current.Shake(10f, base.Properties.WoodpeckerAttackDuration);
		yield return CupheadTime.WaitForSeconds(this, base.Properties.WoodpeckerAttackDuration);
		base.animator.SetBool("isAttacking", value: false);
		while (t3 < base.Properties.WoodpeckermoveUpTime)
		{
			float val3 = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, t3 / base.Properties.WoodpeckermoveUpTime);
			base.transform.position = Vector2.Lerp(start3, startPos, val3);
			t3 += (float)CupheadTime.Delta;
			yield return null;
		}
		base.animator.SetBool("movingDown", value: false);
		isDown = false;
		yield return null;
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = new Color(0f, 1f, 0f, 1f);
		Gizmos.DrawWireSphere(endPos, 100f);
	}

	private void SoundWoodpeckerStart()
	{
		AudioManager.Play("level_platform_woodpecker_attack_start");
		emitAudioFromObject.Add("level_platform_woodpecker_attack_start");
	}
}
