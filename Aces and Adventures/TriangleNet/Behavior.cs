using System;
using TriangleNet.Geometry;

namespace TriangleNet;

internal class Behavior
{
	private bool poly;

	private bool quality;

	private bool varArea;

	private bool convex;

	private bool jettison;

	private bool boundaryMarkers = true;

	private bool noHoles;

	private bool conformDel;

	private Func<ITriangle, double, bool> usertest;

	private int noBisect;

	private double minAngle;

	private double maxAngle;

	private double maxArea = -1.0;

	internal bool fixedArea;

	internal bool useSegments = true;

	internal bool useRegions;

	internal double goodAngle;

	internal double maxGoodAngle;

	internal double offconstant;

	public static bool NoExact { get; set; }

	public bool Quality
	{
		get
		{
			return quality;
		}
		set
		{
			quality = value;
			if (quality)
			{
				Update();
			}
		}
	}

	public double MinAngle
	{
		get
		{
			return minAngle;
		}
		set
		{
			minAngle = value;
			Update();
		}
	}

	public double MaxAngle
	{
		get
		{
			return maxAngle;
		}
		set
		{
			maxAngle = value;
			Update();
		}
	}

	public double MaxArea
	{
		get
		{
			return maxArea;
		}
		set
		{
			maxArea = value;
			fixedArea = value > 0.0;
		}
	}

	public bool VarArea
	{
		get
		{
			return varArea;
		}
		set
		{
			varArea = value;
		}
	}

	public bool Poly
	{
		get
		{
			return poly;
		}
		set
		{
			poly = value;
		}
	}

	public Func<ITriangle, double, bool> UserTest
	{
		get
		{
			return usertest;
		}
		set
		{
			usertest = value;
		}
	}

	public bool Convex
	{
		get
		{
			return convex;
		}
		set
		{
			convex = value;
		}
	}

	public bool ConformingDelaunay
	{
		get
		{
			return conformDel;
		}
		set
		{
			conformDel = value;
		}
	}

	public int NoBisect
	{
		get
		{
			return noBisect;
		}
		set
		{
			noBisect = value;
			if (noBisect < 0 || noBisect > 2)
			{
				noBisect = 0;
			}
		}
	}

	public bool UseBoundaryMarkers
	{
		get
		{
			return boundaryMarkers;
		}
		set
		{
			boundaryMarkers = value;
		}
	}

	public bool NoHoles
	{
		get
		{
			return noHoles;
		}
		set
		{
			noHoles = value;
		}
	}

	public bool Jettison
	{
		get
		{
			return jettison;
		}
		set
		{
			jettison = value;
		}
	}

	public Behavior(bool quality = false, double minAngle = 20.0)
	{
		if (quality)
		{
			this.quality = true;
			this.minAngle = minAngle;
			Update();
		}
	}

	private void Update()
	{
		quality = true;
		if (minAngle < 0.0 || minAngle > 60.0)
		{
			minAngle = 0.0;
			quality = false;
			Log.Instance.Warning("Invalid quality option (minimum angle).", "Mesh.Behavior");
		}
		if (maxAngle != 0.0 && (maxAngle < 60.0 || maxAngle > 180.0))
		{
			maxAngle = 0.0;
			quality = false;
			Log.Instance.Warning("Invalid quality option (maximum angle).", "Mesh.Behavior");
		}
		useSegments = Poly || Quality || Convex;
		goodAngle = Math.Cos(MinAngle * Math.PI / 180.0);
		maxGoodAngle = Math.Cos(MaxAngle * Math.PI / 180.0);
		if (goodAngle == 1.0)
		{
			offconstant = 0.0;
		}
		else
		{
			offconstant = 0.475 * Math.Sqrt((1.0 + goodAngle) / (1.0 - goodAngle));
		}
		goodAngle *= goodAngle;
	}
}
