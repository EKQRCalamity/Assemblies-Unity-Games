using System;
using BezierSplines;

[Serializable]
public class BezierSplineConfig
{
	public BezierSplineData spline;

	public SPLINE_TYPES type;

	public BezierSplineConfig(BezierSplineData _spline, SPLINE_TYPES _type)
	{
		spline = _spline;
		type = _type;
	}
}
