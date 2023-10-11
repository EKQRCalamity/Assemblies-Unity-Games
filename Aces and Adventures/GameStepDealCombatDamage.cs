using System.Collections;

public class GameStepDealCombatDamage : AGameStepCombat
{
	public float waitTime;

	protected override bool shouldBeCanceled => base.state.activeCombat?.canceled ?? true;

	public GameStepDealCombatDamage(float waitTime = 1f / 3f)
	{
		this.waitTime = waitTime;
	}

	protected override void OnAboutToEnableForFirstTime()
	{
		base.state.SignalAboutToProcessCombatDamageEarly();
	}

	protected override void OnFirstEnabled()
	{
		base.state.SignalAboutToProcessCombatDamage();
	}

	public override void Start()
	{
		base.state.SignalProcessCombatDamage();
	}

	protected override IEnumerator Update()
	{
		base.combat.damageHasBeenProcessed = true;
		if (base.combat.result != AttackResultType.Tie && (!base.attacker.statuses.safeAttack || base.combat.result != AttackResultType.Failure))
		{
			base.state.DealDamage((base.combat.result == AttackResultType.Success) ? new ActionContext(base.attacker, base.combat.ability, base.defender) : new ActionContext(base.defender, base.combat.ability, base.attacker), base.combat.totalDamage, (base.combat.result != AttackResultType.Success) ? DamageSource.Defense : DamageSource.Attack, base.combat.action);
		}
		if (!(waitTime > 0f))
		{
			yield break;
		}
		foreach (float item in Wait(waitTime))
		{
			_ = item;
			yield return null;
		}
	}

	public override void OnCompletedSuccessfully()
	{
		base.state.SignalOnCombatEnd();
	}
}
