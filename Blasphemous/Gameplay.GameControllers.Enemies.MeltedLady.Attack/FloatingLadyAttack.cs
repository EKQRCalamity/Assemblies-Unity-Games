using Gameplay.GameControllers.Bosses.BurntFace.Rosary;
using Gameplay.GameControllers.Enemies.Framework.Attack;

namespace Gameplay.GameControllers.Enemies.MeltedLady.Attack;

public class FloatingLadyAttack : EnemyAttack
{
	public BurntFaceBeamAttack BeamAttack;

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)BeamAttack)
		{
			BeamAttack.SetDamage((int)base.EntityOwner.Stats.Strength.Final);
		}
	}
}
