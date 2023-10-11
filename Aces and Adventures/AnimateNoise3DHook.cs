using System.Collections.Generic;
using UnityEngine;

public class AnimateNoise3DHook : MonoBehaviour
{
	[SerializeField]
	protected bool _includeAnimatorsInChildren = true;

	[SerializeField]
	protected bool _clearAnimatorsOnDisable = true;

	private Dictionary<AnimateNoise3d, Vector2> _animators;

	public bool includeAnimatorsInChildren
	{
		get
		{
			return _includeAnimatorsInChildren;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _includeAnimatorsInChildren, value))
			{
				_animators = null;
			}
		}
	}

	public Dictionary<AnimateNoise3d, Vector2> animators => _animators ?? (_animators = (includeAnimatorsInChildren ? GetComponentsInChildren<AnimateNoise3d>(includeInactive: true) : GetComponents<AnimateNoise3d>()).ToDictionarySafe((AnimateNoise3d animator) => animator, (AnimateNoise3d animator) => new Vector2(animator.rangeMultiplier, animator.frequencyMultiplier)));

	private void OnDisable()
	{
		if (_clearAnimatorsOnDisable)
		{
			_animators = null;
		}
	}

	public void SetRangeMultipliers(float multiplier)
	{
		foreach (KeyValuePair<AnimateNoise3d, Vector2> animator in animators)
		{
			animator.Key.SetRangeMultiplier(animator.Value.x * multiplier);
		}
	}

	public void SetFrequencyMultipliers(float multiplier)
	{
		foreach (KeyValuePair<AnimateNoise3d, Vector2> animator in animators)
		{
			animator.Key.SetFrequencyMultiplier(animator.Value.y * multiplier);
		}
	}
}
