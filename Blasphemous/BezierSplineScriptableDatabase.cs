using System.Collections.Generic;
using BezierSplines;
using UnityEngine;

[CreateAssetMenu(fileName = "InputIconLayout", menuName = "Blasphemous/Maikel/SplineDatabase")]
public class BezierSplineScriptableDatabase : ScriptableObject
{
	public List<BezierSplineConfig> splines;

	public void SaveSpline(BezierSpline s, SPLINE_TYPES type)
	{
		BezierSplineConfig bezierSplineConfig = splines.Find((BezierSplineConfig x) => x.type == type);
		if (bezierSplineConfig == null)
		{
			splines.Add(new BezierSplineConfig(s.ToData(), type));
		}
		else
		{
			bezierSplineConfig.spline = s.ToData();
		}
	}

	public BezierSplineData LoadSpline(SPLINE_TYPES type)
	{
		return splines.Find((BezierSplineConfig x) => x.type == type).spline;
	}
}
