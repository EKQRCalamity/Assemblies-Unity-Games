using System;

public class GameStepGroupCanceler : GameStep
{
	public Func<bool> shouldCancel;

	public GroupType cancelGroupType;

	public GameStepGroupCanceler(Func<bool> shouldCancel, GroupType cancelGroupType = GroupType.Owning)
	{
		this.shouldCancel = shouldCancel;
		this.cancelGroupType = cancelGroupType;
	}

	public override void Start()
	{
		Func<bool> func = shouldCancel;
		if (func != null && func())
		{
			CancelGroup(cancelGroupType);
		}
	}
}
