public class AGameStepCombat : GameStep
{
	protected ActiveCombat combat => base.state.activeCombat;

	protected ACombatant attacker => combat.attacker;

	protected ACombatant defender => combat.defender;

	protected override bool shouldBeCanceled => base.state.combatShouldBeCanceled;
}
