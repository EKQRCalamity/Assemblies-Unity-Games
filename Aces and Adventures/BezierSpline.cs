using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BezierSpline
{
	private List<Vector3> _points;

	private List<float> _curveLengths;

	private List<BoundingSphere> _boundingSpheres;

	private List<Bounds> _boundingBoxes;

	private float? _arcLength;

	public List<Vector3> points => _points;

	public List<float> curveLengths
	{
		get
		{
			if (_curveLengths == null)
			{
				Pools.TryUnpool(ref _curveLengths);
				for (int i = 0; i < _points.Count - 1; i += 2)
				{
					_curveLengths.Add(BezierUtil.ArcLength(_points[i], _points[i + 1], _points[i + 2]));
				}
			}
			return _curveLengths;
		}
	}

	public List<BoundingSphere> boundingSpheres
	{
		get
		{
			if (_boundingSpheres == null)
			{
				Pools.TryUnpool(ref _boundingSpheres);
				PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(3);
				for (int i = 0; i < _points.Count - 1; i += 2)
				{
					_boundingSpheres.Add(MathUtil.BoundingSphereFromPoints(_SetExtremaPoints(poolStructArrayHandle, i)));
				}
			}
			return _boundingSpheres;
		}
	}

	public List<Bounds> boundingBoxes
	{
		get
		{
			if (_boundingBoxes == null)
			{
				Pools.TryUnpool(ref _boundingBoxes);
				PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(3);
				for (int i = 0; i < _points.Count - 1; i += 2)
				{
					_boundingBoxes.Add(MathUtil.BoundsFromPoints(_SetExtremaPoints(poolStructArrayHandle, i)));
				}
			}
			return _boundingBoxes;
		}
	}

	public float arcLength
	{
		get
		{
			if (!_arcLength.HasValue)
			{
				_arcLength = 0f;
				ListEnumerator<float>.Enumerator enumerator = curveLengths.Enumerate().GetEnumerator();
				while (enumerator.MoveNext())
				{
					_arcLength += enumerator.Current;
				}
			}
			return _arcLength.Value;
		}
	}

	public int curveCount => (_points.Count + 1) / 2 - 1;

	public void OnUnpool()
	{
		Pools.TryUnpool(ref _points);
	}

	public void Clear()
	{
		Pools.TryRepool(ref _points);
		_SetDirty();
	}

	private void _SetDirty()
	{
		Pools.TryRepool(ref _curveLengths);
		Pools.TryRepool(ref _boundingSpheres);
		Pools.TryRepool(ref _boundingBoxes);
		_arcLength = null;
	}

	private void _GetCurveIndexAndTNearestToPoint(Vector3 point, out int curveIndex, out float t)
	{
		curveIndex = 0;
		t = 0f;
		List<Bounds> list = boundingBoxes;
		float num = float.MaxValue;
		int num2 = 0;
		for (int i = 0; i < _points.Count - 1; i += 2)
		{
			if (list[num2].DistanceToPoint(point) >= num)
			{
				num2++;
				continue;
			}
			Vector3 p = _points[i];
			Vector3 p2 = _points[i + 1];
			Vector3 p3 = _points[i + 2];
			float tNearestToPoint = BezierUtil.GetTNearestToPoint(p, p2, p3, point);
			float magnitude = (point - BezierUtil.GetPosition(p, p2, p3, tNearestToPoint)).magnitude;
			if (magnitude < num)
			{
				curveIndex = num2;
				t = tNearestToPoint;
				num = magnitude;
			}
			num2++;
		}
	}

	private int _GetCurveStartPointIndex(int curveIndex)
	{
		return curveIndex + curveIndex;
	}

	private void _GetCurveIndexAndTAtNormalizedT(float splineT, out int curveIndex, out float t)
	{
		int num = curveCount;
		float f = splineT * (float)num;
		float num2 = MathUtil.Frac(f);
		int num3 = Mathf.FloorToInt(f);
		curveIndex = Mathf.Clamp(num3, 0, num - 1);
		t = ((num3 <= curveIndex) ? num2 : 1f);
	}

	private float _GetNormalizedTFromCurveIndexAndT(int curveIndex, float t)
	{
		return ((float)curveIndex + t) / (float)curveCount;
	}

	private bool _MoveForwardAtSpeed(float speed, ref int curveIndex, ref float t, Action<Vector3, Vector3> onMove = null)
	{
		bool result = false;
		int num = _GetCurveStartPointIndex(curveIndex);
		float num2 = BezierUtil.GetSpeed(_points, t, num).InsureNonZero();
		int curveIndex2 = curveIndex;
		float t2 = t;
		t += speed / num2;
		while (t > 1f)
		{
			curveIndex++;
			num += 2;
			float num3 = MathUtil.Frac(t);
			t -= 1f;
			t -= num3;
			if (num < _points.Count - 2)
			{
				num3 *= num2 / BezierUtil.GetSpeed(_points, 0f, num).InsureNonZero();
				t += num3;
				continue;
			}
			curveIndex = curveCount - 1;
			t = 1f;
			result = true;
			break;
		}
		onMove?.Invoke(GetPosition(_GetNormalizedTFromCurveIndexAndT(curveIndex2, t2)), GetPosition(_GetNormalizedTFromCurveIndexAndT(curveIndex, t)));
		return result;
	}

	private bool _MoveForwardAtSpeed(float speed, float maxSpeedPerSample, ref int curveIndex, ref float t, Action<Vector3, Vector3> onMove = null)
	{
		int num = Mathf.Max(1, Mathf.CeilToInt(speed / maxSpeedPerSample));
		for (int i = 0; i < num; i++)
		{
			if (_MoveForwardAtSpeed(Math.Min(speed, maxSpeedPerSample), ref curveIndex, ref t, onMove))
			{
				return true;
			}
			speed -= maxSpeedPerSample;
		}
		return false;
	}

	private Vector3[] _SetExtremaPoints(Vector3[] pointArray, int x)
	{
		pointArray[0] = _points[x];
		pointArray[1] = _points[x + 1];
		pointArray[2] = _points[x + 2];
		pointArray[1] = BezierUtil.GetOnCurveExtrema(pointArray[0], pointArray[1], pointArray[2]);
		return pointArray;
	}

	public BezierSpline SetData(Vector3 currentPosition, List<Short2> path, float yPosition = 0f, bool setCurrentPositionToStartingPoint = true)
	{
		BezierUtil.PathToQuadraticCurveData(path, _points, yPosition);
		if (setCurrentPositionToStartingPoint)
		{
			SetNewStartingPoint(currentPosition, 0.375f);
		}
		_SetDirty();
		return this;
	}

	public BezierSpline SetData(List<Vector3> points)
	{
		Pools.TryRefresh(ref _points);
		_points.CopyFrom(points);
		_SetDirty();
		return this;
	}

	public Vector3 GetPosition(float splineT)
	{
		_GetCurveIndexAndTAtNormalizedT(splineT, out var curveIndex, out var t);
		int num = _GetCurveStartPointIndex(curveIndex);
		return BezierUtil.GetPosition(_points[num], _points[num + 1], _points[num + 2], t);
	}

	public Vector3 GetVelocity(float splineT)
	{
		_GetCurveIndexAndTAtNormalizedT(splineT, out var curveIndex, out var t);
		int num = _GetCurveStartPointIndex(curveIndex);
		return BezierUtil.GetVelocity(_points[num], _points[num + 1], _points[num + 2], t);
	}

	public float GetSpeed(float splineT)
	{
		return GetVelocity(splineT).magnitude;
	}

	public Vector3 GetDirection(float splineT)
	{
		return GetVelocity(splineT).normalized;
	}

	public Vector3 GetNormal(float splineT, Vector3 upVector)
	{
		_GetCurveIndexAndTAtNormalizedT(splineT, out var curveIndex, out var t);
		int num = _GetCurveStartPointIndex(curveIndex);
		return BezierUtil.GetNormal(_points[num], _points[num + 1], _points[num + 2], t, upVector);
	}

	public float GetDistanceTraveled(float splineT)
	{
		_GetCurveIndexAndTAtNormalizedT(splineT, out var curveIndex, out var t);
		float num = 0f;
		for (int i = 0; i < curveIndex; i++)
		{
			num += curveLengths[i];
		}
		return num + BezierUtil.ArcLength(points, t, _GetCurveStartPointIndex(curveIndex));
	}

	public PoolStructListHandle<BezierCurve> GetCurves()
	{
		PoolStructListHandle<BezierCurve> poolStructListHandle = Pools.UseStructList<BezierCurve>();
		int num = curveCount;
		for (int i = 0; i < num; i++)
		{
			poolStructListHandle.Add(new BezierCurve(_points, _GetCurveStartPointIndex(i)));
		}
		return poolStructListHandle;
	}

	public Vector3 GetPointOnSplineNearestToPoint(Vector3 point)
	{
		_GetCurveIndexAndTNearestToPoint(point, out var curveIndex, out var t);
		int num = _GetCurveStartPointIndex(curveIndex);
		return BezierUtil.GetPosition(_points[num], _points[num + 1], _points[num + 2], t);
	}

	public float GetNormalizedPositionFromPoint(Vector3 point)
	{
		_GetCurveIndexAndTNearestToPoint(point, out var curveIndex, out var t);
		return _GetNormalizedTFromCurveIndexAndT(curveIndex, t);
	}

	public void SetNewStartingPoint(Vector3 point, float minConnectionDistance = 0.5f)
	{
		if (!((point - _points[0]).sqrMagnitude < 0.001f))
		{
			_GetCurveIndexAndTNearestToPoint(point, out var curveIndex, out var t);
			while ((GetPosition(_GetNormalizedTFromCurveIndexAndT(curveIndex, t)) - point).magnitude < minConnectionDistance && !_MoveForwardAtSpeed(0.01f, ref curveIndex, ref t))
			{
			}
			int num = _GetCurveStartPointIndex(curveIndex);
			Vector3 vector = _points[num];
			Vector3 vector2 = _points[num + 1];
			Vector3 vector3 = _points[num + 2];
			Vector3 item;
			if (curveIndex == 0 && t == 0f)
			{
				item = vector + Vector3.Project(point - vector, BezierUtil.GetDirection(vector, vector2, vector3, t));
				_points.RemoveRange(0, 2);
			}
			else if (curveIndex == curveCount - 1 && t == 1f)
			{
				item = vector3 + Vector3.Project(point - vector3, BezierUtil.GetDirection(vector, vector2, vector3, t));
				_points.Clear();
				_points.Add(vector3);
			}
			else
			{
				item = Vector3.Lerp(vector2, BezierUtil.GetPosition(vector, vector2, vector3, t), t);
				_points.RemoveRange(0, num + 2);
			}
			_points.Insert(0, item);
			_points.Insert(0, point);
			_SetDirty();
		}
	}

	public IEnumerator MoveAlongPath(Transform transform, float maxSpeed, float acceleration, float angularMaxSpeed, float angularAcceleration, bool useScaledTime = true, float maxSpeedPerSample = 0.05f)
	{
		transform.position = GetPosition(0f);
		IEnumerator rotation = transform.RotateTo(GetDirection(0f), angularMaxSpeed, angularAcceleration, useScaledTime: false);
		while (rotation.MoveNext())
		{
			yield return null;
		}
		float t = 0f;
		int curveIndex = 0;
		float speed2 = 0f;
		while (true)
		{
			float num = (useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime);
			float num2 = arcLength - GetDistanceTraveled(_GetNormalizedTFromCurveIndexAndT(curveIndex, t));
			speed2 += ((MathUtil.StoppingDistance(speed2, acceleration) < num2) ? acceleration : (0f - acceleration)) * num;
			speed2 = Mathf.Clamp(speed2, 0f, maxSpeed);
			bool num3 = _MoveForwardAtSpeed(speed2 * num, maxSpeedPerSample, ref curveIndex, ref t);
			transform.position = BezierUtil.GetPosition(_points, t, _GetCurveStartPointIndex(curveIndex));
			Vector3 direction = BezierUtil.GetDirection(_points, t, _GetCurveStartPointIndex(curveIndex));
			transform.forward = ((direction != Vector3.zero) ? direction : transform.forward);
			if (num3)
			{
				break;
			}
			yield return null;
		}
	}

	public static implicit operator bool(BezierSpline bezierSpline)
	{
		if (bezierSpline != null)
		{
			return bezierSpline._points != null;
		}
		return false;
	}

	[Conditional("UNITY_EDITOR")]
	public void DebugDraw(float segmentLengths = 0.05f)
	{
		int curveIndex = 0;
		float t = 0f;
		Vector3 start = GetPosition(0f);
		while (!_MoveForwardAtSpeed(segmentLengths, ref curveIndex, ref t))
		{
			Vector3 position = GetPosition(_GetNormalizedTFromCurveIndexAndT(curveIndex, t));
			UnityEngine.Debug.DrawLine(start, position);
			start = position;
		}
	}
}
