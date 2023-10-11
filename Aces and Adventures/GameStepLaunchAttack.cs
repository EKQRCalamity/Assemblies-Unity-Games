public class GameStepLaunchAttack : AGameStepCombat
{
	public override void Start()
	{
		if (!base.defender.isTakingTurn)
		{
			base.defender.view.offsets.Add(AGameStepTurn.OFFSET);
			if (base.defender is Enemy)
			{
				VoiceManager.Instance.Play(base.defender.view.transform, base.defender.audio.grunt, interrupt: true);
			}
		}
		if (!base.attacker.CanFormAttack(base.attacker.resourceDeck.GetCards(ResourceCard.Pile.AttackHand), base.defender, base.attacker.resourceDeck.GetCards(ResourceCard.Pile.AttackHand)))
		{
			base.attacker.GetAttackCombatHand(ResourceCard.Pile.AttackHand, base.defender).WildIntoPokerHand();
		}
		base.state.SignalAttackLaunched();
	}

	protected override void End()
	{
		AppendStep(base.defender.GetDefenseStep());
	}

	protected override void OnDestroy()
	{
		if (!base.defender.isTakingTurn)
		{
			base.defender.view.offsets.Remove(AGameStepTurn.OFFSET);
		}
	}
}
