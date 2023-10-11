using System.Runtime.CompilerServices;
using UnityEngine;

public class CardTargetTransforms : MonoBehaviour, IProjectileExtrema
{
	public Transform cost;

	public Transform nameTransform;

	public Transform image;

	public Transform center;

	public Transform description;

	public Transform offense;

	public Transform hp;

	public Transform defense;

	public Transform this[CardTarget target] => target switch
	{
		CardTarget.Cost => cost, 
		CardTarget.Name => nameTransform, 
		CardTarget.ImageCenter => image, 
		CardTarget.Center => center, 
		CardTarget.Description => description, 
		CardTarget.Offense => offense, 
		CardTarget.HP => hp, 
		CardTarget.Defense => defense, 
		_ => center, 
	};

	public Transform GetTargetForProjectile(CardTarget cardTarget)
	{
		return this[cardTarget];
	}

	[SpecialName]
	Transform IProjectileExtrema.get_transform()
	{
		return base.transform;
	}
}
