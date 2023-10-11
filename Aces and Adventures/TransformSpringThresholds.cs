using System;
using UnityEngine;

[Serializable]
public class TransformSpringThresholds
{
	public bool usePerAxisDistance;

	[Range(0f, 100f, order = 2)]
	[HideInInspectorIf("_hideDistance", false, order = 1)]
	public float distance = 0.01f;

	[HideInInspectorIf("_hidePerAxisDistance", false)]
	public Vector3 perAxisDistance = new Vector3(0.01f, 0.01f, 0.01f);

	[Range(0f, 180f)]
	public float degrees = 1f;

	[Range(0f, 100f)]
	public float scale = 0.01f;

	public bool useScaleThreshold;

	private bool _hideDistance => usePerAxisDistance;

	private bool _hidePerAxisDistance => !usePerAxisDistance;
}
