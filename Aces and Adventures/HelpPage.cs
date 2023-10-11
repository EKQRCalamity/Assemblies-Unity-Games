using System;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HelpPage : MonoBehaviour, IComparable<HelpPage>
{
	private RectTransform _rect;

	public RectTransform rect => this.CacheComponent(ref _rect);

	public virtual int priority => 1;

	public int CompareTo(HelpPage other)
	{
		return other.priority - priority;
	}
}
