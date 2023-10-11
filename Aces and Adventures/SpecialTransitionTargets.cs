using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ACardLayout))]
public class SpecialTransitionTargets : MonoBehaviour
{
	public enum TargetType
	{
		Enter,
		Exit
	}

	public enum CombineType
	{
		Add,
		Override
	}

	public List<ACardLayout.Target> transitionTargets;

	public TargetType transitionType;

	public CombineType combineType;

	private ACardLayout _layout;

	private List<ACardLayout.Target> _overridenTargets;

	public ACardLayout layout => this.CacheComponent(ref _layout);

	private List<ACardLayout.Target> layoutTargets
	{
		get
		{
			if (transitionType != 0)
			{
				return layout.exitTargets;
			}
			return layout.enterTargets;
		}
	}

	public void Set(TargetType? transitionTypeOverride = null, CombineType? combineTypeOverride = null)
	{
		transitionType = transitionTypeOverride ?? transitionType;
		combineType = combineTypeOverride ?? combineType;
		switch (combineType)
		{
		case CombineType.Add:
		{
			foreach (ACardLayout.Target transitionTarget in transitionTargets)
			{
				layoutTargets.Add(transitionTarget);
			}
			break;
		}
		case CombineType.Override:
			_overridenTargets = layoutTargets;
			switch (transitionType)
			{
			case TargetType.Enter:
				layout.enterTargets = transitionTargets;
				break;
			case TargetType.Exit:
				layout.exitTargets = transitionTargets;
				break;
			}
			break;
		}
	}

	public void Restore()
	{
		switch (combineType)
		{
		case CombineType.Add:
		{
			for (int num = transitionTargets.Count - 1; num >= 0; num--)
			{
				layoutTargets.RemoveFromEnd(transitionTargets[num]);
			}
			break;
		}
		case CombineType.Override:
			switch (transitionType)
			{
			case TargetType.Enter:
				layout.enterTargets = _overridenTargets ?? layoutTargets;
				break;
			case TargetType.Exit:
				layout.exitTargets = _overridenTargets ?? layoutTargets;
				break;
			}
			_overridenTargets = null;
			break;
		}
	}
}
