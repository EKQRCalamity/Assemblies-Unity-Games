using System.Collections.Generic;

public class AGameStepMap : GameStep
{
	private LayoutOffset _layoutOffset;

	protected virtual IEnumerable<ACardLayout> _layoutsToOffset
	{
		get
		{
			yield return base.view.playerResourceDeckLayout.hand;
			yield return base.view.playerAbilityDeckLayout.hand;
			yield return base.view.playerResourceDeckLayout.activationHand;
			yield return base.view.playerResourceDeckLayout.select;
			yield return base.view.adventureDeckLayout.selectionHand;
			foreach (ACardLayout cardLayout in base.view.heroDeckLayout.selectionHandUnrestricted.GetCardLayouts())
			{
				yield return cardLayout;
			}
			yield return base.view.adventureDeckLayout.inspectHand;
		}
	}

	protected void _OffsetLayouts()
	{
		_layoutOffset = new LayoutOffset(_layoutsToOffset, base.view.mapDeckLayout.dragPlane.minZoomViewRect.GetPlane(PlaneAxes.XY));
	}

	protected void _ClearLayoutOffsets()
	{
		_layoutOffset?.ClearLayoutOffsets();
	}

	protected void _ResetLayouts()
	{
		_ClearLayoutOffsets();
		_OffsetLayouts();
	}
}
