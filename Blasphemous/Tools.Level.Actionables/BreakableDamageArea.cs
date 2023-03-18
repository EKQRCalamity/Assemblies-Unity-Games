using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Tools.Level.Actionables;

public class BreakableDamageArea : DamageArea
{
	[SerializeField]
	protected bool grantsFervour = true;

	public bool GrantsFervour => grantsFervour;

	protected override void OnAwake()
	{
	}
}
