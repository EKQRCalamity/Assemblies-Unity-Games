using System.Collections;
using UnityEngine;

public class DicePalaceDominoLevelFloorTile : DicePalaceDominoLevelBaseTile
{
	private SpriteRenderer spriteRenderer;

	private DamageDealer damageDealer;

	private BoxCollider2D boxCollider;

	public bool spikesActive { get; private set; }

	protected override void Awake()
	{
		damageDealer = DamageDealer.NewEnemy();
		boxCollider = GetComponent<BoxCollider2D>();
		base.Awake();
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public override void InitTile()
	{
		base.InitTile();
		OnMoveStart();
	}

	public void SetColour(int colourIndex, LevelProperties.DicePalaceDomino properties)
	{
		base.properties = properties;
		base.currentColourIndex = colourIndex;
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = colours[base.currentColourIndex];
	}

	public void OnMoveStart()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		yield return null;
		while (base.isActivated)
		{
			base.transform.position += Vector3.left * properties.CurrentState.domino.floorSpeed * CupheadTime.Delta;
			if (base.transform.position.x + 200f < (float)Level.Current.Left)
			{
				DeactivateTile();
			}
			yield return null;
		}
	}

	public void TriggerSpikes(bool spikesActive)
	{
		StartCoroutine(toggleSpikes_cr(spikesActive));
	}

	public override void DeactivateTile()
	{
		base.DeactivateTile();
		toggleSpikes_cr(spikesActive: false);
	}

	private IEnumerator toggleSpikes_cr(bool spikesActive)
	{
		if (spikesActive)
		{
			base.animator.Play("Spikes_Up");
			this.spikesActive = true;
			boxCollider.enabled = true;
		}
		else
		{
			if (this.spikesActive)
			{
				base.animator.Play("Spikes_Down");
				StartCoroutine(disableCollider_cr());
			}
			else
			{
				base.animator.Play("Off");
				boxCollider.enabled = false;
			}
			this.spikesActive = false;
		}
		yield return null;
	}

	private IEnumerator disableCollider_cr()
	{
		yield return base.animator.WaitForAnimationToEnd(this, "Spikes_Down", waitForEndOfFrame: true);
		boxCollider.enabled = false;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
