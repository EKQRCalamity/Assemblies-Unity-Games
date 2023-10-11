using UnityEngine;

public class TurnOrderSpaceView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/TurnOrderSpaceView";

	[Header("Turn Order Space")]
	public BoolEvent onTappedChange;

	public TurnOrderSpace turnOrderSpace
	{
		get
		{
			return (TurnOrderSpace)base.target;
		}
		set
		{
			base.target = value;
		}
	}

	protected override bool _includeInactiveInInputColliderSearch => true;

	public static TurnOrderSpaceView Create(TurnOrderSpace space, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<TurnOrderSpaceView>()._SetData(space);
	}

	private void _OnTappedChange(bool tapped)
	{
		onTappedChange?.Invoke(tapped);
	}

	private TurnOrderSpaceView _SetData(TurnOrderSpace space)
	{
		turnOrderSpace = space;
		return this;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (oldTarget is TurnOrderSpace turnOrderSpace)
		{
			turnOrderSpace.tapped.onValueChanged -= _OnTappedChange;
		}
		if (newTarget != null)
		{
			_OnTappedChange(this.turnOrderSpace.tapped);
			this.turnOrderSpace.tapped.onValueChanged += _OnTappedChange;
		}
	}
}
