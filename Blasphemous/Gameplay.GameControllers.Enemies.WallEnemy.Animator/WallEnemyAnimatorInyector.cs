using Gameplay.GameControllers.Entities.Animations;

namespace Gameplay.GameControllers.Enemies.WallEnemy.Animator;

public class WallEnemyAnimatorInyector : EnemyAnimatorInyector
{
	public void Attack()
	{
		WallEnemy wallEnemy = (WallEnemy)OwnerEntity;
		wallEnemy.Audio.PlayWoosh();
		wallEnemy.EntityAttack.CurrentWeaponAttack();
	}
}
