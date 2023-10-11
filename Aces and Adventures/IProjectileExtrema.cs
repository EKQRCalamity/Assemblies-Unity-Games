using UnityEngine;

public interface IProjectileExtrema
{
	Transform transform { get; }

	Transform GetTargetForProjectile(CardTarget cardTarget);
}
