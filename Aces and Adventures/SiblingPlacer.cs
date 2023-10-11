using System;
using UnityEngine;

public class SiblingPlacer : MonoBehaviour
{
	[Tooltip("Should [Index Distance] be relative to end of sibling list?")]
	public bool distanceRelativeToEnd = true;

	public int indexDistance;

	private int? _previousChildCount;

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		Transform parent = base.transform.parent;
		if (!(parent == null))
		{
			int childCount = parent.childCount;
			if (!_previousChildCount.HasValue || _previousChildCount.Value != childCount)
			{
				_previousChildCount = childCount;
				_OnSiblingCountChanged();
			}
		}
	}

	private void _OnSiblingCountChanged()
	{
		int value = _previousChildCount.Value;
		int siblingIndex = (distanceRelativeToEnd ? Math.Max(0, value - 1 - indexDistance) : Math.Min(indexDistance, value - 1));
		base.transform.SetSiblingIndex(siblingIndex);
	}
}
