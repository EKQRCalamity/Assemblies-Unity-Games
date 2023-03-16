using System.Collections;
using UnityEngine;

public class TreePlatformingLevelButterfly : AbstractPausableComponent
{
	[SerializeField]
	private GameObject sprite1;

	[SerializeField]
	private GameObject sprite2;

	[SerializeField]
	private GameObject sprite3;

	private const float FRAME_TIME = 1f / 12f;

	private Vector2 velocity;

	private float rotation;

	private float frameTime;

	private int loopCounter;

	private int maxCounter;

	private MinMax velMinMax;

	public bool isActive { get; private set; }

	private void Start()
	{
		maxCounter = Random.Range(4, 7);
		if (sprite3.GetComponent<ParrySwitch>() != null)
		{
			sprite3.GetComponent<ParrySwitch>().OnActivate += Deactivate;
		}
	}

	public void Init(Vector2 velocity, float scale, int color, MinMax velMinMax)
	{
		isActive = true;
		base.transform.SetScale(scale);
		base.transform.SetEulerAngles(null, null, (!(scale < 0f)) ? base.transform.eulerAngles.z : (0f - base.transform.eulerAngles.z));
		this.velocity = velocity;
		this.velMinMax = velMinMax;
		SelectColor(color);
		Setup();
	}

	private void Setup()
	{
		string stateName = "P" + Random.Range(1, 5).ToStringInvariant();
		base.animator.Play(stateName);
		StartCoroutine(check_dist_cr());
		StartCoroutine(move_cr());
		StartCoroutine(switch_y_cr());
		StartCoroutine(adjust_x_speed(velMinMax));
	}

	public void Deactivate()
	{
		isActive = false;
		StopAllCoroutines();
		sprite1.SetActive(value: false);
		sprite2.SetActive(value: false);
		sprite3.SetActive(value: false);
	}

	private void SelectColor(int color)
	{
		sprite1.SetActive(value: false);
		sprite2.SetActive(value: false);
		sprite3.SetActive(value: false);
		switch (color)
		{
		case 1:
			sprite1.SetActive(value: true);
			break;
		case 2:
			sprite2.SetActive(value: true);
			break;
		case 3:
			sprite3.SetActive(value: true);
			break;
		}
	}

	private IEnumerator move_cr()
	{
		while (true)
		{
			frameTime += CupheadTime.Delta;
			if (frameTime > 1f / 12f)
			{
				frameTime -= 1f / 12f;
				Vector2 vector = base.transform.position;
				vector += velocity * CupheadTime.Delta;
				base.transform.position = vector;
			}
			yield return null;
		}
	}

	private IEnumerator switch_y_cr()
	{
		float time = Random.Range(1f, 2f);
		float t = 0f;
		float startVel = velocity.y;
		float endVel = 0f - velocity.y;
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, Random.Range(3f, 6f));
			while (t < time)
			{
				t += (float)CupheadTime.Delta;
				velocity.y = Mathf.Lerp(startVel, endVel, t / time);
				yield return null;
			}
			velocity.y = endVel;
			t = 0f;
			startVel = velocity.y;
			endVel = 0f - velocity.y;
			yield return null;
		}
	}

	private IEnumerator adjust_x_speed(MinMax adjustment)
	{
		float t = 0f;
		float time = Random.Range(1f, 2f);
		float startVel = velocity.x;
		float endVel = ((Mathf.Sign(velocity.x) != 1f) ? (0f - adjustment.RandomFloat()) : adjustment.RandomFloat());
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, Random.Range(4f, 6f));
			while (t < time)
			{
				velocity.x = Mathf.Lerp(startVel, endVel, time);
				yield return null;
			}
			velocity.x = endVel;
			endVel = ((Mathf.Sign(velocity.x) != 1f) ? (0f - adjustment.RandomFloat()) : adjustment.RandomFloat());
			startVel = velocity.x;
			t = 0f;
			yield return null;
		}
	}

	private IEnumerator check_dist_cr()
	{
		while (true)
		{
			float dist = Vector3.Distance(CupheadLevelCamera.Current.transform.position, base.transform.position);
			if (dist > 2000f)
			{
				break;
			}
			yield return null;
		}
		Deactivate();
		yield return null;
	}

	private void Counter()
	{
		if (loopCounter < maxCounter)
		{
			loopCounter++;
			return;
		}
		string stateName = "P" + Random.Range(1, 5).ToStringInvariant();
		base.animator.Play(stateName);
		maxCounter = Random.Range(4, 6);
		loopCounter = 0;
	}
}
