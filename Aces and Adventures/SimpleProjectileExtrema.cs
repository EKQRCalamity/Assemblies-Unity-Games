using System.Runtime.CompilerServices;
using UnityEngine;

public class SimpleProjectileExtrema : MonoBehaviour, IProjectileExtrema
{
	public Transform GetTargetForProjectile(CardTarget cardTarget)
	{
		return base.transform;
	}

	[SpecialName]
	Transform IProjectileExtrema.get_transform()
	{
		return base.transform;
	}
}
