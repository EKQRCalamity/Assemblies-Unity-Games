using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BezierSplineEvent : UnityEvent<Matrix4x4, BezierSpline>
{
}
