using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public abstract class UILayoutAttribute : Attribute
{
	public string name { get; set; }

	public abstract GameObject CreateLayoutObject(Transform parent);

	public abstract void SetLayoutElementData(GameObject gameObject);
}
