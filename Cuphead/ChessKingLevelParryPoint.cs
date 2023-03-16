using System.Collections;
using UnityEngine;

public class ChessKingLevelParryPoint : ParrySwitch
{
	private Vector3 dir;

	private float amount;

	private float speed;

	public bool GOT_PARRIED { get; private set; }

	public bool IS_BLUE { get; private set; }

	protected override void Awake()
	{
		GOT_PARRIED = false;
		GetComponent<Collider2D>().enabled = false;
		GetComponent<SpriteRenderer>().color = Color.grey;
		base.Awake();
	}

	public void Init(Vector3 pos)
	{
		base.transform.position = pos;
		IS_BLUE = false;
		GOT_PARRIED = false;
	}

	public void Init(Vector3 pos, Vector3 dir, float speed, float amount)
	{
		GetComponent<SpriteRenderer>().color = Color.blue;
		base.transform.position = pos;
		this.dir = dir;
		this.amount = amount;
		this.speed = speed;
		IS_BLUE = true;
		GOT_PARRIED = false;
	}

	public void Activate()
	{
		GetComponent<Collider2D>().enabled = true;
		GetComponent<SpriteRenderer>().color = Color.magenta;
	}

	public override void OnParryPrePause(AbstractPlayerController player)
	{
		base.OnParryPrePause(player);
		GOT_PARRIED = true;
		GetComponent<Collider2D>().enabled = false;
		GetComponent<SpriteRenderer>().color = Color.grey;
	}

	public void MovePoint()
	{
		if (IS_BLUE)
		{
			StartCoroutine(move_cr());
		}
	}

	private IEnumerator move_cr()
	{
		Vector3 startPos = base.transform.position;
		Vector3 endPos = GetEndPos();
		YieldInstruction wait = new WaitForFixedUpdate();
		float t = 0f;
		float time = Vector3.Distance(startPos, endPos) / speed;
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			base.transform.position = Vector3.Lerp(startPos, endPos, t / time);
			yield return wait;
		}
		base.transform.position = endPos;
		yield return null;
	}

	private Vector3 GetEndPos()
	{
		if (dir == Vector3.right)
		{
			return new Vector3(base.transform.position.x + amount, base.transform.position.y);
		}
		if (dir == Vector3.left)
		{
			return new Vector3(base.transform.position.x - amount, base.transform.position.y);
		}
		if (dir == Vector3.up)
		{
			return new Vector3(base.transform.position.x, base.transform.position.y + amount);
		}
		return new Vector3(base.transform.position.x, base.transform.position.y - amount);
	}
}
