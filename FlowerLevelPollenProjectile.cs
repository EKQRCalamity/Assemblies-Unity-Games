using System.Collections;
using UnityEngine;

public class FlowerLevelPollenProjectile : BasicProjectile
{
	private const float ROTATE_FRAME_TIME = 1f / 24f;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private FlowerLevelPollenPetal petalPink;

	[SerializeField]
	private FlowerLevelPollenPetal petal;

	private bool manual;

	private float time;

	private float speed;

	private float waveStrength;

	private float initPosY;

	private Transform target;

	private float pct;

	public void InitPollen(float speed, float strength, int type, Transform target)
	{
		pct = 0f;
		time = 0.7795515f;
		manual = true;
		this.speed = 0f - speed;
		waveStrength = strength;
		this.target = target;
		Speed = 0f;
		move = false;
		if (type == 1)
		{
			SetParryable(parryable: true);
			base.animator.Play("Pink_Idle");
		}
		StartCoroutine(move_cr());
		StartCoroutine(spawn_petals_cr(type));
	}

	public void StartMoving()
	{
		manual = false;
		move = true;
		Speed = speed;
		initPosY = base.transform.position.y;
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(rotate_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			if (CupheadTime.GlobalSpeed != 0f)
			{
				if (!manual)
				{
					Vector3 position = base.transform.position;
					position.y = initPosY + Mathf.Sin(time * 6f) * (waveStrength * pct) * CupheadTime.GlobalSpeed;
					base.transform.position = position;
					if (pct < 1f)
					{
						pct += CupheadTime.FixedDelta * 2f;
					}
					else
					{
						pct = 1f;
					}
				}
				else
				{
					base.transform.position = target.position;
					Speed = 0f;
				}
				time += CupheadTime.FixedDelta;
			}
			yield return wait;
		}
	}

	private IEnumerator rotate_cr()
	{
		float val = ((!Rand.Bool()) ? 420f : (-420f));
		float frameTime = 0f;
		while (true)
		{
			frameTime += (float)CupheadTime.Delta;
			if (frameTime > 1f / 24f)
			{
				frameTime -= 1f / 24f;
				sprite.transform.Rotate(0f, 0f, val * (float)CupheadTime.Delta);
			}
			yield return null;
		}
	}

	private IEnumerator spawn_petals_cr(int type)
	{
		while (true)
		{
			if (type == 1)
			{
				petalPink.Create(base.transform.position);
			}
			else
			{
				petal.Create(base.transform.position);
			}
			yield return CupheadTime.WaitForSeconds(this, Random.Range(0.2f, 1f));
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			CupheadRenderer instance = CupheadRenderer.Instance;
			instance.TouchFuzzy(15f, 8f, 1.2f);
			GetComponent<AudioWarble>().HandleWarble();
		}
	}

	protected override void Die()
	{
		base.Die();
		Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		petal = null;
		petalPink = null;
	}
}
