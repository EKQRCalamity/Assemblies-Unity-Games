using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using UnityEngine;

namespace Tools.Items;

public class ItemGhostTrail : ObjectEffect
{
	[SerializeField]
	private Color color;

	private Color prevColor;

	private GhostTrailGenerator trail;

	private bool stopped;

	protected override bool OnApplyEffect()
	{
		if (Core.Logic.Penitent == null)
		{
			return false;
		}
		trail = Core.Logic.Penitent.GetComponentInChildren<GhostTrailGenerator>();
		prevColor = trail.TrailColor;
		stopped = false;
		return true;
	}

	protected override void OnUpdate()
	{
		if (!(trail == null) && !stopped && !Core.Logic.Penitent.IsOnExecution && !trail.EnableGhostTrail)
		{
			trail.EnableGhostTrail = true;
			trail.TrailColor = color;
		}
	}

	protected override void OnRemoveEffect()
	{
		if (!(trail == null))
		{
			trail.TrailColor = prevColor;
			trail.EnableGhostTrail = false;
			stopped = true;
		}
	}
}
