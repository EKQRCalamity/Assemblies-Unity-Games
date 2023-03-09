using System.Collections;
using UnityEngine;

public class TrainLevelPlatform : LevelPlatform
{
	public enum CartPosition
	{
		Left,
		Middle,
		Right
	}

	private const float DISTANCE = 390f;

	[SerializeField]
	private ParrySwitch rightSwitch;

	[SerializeField]
	private ParrySwitch leftSwitch;

	[SerializeField]
	private Transform[] sparkRoots;

	[SerializeField]
	private Effect sparkEffectPrefab;

	private CartPosition position;

	private AnimationHelper animHelper;

	private SpriteRenderer spriteRenderer;

	private float middlePos;

	private float leftPos;

	private float rightPos;

	private bool isMoving;

	protected override void Awake()
	{
		base.Awake();
		animHelper = GetComponent<AnimationHelper>();
		middlePos = base.transform.position.x + 390f;
		leftPos = base.transform.position.x;
		rightPos = base.transform.position.x + 780f;
		position = CartPosition.Left;
		rightSwitch.OnActivate += OnRight;
		leftSwitch.OnActivate += OnLeft;
		StartCoroutine(spark_cr());
	}

	private void OnLeft()
	{
		if (!isMoving)
		{
			AudioManager.Play("train_hand_car_valves_spin");
			emitAudioFromObject.Add("train_hand_car_valves_spin");
			position = ((position == CartPosition.Right) ? CartPosition.Middle : CartPosition.Left);
			Move(SelectPosition());
		}
	}

	private void OnRight()
	{
		if (!isMoving)
		{
			AudioManager.Play("train_hand_car_valves_spin");
			emitAudioFromObject.Add("train_hand_car_valves_spin");
			position = ((position == CartPosition.Left) ? CartPosition.Middle : CartPosition.Right);
			Move(SelectPosition());
		}
	}

	private float SelectPosition()
	{
		float result = 0f;
		switch (position)
		{
		case CartPosition.Left:
			result = leftPos;
			break;
		case CartPosition.Right:
			result = rightPos;
			break;
		case CartPosition.Middle:
			result = middlePos;
			break;
		}
		return result;
	}

	private void Move(float x)
	{
		StartCoroutine(move_cr(x));
	}

	private IEnumerator move_cr(float x)
	{
		isMoving = true;
		rightSwitch.gameObject.SetActive(value: false);
		leftSwitch.gameObject.SetActive(value: false);
		base.animator.SetTrigger("OnSlap");
		base.animator.SetBool("Spinning", value: true);
		base.animator.SetBool("Effect", value: false);
		animHelper.Speed = 1f;
		float t = 0f;
		float time = 1.5f;
		float startX = base.transform.position.x;
		base.transform.SetPosition(startX);
		yield return null;
		while (t < time)
		{
			t += (float)CupheadTime.Delta;
			float val = t / time;
			base.transform.SetPosition(EaseUtils.Ease(EaseUtils.EaseType.easeOutCubic, startX, x, val));
			if (val > 0.5f)
			{
				animHelper.Speed = 0.5f;
			}
			yield return null;
		}
		base.transform.SetPosition(x);
		isMoving = false;
	}

	private void FadeIn()
	{
		rightSwitch.gameObject.SetActive(value: true);
		leftSwitch.gameObject.SetActive(value: true);
		animHelper.Speed = 1f;
		base.animator.SetTrigger("OnContinue");
		base.animator.SetBool("Effect", value: true);
	}

	private IEnumerator spark_cr()
	{
		while (true)
		{
			if (!leftSwitch.isActiveAndEnabled)
			{
				yield return null;
				continue;
			}
			yield return CupheadTime.WaitForSeconds(this, Random.Range(0.5f, 1f));
			sparkEffectPrefab.Create(sparkRoots.RandomChoice().position);
			yield return CupheadTime.WaitForSeconds(this, 1f / 3f);
		}
	}
}
