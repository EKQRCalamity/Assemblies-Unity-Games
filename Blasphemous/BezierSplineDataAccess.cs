using BezierSplines;
using Sirenix.OdinInspector;
using UnityEngine;

public class BezierSplineDataAccess : MonoBehaviour
{
	public BezierSpline spline;

	public BezierSplineScriptableDatabase database;

	public SPLINE_TYPES splineReference;

	[Button(ButtonSizes.Small)]
	public void LoadSpline()
	{
		BezierSplineData bsp = database.LoadSpline(splineReference);
		spline.FromData(bsp);
	}

	[Button(ButtonSizes.Small)]
	public void SaveSpline()
	{
		database.SaveSpline(spline, splineReference);
	}

	private void SetSpline(BezierSpline newSpline)
	{
		spline = newSpline;
	}
}
