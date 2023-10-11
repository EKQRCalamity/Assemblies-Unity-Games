using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public abstract class AnimateMaterialProperty : AnimateComponent
{
	[Header("Material Property")]
	public string propertyName;

	public bool getMaterialOnReset;

	protected bool propertyFound;

	protected Material material;

	protected abstract Type propertyType { get; }

	protected override void UniqueReset()
	{
		if (getMaterialOnReset)
		{
			material = GetComponent<Renderer>().material;
		}
	}

	public override void CacheInitialValues()
	{
		material = GetComponent<Renderer>().material;
		if (!string.IsNullOrEmpty(propertyName))
		{
			propertyFound = material.HasProperty(propertyName);
			if (propertyFound)
			{
				return;
			}
			if (propertyName[0] != '_')
			{
				propertyName = "_" + propertyName;
				propertyFound = material.HasProperty(propertyName);
			}
		}
		_ = propertyFound;
	}
}
