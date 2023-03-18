using HutongGames.PlayMaker;
using Tools.Level;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Entity raised when an entity exists a region.")]
public class RegionExit : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmGameObject region;

	public FsmBool listenOnlySelf;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		if (listenOnlySelf.Value)
		{
			Region.OnRegionExit += ListenToSelf;
		}
		else
		{
			Region.OnRegionExit += ListenToAll;
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
			Region.OnRegionExit -= ListenToSelf;
		}
		else
		{
			Region.OnRegionExit -= ListenToAll;
		}
	}
}
