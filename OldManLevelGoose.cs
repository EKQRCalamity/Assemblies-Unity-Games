using System.Collections;
using UnityEngine;

public class OldManLevelGoose : AbstractProjectile
{
	private const float OFFSET = 1000f;

	private LevelProperties.OldMan.GooseAttack properties;

	private float speed;

	[SerializeField]
	private BoxCollider2D coll;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private Material altMaterial;

	public virtual OldManLevelGoose Init(Vector2 pos, float speed, LevelProperties.OldMan.GooseAttack properties, bool hasCollision, string sortingLayer, int sortingOrder, float whiten)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		this.properties = properties;
		this.speed = speed;
		coll.enabled = hasCollision;
		rend.sortingLayerName = sortingLayer;
		rend.color = new Color(whiten, whiten, whiten);
		if (sortingLayer == "Foreground")
		{
			rend.material = altMaterial;
			base.gameObject.layer = 31;
		}
		rend.sortingOrder = sortingOrder;
		anim.Play((Random.Range(0, 8) % 6).ToString());
		Move();
		return this;
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void Move()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		while (base.transform.position.x > (float)Level.Current.Left - 1000f)
		{
			base.transform.position += Vector3.left * speed * CupheadTime.FixedDelta;
			yield return wait;
		}
		this.Recycle();
		yield return null;
	}
}
