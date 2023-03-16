using System;
using System.Collections;
using UnityEngine;

public class LevelPlayerParryController : AbstractLevelPlayerComponent, IParryAttack
{
	public enum ParryState
	{
		Init,
		Ready,
		Parrying
	}

	public const float DURATION = 0.2f;

	private ParryState state;

	[SerializeField]
	private LevelPlayerParryEffect effect;

	public ParryState State => state;

	public bool AttackParryUsed { get; set; }

	public bool HasHitEnemy { get; set; }

	public event Action OnParryStartEvent;

	public event Action OnParryEndEvent;

	private void Start()
	{
		base.player.motor.OnParryEvent += StartParry;
		base.player.motor.OnGroundedEvent += OnGround;
	}

	private void OnGround()
	{
		AttackParryUsed = false;
	}

	public override void OnLevelStart()
	{
		base.OnLevelStart();
		state = ParryState.Ready;
	}

	private void StartParry()
	{
		state = ParryState.Parrying;
		if (this.OnParryStartEvent != null)
		{
			this.OnParryStartEvent();
		}
		StartCoroutine(parry_cr());
	}

	private IEnumerator parry_cr()
	{
		effect.Create(base.player);
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		state = ParryState.Ready;
		if (this.OnParryEndEvent != null)
		{
			this.OnParryEndEvent();
		}
		HasHitEnemy = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		effect = null;
	}
}
