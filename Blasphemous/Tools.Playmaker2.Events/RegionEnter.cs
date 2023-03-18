using HutongGames.PlayMaker;
using Tools.Level;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Event raised when an entity enters a region.")]
public class RegionEnter : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmGameObject region;

	public FsmBool listenOnlySelf;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		if (listenOnlySelf.Value)
		{
			Region.OnRegionEnter += ListenToSelf;
		}
		else
		{
			Region.OnRegionEnter += ListenToAll;
		}
	}

	private void ListenToAll(Region go)
	{
		region.Value = go.gameObject;
		base.Fsm.Event(onSuccess);
		Finish();
	}

	private void ListenToSelf(Region go)
	{
		Region componentInChildren = base.Owner.GetComponentInChildren<Region>();
		if (go.Equals(componentInChildren))
		{
			region.Value = go.gameObject;
			base.Fsm.Event(onSuccess);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (listenOnlySelf.Value)
		{
			Region.OnRegionEnter -= ListenToSelf;
		}
		else
		{
			Region.OnRegionEnter -= ListenToAll;
		}
	}
}
