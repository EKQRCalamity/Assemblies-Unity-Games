using System.Collections;
using UnityEngine;

public class DevilLevelPlatform : AbstractPausableComponent
{
	public enum PlatformType
	{
		A,
		B,
		C,
		D,
		E
	}

	public enum State
	{
		Idle,
		Raising,
		Lowering,
		Dead
	}

	private const float LOWER_DISTANCE = 300f;

	public PlatformType type;

	public State state;

	private float baseY;

	protected override void Awake()
	{
		base.Awake();
		base.animator.Play("Platform_" + type);
		baseY = base.transform.position.y;
	}

	public void Raise(float speed, float height, float delay)
	{
		state = State.Raising;
		StartCoroutine(raise_cr(speed, height, delay));
	}

	private IEnumerator raise_cr(float speed, float height, float delay)
	{
		float t = 0f;
		float moveTime = height / speed;
		while (t < moveTime)
		{
			base.transform.SetPosition(null, EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, baseY, baseY + height, t / moveTime));
			t += CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
		base.transform.SetPosition(null, baseY + height);
		yield return CupheadTime.WaitForSeconds(this, delay);
		t = 0f;
		while (t < moveTime)
		{
			base.transform.SetPosition(null, EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, baseY + height, baseY, t / moveTime));
			t += CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
		base.transform.SetPosition(null, baseY);
		state = State.Idle;
	}

	public void Lower(float speed)
	{
		state = State.Lowering;
		StartCoroutine(lower_cr(speed));
	}

	private IEnumerator lower_cr(float speed)
	{
		float t = 0f;
		float moveTime = 300f / speed;
		while (t < moveTime)
		{
			base.transform.SetPosition(null, EaseUtils.Ease(EaseUtils.EaseType.easeInSine, baseY, baseY - 300f, t / moveTime));
			t += CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
		Die();
	}

	public void Die()
	{
		AbstractPlayerController[] componentsInChildren = GetComponentsInChildren<AbstractPlayerController>();
		foreach (AbstractPlayerController abstractPlayerController in componentsInChildren)
		{
			if (!(abstractPlayerController == null))
			{
				abstractPlayerController.transform.parent = null;
			}
		}
		base.gameObject.SetActive(value: false);
		state = State.Dead;
	}
}
