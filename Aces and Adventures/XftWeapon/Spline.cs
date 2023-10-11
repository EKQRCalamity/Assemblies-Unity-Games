using System.Collections.Generic;
using UnityEngine;

namespace XftWeapon;

public class Spline
{
	private List<SplineControlPoint> mControlPoints = new List<SplineControlPoint>();

	private List<SplineControlPoint> mSegments = new List<SplineControlPoint>();

	public int Granularity = 20;

	public SplineControlPoint this[int index]
	{
		get
		{
			if (index <= -1 || index >= mSegments.Count)
			{
				return null;
			}
			return mSegments[index];
		}
	}

	public List<SplineControlPoint> Segments => mSegments;

	public List<SplineControlPoint> ControlPoints => mControlPoints;

	private void RefreshDistance()
	{
		if (mSegments.Count >= 1)
		{
			mSegments[0].Dist = 0f;
			for (int i = 1; i < mSegments.Count; i++)
			{
				mSegments[i].Dist = mSegments[i - 1].Dist + (mSegments[i].Position - mSegments[i - 1].Position).magnitude;
			}
		}
	}

	public SplineControlPoint NextControlPoint(SplineControlPoint controlpoint)
	{
		if (mControlPoints.Count == 0)
		{
			return null;
		}
		int num = controlpoint.ControlPointIndex + 1;
		if (num < mControlPoints.Count)
		{
			return mControlPoints[num];
		}
		return null;
	}

	public SplineControlPoint PreviousControlPoint(SplineControlPoint controlpoint)
	{
		if (mControlPoints.Count == 0)
		{
			return null;
		}
		int num = controlpoint.ControlPointIndex - 1;
		if (num >= 0)
		{
			return mControlPoints[num];
		}
		return null;
	}

	public Vector3 NextPosition(SplineControlPoint controlpoint)
	{
		return NextControlPoint(controlpoint)?.Position ?? controlpoint.Position;
	}

	public Vector3 PreviousPosition(SplineControlPoint controlpoint)
	{
		return PreviousControlPoint(controlpoint)?.Position ?? controlpoint.Position;
	}

	public Vector3 PreviousNormal(SplineControlPoint controlpoint)
	{
		return PreviousControlPoint(controlpoint)?.Normal ?? controlpoint.Normal;
	}

	public Vector3 NextNormal(SplineControlPoint controlpoint)
	{
		return NextControlPoint(controlpoint)?.Normal ?? controlpoint.Normal;
	}

	public SplineControlPoint LenToSegment(float t, out float localF)
	{
		SplineControlPoint splineControlPoint = null;
		t = Mathf.Clamp01(t);
		float num = t * mSegments[mSegments.Count - 1].Dist;
		int num2 = 0;
		for (num2 = 0; num2 < mSegments.Count; num2++)
		{
			if (mSegments[num2].Dist >= num)
			{
				splineControlPoint = mSegments[num2];
				break;
			}
		}
		if (num2 == 0)
		{
			localF = 0f;
			return splineControlPoint;
		}
		float num3 = 0f;
		int index = splineControlPoint.SegmentIndex - 1;
		SplineControlPoint splineControlPoint2 = mSegments[index];
		num3 = splineControlPoint.Dist - splineControlPoint2.Dist;
		localF = (num - splineControlPoint2.Dist) / num3;
		return splineControlPoint2;
	}

	public static Vector3 CatmulRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
	{
		double num = 1.5;
		double num2 = -1.5;
		double num3 = 0.5;
		double num4 = -2.5;
		double num5 = 2.0;
		double num6 = -0.5;
		double num7 = -0.5;
		double num8 = 0.5;
		double num9 = -0.5 * (double)T0.x + num * (double)P0.x + num2 * (double)P1.x + num3 * (double)T1.x;
		double num10 = (double)T0.x + num4 * (double)P0.x + num5 * (double)P1.x + num6 * (double)T1.x;
		double num11 = num7 * (double)T0.x + num8 * (double)P1.x;
		double num12 = P0.x;
		double num13 = -0.5 * (double)T0.y + num * (double)P0.y + num2 * (double)P1.y + num3 * (double)T1.y;
		double num14 = (double)T0.y + num4 * (double)P0.y + num5 * (double)P1.y + num6 * (double)T1.y;
		double num15 = num7 * (double)T0.y + num8 * (double)P1.y;
		double num16 = P0.y;
		double num17 = -0.5 * (double)T0.z + num * (double)P0.z + num2 * (double)P1.z + num3 * (double)T1.z;
		double num18 = (double)T0.z + num4 * (double)P0.z + num5 * (double)P1.z + num6 * (double)T1.z;
		double num19 = num7 * (double)T0.z + num8 * (double)P1.z;
		double num20 = P0.z;
		float x = (float)(((num9 * (double)f + num10) * (double)f + num11) * (double)f + num12);
		float y = (float)(((num13 * (double)f + num14) * (double)f + num15) * (double)f + num16);
		float z = (float)(((num17 * (double)f + num18) * (double)f + num19) * (double)f + num20);
		return new Vector3(x, y, z);
	}

	public Vector3 InterpolateByLen(float tl)
	{
		float localF;
		return LenToSegment(tl, out localF).Interpolate(localF);
	}

	public Vector3 InterpolateNormalByLen(float tl)
	{
		float localF;
		return LenToSegment(tl, out localF).InterpolateNormal(localF);
	}

	public SplineControlPoint AddControlPoint(Vector3 pos, Vector3 up)
	{
		SplineControlPoint splineControlPoint = new SplineControlPoint();
		splineControlPoint.Init(this);
		splineControlPoint.Position = pos;
		splineControlPoint.Normal = up;
		mControlPoints.Add(splineControlPoint);
		splineControlPoint.ControlPointIndex = mControlPoints.Count - 1;
		return splineControlPoint;
	}

	public void SetControlPointAtIndex(int index, Vector3 pos, Vector3 up)
	{
		ControlPoints[index].SetData(pos, up);
	}

	public void Clear()
	{
		mControlPoints.Clear();
	}

	public void RefreshSpline()
	{
		mSegments.Clear();
		for (int i = 0; i < mControlPoints.Count; i++)
		{
			if (mControlPoints[i].IsValid)
			{
				mSegments.Add(mControlPoints[i]);
				mControlPoints[i].SegmentIndex = mSegments.Count - 1;
			}
		}
		RefreshDistance();
	}
}
