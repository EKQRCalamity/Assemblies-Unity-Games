using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class RamSpline : MonoBehaviour
{
	public SplineProfile currentProfile;

	public SplineProfile oldProfile;

	public List<RamSpline> beginnigChildSplines = new List<RamSpline>();

	public List<RamSpline> endingChildSplines = new List<RamSpline>();

	public RamSpline beginningSpline;

	public RamSpline endingSpline;

	public int beginningConnectionID;

	public int endingConnectionID;

	public float beginningMinWidth = 0.5f;

	public float beginningMaxWidth = 1f;

	public float endingMinWidth = 0.5f;

	public float endingMaxWidth = 1f;

	public int toolbarInt;

	public bool invertUVDirection;

	public bool uvRotation = true;

	public MeshFilter meshfilter;

	public List<Vector4> controlPoints = new List<Vector4>();

	public List<Quaternion> controlPointsRotations = new List<Quaternion>();

	public List<Quaternion> controlPointsOrientation = new List<Quaternion>();

	public List<Vector3> controlPointsUp = new List<Vector3>();

	public List<Vector3> controlPointsDown = new List<Vector3>();

	public List<float> controlPointsSnap = new List<float>();

	public AnimationCurve meshCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public List<AnimationCurve> controlPointsMeshCurves = new List<AnimationCurve>();

	public bool normalFromRaycast;

	public bool snapToTerrain;

	public LayerMask snapMask = 1;

	public List<Vector3> points = new List<Vector3>();

	public List<Vector3> pointsUp = new List<Vector3>();

	public List<Vector3> pointsDown = new List<Vector3>();

	public List<Vector3> points2 = new List<Vector3>();

	public List<Vector3> verticesBeginning = new List<Vector3>();

	public List<Vector3> verticesEnding = new List<Vector3>();

	public List<Vector3> normalsBeginning = new List<Vector3>();

	public List<Vector3> normalsEnding = new List<Vector3>();

	public List<float> widths = new List<float>();

	public List<float> snaps = new List<float>();

	public List<float> lerpValues = new List<float>();

	public List<Quaternion> orientations = new List<Quaternion>();

	public List<Vector3> tangents = new List<Vector3>();

	public List<Vector3> normalsList = new List<Vector3>();

	public Color[] colors;

	public List<Vector2> colorsFlowMap = new List<Vector2>();

	public List<Vector3> verticeDirection = new List<Vector3>();

	public float floatSpeed = 10f;

	public bool generateOnStart;

	public float minVal = 0.5f;

	public float maxVal = 0.5f;

	public float width = 4f;

	public int vertsInShape = 3;

	public float traingleDensity = 0.2f;

	public float uvScale = 3f;

	public Material oldMaterial;

	public bool showVertexColors;

	public bool showFlowMap;

	public bool overrideFlowMap;

	public bool drawOnMesh;

	public bool drawOnMeshFlowMap;

	public bool uvScaleOverride;

	public bool debug;

	public bool debugNormals;

	public bool debugTangents;

	public bool debugBitangent;

	public bool debugFlowmap;

	public bool debugPoints;

	public bool debugPointsConnect;

	public bool debugMesh = true;

	public float distanceToDebug = 5f;

	public Color drawColor = Color.black;

	public bool drawColorR = true;

	public bool drawColorG = true;

	public bool drawColorB = true;

	public bool drawColorA = true;

	public bool drawOnMultiple;

	public float flowSpeed = 1f;

	public float flowDirection;

	public AnimationCurve flowFlat = new AnimationCurve(new Keyframe(0f, 0.025f), new Keyframe(0.5f, 0.05f), new Keyframe(1f, 0.025f));

	public AnimationCurve flowWaterfall = new AnimationCurve(new Keyframe(0f, 0.25f), new Keyframe(1f, 0.25f));

	public bool noiseflowMap;

	public float noiseMultiplierflowMap = 0.1f;

	public float noiseSizeXflowMap = 2f;

	public float noiseSizeZflowMap = 2f;

	public float opacity = 0.1f;

	public float drawSize = 1f;

	public float length;

	public float fulllength;

	public float uv3length;

	public float minMaxWidth;

	public float uvWidth;

	public float uvBeginning;

	public bool receiveShadows;

	public ShadowCastingMode shadowCastingMode;

	public bool generateMeshParts;

	public int meshPartsCount = 3;

	public List<Transform> meshesPartTransforms = new List<Transform>();

	public float simulatedRiverLength = 100f;

	public int simulatedRiverPoints = 10;

	public float simulatedMinStepSize = 1f;

	public bool simulatedNoUp;

	public bool simulatedBreakOnUp = true;

	public int detailTerrain = 100;

	public int detailTerrainForward = 100;

	public float terrainAdditionalWidth = 2f;

	public float terrainSmoothMultiplier = 5f;

	public bool overrideRiverRender;

	public bool noiseWidth;

	public float noiseMultiplierWidth = 4f;

	public float noiseSizeWidth = 0.5f;

	public bool noiseCarve;

	public float noiseMultiplierInside = 1f;

	public float noiseMultiplierOutside = 0.25f;

	public float noiseSizeX = 0.2f;

	public float noiseSizeZ = 0.2f;

	public bool noisePaint;

	public float noiseMultiplierInsidePaint = 0.25f;

	public float noiseMultiplierOutsidePaint = 0.25f;

	public float noiseSizeXPaint = 0.2f;

	public float noiseSizeZPaint = 0.2f;

	public LayerMask maskCarve = 1;

	public AnimationCurve terrainCarve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(10f, -4f));

	public float distSmooth = 5f;

	public float distSmoothStart = 1f;

	public AnimationCurve terrainPaintCarve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public int currentSplatMap = 1;

	public bool mixTwoSplatMaps;

	public int secondSplatMap = 1;

	public bool addCliffSplatMap;

	public int cliffSplatMap = 1;

	public float cliffAngle = 45f;

	public float cliffBlend = 1f;

	public int cliffSplatMapOutside = 1;

	public float cliffAngleOutside = 45f;

	public float cliffBlendOutside = 1f;

	public float distanceClearFoliage = 1f;

	public float distanceClearFoliageTrees = 1f;

	public GameObject meshGO;

	public void Start()
	{
		if (generateOnStart)
		{
			GenerateSpline();
		}
	}

	public static RamSpline CreateSpline(Material splineMaterial = null, List<Vector4> positions = null, string name = "RamSpline")
	{
		GameObject obj = new GameObject(name)
		{
			layer = LayerMask.NameToLayer("Water")
		};
		RamSpline ramSpline = obj.AddComponent<RamSpline>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		if (splineMaterial != null)
		{
			meshRenderer.sharedMaterial = splineMaterial;
		}
		if (positions != null)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				ramSpline.AddPoint(positions[i]);
			}
		}
		return ramSpline;
	}

	public void AddPoint(Vector4 position)
	{
		if (position.w == 0f)
		{
			if (controlPoints.Count > 0)
			{
				position.w = controlPoints[controlPoints.Count - 1].w;
			}
			else
			{
				position.w = width;
			}
		}
		controlPointsRotations.Add(Quaternion.identity);
		controlPoints.Add(position);
		controlPointsSnap.Add(0f);
		controlPointsMeshCurves.Add(new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)));
	}

	public void AddPointAfter(int i)
	{
		Vector4 vector = ((i != -1) ? controlPoints[i] : controlPoints[0]);
		if (i < controlPoints.Count - 1 && controlPoints.Count > i + 1)
		{
			Vector4 vector2 = controlPoints[i + 1];
			if (Vector3.Distance(vector2, vector) > 0f)
			{
				vector = (vector + vector2) * 0.5f;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else if (controlPoints.Count > 1 && i == controlPoints.Count - 1)
		{
			Vector4 vector3 = controlPoints[i - 1];
			if (Vector3.Distance(vector3, vector) > 0f)
			{
				vector += vector - vector3;
			}
			else
			{
				vector.x += 1f;
			}
		}
		else
		{
			vector.x += 1f;
		}
		controlPoints.Insert(i + 1, vector);
		controlPointsRotations.Insert(i + 1, Quaternion.identity);
		controlPointsSnap.Insert(i + 1, 0f);
		controlPointsMeshCurves.Insert(i + 1, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)));
	}

	public void ChangePointPosition(int i, Vector3 position)
	{
		ChangePointPosition(i, new Vector4(position.x, position.y, position.z, 0f));
	}

	public void ChangePointPosition(int i, Vector4 position)
	{
		Vector4 vector = controlPoints[i];
		if (position.w == 0f)
		{
			position.w = vector.w;
		}
		controlPoints[i] = position;
	}

	public void RemovePoint(int i)
	{
		if (i < controlPoints.Count)
		{
			controlPoints.RemoveAt(i);
			controlPointsRotations.RemoveAt(i);
			controlPointsMeshCurves.RemoveAt(i);
			controlPointsSnap.RemoveAt(i);
		}
	}

	public void RemovePoints(int fromID = -1)
	{
		for (int num = controlPoints.Count - 1; num > fromID; num--)
		{
			RemovePoint(num);
		}
	}

	public void GenerateBeginningParentBased()
	{
		vertsInShape = (int)Mathf.Round((float)(beginningSpline.vertsInShape - 1) * (beginningMaxWidth - beginningMinWidth) + 1f);
		if (vertsInShape < 1)
		{
			vertsInShape = 1;
		}
		beginningConnectionID = beginningSpline.points.Count - 1;
		float w = beginningSpline.controlPoints[beginningSpline.controlPoints.Count - 1].w;
		w *= beginningMaxWidth - beginningMinWidth;
		Vector4 value = Vector3.Lerp(beginningSpline.pointsDown[beginningConnectionID], beginningSpline.pointsUp[beginningConnectionID], beginningMinWidth + (beginningMaxWidth - beginningMinWidth) * 0.5f) + beginningSpline.transform.position - base.transform.position;
		value.w = w;
		controlPoints[0] = value;
		if (!uvScaleOverride)
		{
			uvScale = beginningSpline.uvScale;
		}
	}

	public void GenerateEndingParentBased()
	{
		if (beginningSpline == null)
		{
			vertsInShape = (int)Mathf.Round((float)(endingSpline.vertsInShape - 1) * (endingMaxWidth - endingMinWidth) + 1f);
			if (vertsInShape < 1)
			{
				vertsInShape = 1;
			}
		}
		endingConnectionID = 0;
		float w = endingSpline.controlPoints[0].w;
		w *= endingMaxWidth - endingMinWidth;
		Vector4 value = Vector3.Lerp(endingSpline.pointsDown[endingConnectionID], endingSpline.pointsUp[endingConnectionID], endingMinWidth + (endingMaxWidth - endingMinWidth) * 0.5f) + endingSpline.transform.position - base.transform.position;
		value.w = w;
		controlPoints[controlPoints.Count - 1] = value;
	}

	public void GenerateSpline(List<RamSpline> generatedSplines = null)
	{
		generatedSplines = new List<RamSpline>();
		if (beginningSpline != null && beginningSpline.endingSpline != null)
		{
			Debug.LogError("River can't be ending spline and have beginning spline");
			return;
		}
		if (endingSpline != null && endingSpline.beginningSpline != null)
		{
			Debug.LogError("River can't be begining spline and have ending spline");
			return;
		}
		if ((bool)beginningSpline)
		{
			GenerateBeginningParentBased();
		}
		if ((bool)endingSpline)
		{
			GenerateEndingParentBased();
		}
		List<Vector4> list = new List<Vector4>();
		for (int i = 0; i < controlPoints.Count; i++)
		{
			if (i > 0)
			{
				if (Vector3.Distance(controlPoints[i], controlPoints[i - 1]) > 0f)
				{
					list.Add(controlPoints[i]);
				}
			}
			else
			{
				list.Add(controlPoints[i]);
			}
		}
		Mesh mesh = new Mesh();
		meshfilter = GetComponent<MeshFilter>();
		if (list.Count < 2)
		{
			mesh.Clear();
			meshfilter.mesh = mesh;
			return;
		}
		controlPointsOrientation = new List<Quaternion>();
		lerpValues.Clear();
		snaps.Clear();
		points.Clear();
		pointsUp.Clear();
		pointsDown.Clear();
		orientations.Clear();
		tangents.Clear();
		normalsList.Clear();
		widths.Clear();
		controlPointsUp.Clear();
		controlPointsDown.Clear();
		verticesBeginning.Clear();
		verticesEnding.Clear();
		normalsBeginning.Clear();
		normalsEnding.Clear();
		if (beginningSpline != null && beginningSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[0] = Quaternion.identity;
		}
		if (endingSpline != null && endingSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[controlPointsRotations.Count - 1] = Quaternion.identity;
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (j <= list.Count - 2)
			{
				CalculateCatmullRomSideSplines(list, j);
			}
		}
		if (beginningSpline != null && beginningSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[0] = Quaternion.Inverse(controlPointsOrientation[0]) * beginningSpline.controlPointsOrientation[beginningSpline.controlPointsOrientation.Count - 1];
		}
		if (endingSpline != null && endingSpline.controlPointsRotations.Count > 0)
		{
			controlPointsRotations[controlPointsRotations.Count - 1] = Quaternion.Inverse(controlPointsOrientation[controlPointsOrientation.Count - 1]) * endingSpline.controlPointsOrientation[0];
		}
		controlPointsOrientation = new List<Quaternion>();
		controlPointsUp.Clear();
		controlPointsDown.Clear();
		for (int k = 0; k < list.Count; k++)
		{
			if (k <= list.Count - 2)
			{
				CalculateCatmullRomSideSplines(list, k);
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			if (l <= list.Count - 2)
			{
				CalculateCatmullRomSplineParameters(list, l);
			}
		}
		for (int m = 0; m < controlPointsUp.Count; m++)
		{
			if (m <= controlPointsUp.Count - 2)
			{
				CalculateCatmullRomSpline(controlPointsUp, m, ref pointsUp);
			}
		}
		for (int n = 0; n < controlPointsDown.Count; n++)
		{
			if (n <= controlPointsDown.Count - 2)
			{
				CalculateCatmullRomSpline(controlPointsDown, n, ref pointsDown);
			}
		}
		GenerateMesh(ref mesh);
		if (generatedSplines == null)
		{
			return;
		}
		generatedSplines.Add(this);
		foreach (RamSpline beginnigChildSpline in beginnigChildSplines)
		{
			if (beginnigChildSpline != null && !generatedSplines.Contains(beginnigChildSpline) && (beginnigChildSpline.beginningSpline == this || beginnigChildSpline.endingSpline == this))
			{
				beginnigChildSpline.GenerateSpline(generatedSplines);
			}
		}
		foreach (RamSpline endingChildSpline in endingChildSplines)
		{
			if (endingChildSpline != null && !generatedSplines.Contains(endingChildSpline) && (endingChildSpline.beginningSpline == this || endingChildSpline.endingSpline == this))
			{
				endingChildSpline.GenerateSpline(generatedSplines);
			}
		}
	}

	private void CalculateCatmullRomSideSplines(List<Vector4> controlPoints, int pos)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[ClampListPos(pos + 2)];
		}
		int num = 0;
		if (pos == controlPoints.Count - 2)
		{
			num = 1;
		}
		for (int i = 0; i <= num; i++)
		{
			Vector3 catmullRomPosition = GetCatmullRomPosition(i, p, p2, p3, p4);
			Vector3 normalized = GetCatmullRomTangent(i, p, p2, p3, p4).normalized;
			Vector3 normalized2 = CalculateNormal(normalized, Vector3.up).normalized;
			Quaternion quaternion = ((!(normalized2 == normalized) || !(normalized2 == Vector3.zero)) ? Quaternion.LookRotation(normalized, normalized2) : Quaternion.identity);
			quaternion *= Quaternion.Lerp(controlPointsRotations[pos], controlPointsRotations[ClampListPos(pos + 1)], i);
			controlPointsOrientation.Add(quaternion);
			Vector3 item = catmullRomPosition + quaternion * (0.5f * controlPoints[pos + i].w * Vector3.right);
			Vector3 item2 = catmullRomPosition + quaternion * (0.5f * controlPoints[pos + i].w * Vector3.left);
			controlPointsUp.Add(item);
			controlPointsDown.Add(item2);
		}
	}

	private void CalculateCatmullRomSplineParameters(List<Vector4> controlPoints, int pos, bool initialPoints = false)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[ClampListPos(pos + 2)];
		}
		int num = Mathf.FloorToInt(1f / traingleDensity);
		float num2 = 1f;
		float num3 = 0f;
		if (pos > 0)
		{
			num3 = 1f;
		}
		for (num2 = num3; num2 <= (float)num; num2 += 1f)
		{
			float t = num2 * traingleDensity;
			CalculatePointParameters(controlPoints, pos, p, p2, p3, p4, t);
		}
		if (num2 < (float)num)
		{
			num2 = num;
			float t2 = num2 * traingleDensity;
			CalculatePointParameters(controlPoints, pos, p, p2, p3, p4, t2);
		}
	}

	private void CalculateCatmullRomSpline(List<Vector3> controlPoints, int pos, ref List<Vector3> points)
	{
		Vector3 p = controlPoints[pos];
		Vector3 p2 = controlPoints[pos];
		Vector3 p3 = controlPoints[ClampListPos(pos + 1)];
		Vector3 p4 = controlPoints[ClampListPos(pos + 1)];
		if (pos > 0)
		{
			p = controlPoints[ClampListPos(pos - 1)];
		}
		if (pos < controlPoints.Count - 2)
		{
			p4 = controlPoints[ClampListPos(pos + 2)];
		}
		int num = Mathf.FloorToInt(1f / traingleDensity);
		float num2 = 1f;
		float num3 = 0f;
		if (pos > 0)
		{
			num3 = 1f;
		}
		for (num2 = num3; num2 <= (float)num; num2 += 1f)
		{
			float t = num2 * traingleDensity;
			CalculatePointPosition(controlPoints, pos, p, p2, p3, p4, t, ref points);
		}
		if (num2 < (float)num)
		{
			num2 = num;
			float t2 = num2 * traingleDensity;
			CalculatePointPosition(controlPoints, pos, p, p2, p3, p4, t2, ref points);
		}
	}

	private void CalculatePointPosition(List<Vector3> controlPoints, int pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, ref List<Vector3> points)
	{
		Vector3 catmullRomPosition = GetCatmullRomPosition(t, p0, p1, p2, p3);
		points.Add(catmullRomPosition);
		Vector3 normalized = GetCatmullRomTangent(t, p0, p1, p2, p3).normalized;
		_ = CalculateNormal(normalized, Vector3.up).normalized;
	}

	private void CalculatePointParameters(List<Vector4> controlPoints, int pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 catmullRomPosition = GetCatmullRomPosition(t, p0, p1, p2, p3);
		widths.Add(Mathf.Lerp(controlPoints[pos].w, controlPoints[ClampListPos(pos + 1)].w, t));
		if (controlPointsSnap.Count > pos + 1)
		{
			snaps.Add(Mathf.Lerp(controlPointsSnap[pos], controlPointsSnap[ClampListPos(pos + 1)], t));
		}
		else
		{
			snaps.Add(0f);
		}
		lerpValues.Add((float)pos + t);
		points.Add(catmullRomPosition);
		Vector3 normalized = GetCatmullRomTangent(t, p0, p1, p2, p3).normalized;
		Vector3 normalized2 = CalculateNormal(normalized, Vector3.up).normalized;
		Quaternion item = ((!(normalized2 == normalized) || !(normalized2 == Vector3.zero)) ? Quaternion.LookRotation(normalized, normalized2) : Quaternion.identity);
		item *= Quaternion.Lerp(controlPointsRotations[pos], controlPointsRotations[ClampListPos(pos + 1)], t);
		orientations.Add(item);
		tangents.Add(normalized);
		if (normalsList.Count > 0 && Vector3.Angle(normalsList[normalsList.Count - 1], normalized2) > 90f)
		{
			normalized2 *= -1f;
		}
		normalsList.Add(normalized2);
	}

	private int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = controlPoints.Count - 1;
		}
		if (pos > controlPoints.Count)
		{
			pos = 1;
		}
		else if (pos > controlPoints.Count - 1)
		{
			pos = 0;
		}
		return pos;
	}

	private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 vector = 2f * p1;
		Vector3 vector2 = p2 - p0;
		Vector3 vector3 = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 vector4 = -p0 + 3f * p1 - 3f * p2 + p3;
		return 0.5f * (vector + vector2 * t + vector3 * t * t + vector4 * t * t * t);
	}

	private Vector3 GetCatmullRomTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		return 0.5f * (-p0 + p2 + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t);
	}

	private Vector3 CalculateNormal(Vector3 tangent, Vector3 up)
	{
		Vector3 rhs = Vector3.Cross(up, tangent);
		return Vector3.Cross(tangent, rhs);
	}

	private void GenerateMesh(ref Mesh mesh)
	{
		MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.receiveShadows = receiveShadows;
			component.shadowCastingMode = shadowCastingMode;
		}
		foreach (Transform meshesPartTransform in meshesPartTransforms)
		{
			if (meshesPartTransform != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(meshesPartTransform.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(meshesPartTransform.gameObject);
				}
				UnityEngine.Object.Destroy(meshesPartTransform.gameObject);
			}
		}
		int num = points.Count - 1;
		int count = points.Count;
		int num2 = vertsInShape * count;
		List<int> list = new List<int>();
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		Vector2[] array3 = new Vector2[num2];
		Vector2[] array4 = new Vector2[num2];
		Vector2[] array5 = new Vector2[num2];
		if (colors == null || colors.Length != num2)
		{
			colors = new Color[num2];
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = Color.black;
			}
		}
		if (colorsFlowMap.Count != num2)
		{
			colorsFlowMap.Clear();
		}
		length = 0f;
		fulllength = 0f;
		if (beginningSpline != null)
		{
			length = beginningSpline.length;
		}
		minMaxWidth = 1f;
		uvWidth = 1f;
		uvBeginning = 0f;
		if (beginningSpline != null)
		{
			minMaxWidth = beginningMaxWidth - beginningMinWidth;
			uvWidth = minMaxWidth * beginningSpline.uvWidth;
			uvBeginning = beginningSpline.uvWidth * beginningMinWidth + beginningSpline.uvBeginning;
		}
		else if (endingSpline != null)
		{
			minMaxWidth = endingMaxWidth - endingMinWidth;
			uvWidth = minMaxWidth * endingSpline.uvWidth;
			uvBeginning = endingSpline.uvWidth * endingMinWidth + endingSpline.uvBeginning;
		}
		for (int j = 0; j < pointsDown.Count; j++)
		{
			float num3 = widths[j];
			if (j > 0)
			{
				fulllength += uvWidth * Vector3.Distance(pointsDown[j], pointsDown[j - 1]) / (uvScale * num3);
			}
		}
		float num4 = Mathf.Round(fulllength);
		for (int k = 0; k < pointsDown.Count; k++)
		{
			float num5 = widths[k];
			int num6 = k * vertsInShape;
			if (k > 0)
			{
				length += uvWidth * Vector3.Distance(pointsDown[k], pointsDown[k - 1]) / (uvScale * num5) / fulllength * num4;
			}
			float num7 = 0f;
			float num8 = 0f;
			for (int l = 0; l < vertsInShape; l++)
			{
				int num9 = num6 + l;
				float num10 = (float)l / (float)(vertsInShape - 1);
				num10 = ((!(num10 < 0.5f)) ? (((num10 - 0.5f) * (1f - maxVal) + 0.5f * maxVal) * 2f) : (num10 * (minVal * 2f)));
				if (k == 0 && beginningSpline != null && beginningSpline.verticesEnding != null && beginningSpline.normalsEnding != null)
				{
					int num11 = (int)((float)beginningSpline.vertsInShape * beginningMinWidth);
					array[num9] = beginningSpline.verticesEnding[Mathf.Clamp(l + num11, 0, beginningSpline.verticesEnding.Count - 1)] + beginningSpline.transform.position - base.transform.position;
				}
				else if (k == pointsDown.Count - 1 && endingSpline != null && endingSpline.verticesBeginning != null && endingSpline.verticesBeginning.Count > 0 && endingSpline.normalsBeginning != null)
				{
					int num12 = (int)((float)endingSpline.vertsInShape * endingMinWidth);
					array[num9] = endingSpline.verticesBeginning[Mathf.Clamp(l + num12, 0, endingSpline.verticesBeginning.Count - 1)] + endingSpline.transform.position - base.transform.position;
				}
				else
				{
					array[num9] = Vector3.Lerp(pointsDown[k], pointsUp[k], num10);
					if (Physics.Raycast(array[num9] + base.transform.position + Vector3.up * 5f, Vector3.down, out var hitInfo, 1000f, snapMask.value))
					{
						array[num9] = Vector3.Lerp(array[num9], hitInfo.point - base.transform.position + new Vector3(0f, 0.1f, 0f), (Mathf.Sin(MathF.PI * snaps[k] - MathF.PI / 2f) + 1f) * 0.5f);
					}
					if (normalFromRaycast && Physics.Raycast(points[k] + base.transform.position + Vector3.up * 5f, Vector3.down, out var hitInfo2, 1000f, snapMask.value))
					{
						array2[num9] = hitInfo2.normal;
					}
					array[num9].y += Mathf.Lerp(controlPointsMeshCurves[Mathf.FloorToInt(lerpValues[k])].Evaluate(num10), controlPointsMeshCurves[Mathf.CeilToInt(lerpValues[k])].Evaluate(num10), lerpValues[k] - Mathf.Floor(lerpValues[k]));
				}
				if (k > 0 && k < 5 && beginningSpline != null && beginningSpline.verticesEnding != null)
				{
					array[num9].y = (array[num9].y + array[num9 - vertsInShape].y) * 0.5f;
				}
				if (k == pointsDown.Count - 1 && endingSpline != null && endingSpline.verticesBeginning != null)
				{
					for (int m = 1; m < 5; m++)
					{
						array[num9 - vertsInShape * m].y = (array[num9 - vertsInShape * (m - 1)].y + array[num9 - vertsInShape * m].y) * 0.5f;
					}
				}
				if (k == 0)
				{
					verticesBeginning.Add(array[num9]);
				}
				if (k == pointsDown.Count - 1)
				{
					verticesEnding.Add(array[num9]);
				}
				if (!normalFromRaycast)
				{
					array2[num9] = orientations[k] * Vector3.up;
				}
				if (k == 0)
				{
					normalsBeginning.Add(array2[num9]);
				}
				if (k == pointsDown.Count - 1)
				{
					normalsEnding.Add(array2[num9]);
				}
				if (l > 0)
				{
					num7 = num10 * uvWidth;
					num8 = num10;
				}
				if (beginningSpline != null || endingSpline != null)
				{
					num7 += uvBeginning;
				}
				num7 /= uvScale;
				float num13 = FlowCalculate(num8, array2[num9].y, array[num9]);
				int num14 = 10;
				if (beginnigChildSplines.Count > 0 && k <= num14)
				{
					float num15 = 0f;
					foreach (RamSpline beginnigChildSpline in beginnigChildSplines)
					{
						if (!(beginnigChildSpline == null) && Mathf.CeilToInt(beginnigChildSpline.endingMaxWidth * (float)(vertsInShape - 1)) >= l && l >= Mathf.CeilToInt(beginnigChildSpline.endingMinWidth * (float)(vertsInShape - 1)))
						{
							num15 = (float)(l - Mathf.CeilToInt(beginnigChildSpline.endingMinWidth * (float)(vertsInShape - 1))) / (float)(Mathf.CeilToInt(beginnigChildSpline.endingMaxWidth * (float)(vertsInShape - 1)) - Mathf.CeilToInt(beginnigChildSpline.endingMinWidth * (float)(vertsInShape - 1)));
							num15 = FlowCalculate(num15, array2[num9].y, array[num9]);
						}
					}
					num13 = ((k <= 0) ? num15 : Mathf.Lerp(num13, num15, 1f - (float)k / (float)num14));
				}
				if (k >= pointsDown.Count - num14 - 1 && endingChildSplines.Count > 0)
				{
					float num16 = 0f;
					foreach (RamSpline endingChildSpline in endingChildSplines)
					{
						if (!(endingChildSpline == null) && Mathf.CeilToInt(endingChildSpline.beginningMaxWidth * (float)(vertsInShape - 1)) >= l && l >= Mathf.CeilToInt(endingChildSpline.beginningMinWidth * (float)(vertsInShape - 1)))
						{
							num16 = (float)(l - Mathf.CeilToInt(endingChildSpline.beginningMinWidth * (float)(vertsInShape - 1))) / (float)(Mathf.CeilToInt(endingChildSpline.beginningMaxWidth * (float)(vertsInShape - 1)) - Mathf.CeilToInt(endingChildSpline.beginningMinWidth * (float)(vertsInShape - 1)));
							num16 = FlowCalculate(num16, array2[num9].y, array[num9]);
						}
					}
					num13 = ((k >= pointsDown.Count - 1) ? num16 : Mathf.Lerp(num13, num16, (float)(k - (pointsDown.Count - num14 - 1)) / (float)num14));
				}
				float num17 = (0f - (num8 - 0.5f)) * 0.01f;
				uv3length = length / fulllength;
				if (beginningSpline != null)
				{
					uv3length = (length - beginningSpline.length) / fulllength + beginningSpline.uv3length;
				}
				if (beginnigChildSplines != null && beginnigChildSplines.Count > 0)
				{
					uv3length = length / fulllength + beginnigChildSplines[0].uv3length;
				}
				if (uvRotation)
				{
					if (!invertUVDirection)
					{
						array3[num9] = new Vector2(1f - length, num7);
						array4[num9] = new Vector2(1f - uv3length, num8);
						array5[num9] = new Vector2(num13, num17);
					}
					else
					{
						array3[num9] = new Vector2(1f + length, num7);
						array4[num9] = new Vector2(1f + uv3length, num8);
						array5[num9] = new Vector2(num13, num17);
					}
				}
				else if (!invertUVDirection)
				{
					array3[num9] = new Vector2(num7, 1f - length);
					array4[num9] = new Vector2(num8, 1f - uv3length);
					array5[num9] = new Vector2(num17, num13);
				}
				else
				{
					array3[num9] = new Vector2(num7, 1f + length);
					array4[num9] = new Vector2(num8, 1f + uv3length);
					array5[num9] = new Vector2(num17, num13);
				}
				float num18 = (int)(array5[num9].x * 100f);
				array5[num9].x = num18 * 0.01f;
				num18 = (int)(array5[num9].y * 100f);
				array5[num9].y = num18 * 0.01f;
				if (colorsFlowMap.Count <= num9)
				{
					colorsFlowMap.Add(array5[num9]);
				}
				else if (!overrideFlowMap)
				{
					colorsFlowMap[num9] = array5[num9];
				}
			}
		}
		for (int n = 0; n < num; n++)
		{
			int num19 = n * vertsInShape;
			for (int num20 = 0; num20 < vertsInShape - 1; num20++)
			{
				int item = num19 + num20;
				int item2 = num19 + num20 + vertsInShape;
				int item3 = num19 + num20 + 1 + vertsInShape;
				int item4 = num19 + num20 + 1;
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				list.Add(item3);
				list.Add(item4);
				list.Add(item);
			}
		}
		verticeDirection.Clear();
		for (int num21 = 0; num21 < array.Length - vertsInShape; num21++)
		{
			Vector3 item5 = (array[num21 + vertsInShape] - array[num21]).normalized;
			if (uvRotation)
			{
				item5 = new Vector3(item5.z, 0f, 0f - item5.x);
			}
			verticeDirection.Add(item5);
		}
		for (int num22 = array.Length - vertsInShape; num22 < array.Length; num22++)
		{
			Vector3 item6 = (array[num22] - array[num22 - vertsInShape]).normalized;
			if (uvRotation)
			{
				item6 = new Vector3(item6.z, 0f, 0f - item6.x);
			}
			verticeDirection.Add(item6);
		}
		mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.uv = array3;
		mesh.uv3 = array4;
		mesh.uv4 = colorsFlowMap.ToArray();
		mesh.triangles = list.ToArray();
		mesh.colors = colors;
		mesh.RecalculateTangents();
		meshfilter.mesh = mesh;
		GetComponent<MeshRenderer>().enabled = true;
		if (generateMeshParts)
		{
			GenerateMeshParts(mesh);
		}
	}

	public void GenerateMeshParts(Mesh baseMesh)
	{
		foreach (Transform meshesPartTransform in meshesPartTransforms)
		{
			if (meshesPartTransform != null)
			{
				UnityEngine.Object.DestroyImmediate(meshesPartTransform.gameObject);
			}
		}
		Vector3[] vertices = baseMesh.vertices;
		Vector3[] normals = baseMesh.normals;
		Vector2[] uv = baseMesh.uv;
		Vector2[] uv2 = baseMesh.uv3;
		GetComponent<MeshRenderer>().enabled = false;
		int num = Mathf.RoundToInt((float)(vertices.Length / vertsInShape) / (float)meshPartsCount) * vertsInShape;
		for (int i = 0; i < meshPartsCount; i++)
		{
			GameObject gameObject = new GameObject(base.gameObject.name + "- Mesh part " + i);
			gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			meshesPartTransforms.Add(gameObject.transform);
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
			meshRenderer.receiveShadows = receiveShadows;
			meshRenderer.shadowCastingMode = shadowCastingMode;
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			Mesh mesh = new Mesh();
			mesh.Clear();
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector2> list3 = new List<Vector2>();
			List<Vector2> list4 = new List<Vector2>();
			List<Vector2> list5 = new List<Vector2>();
			List<Color> list6 = new List<Color>();
			List<int> list7 = new List<int>();
			for (int j = num * i + ((i > 0) ? (-vertsInShape) : 0); (j < num * (i + 1) && j < vertices.Length) || (i == meshPartsCount - 1 && j < vertices.Length); j++)
			{
				list.Add(vertices[j]);
				list2.Add(normals[j]);
				list3.Add(uv[j]);
				list4.Add(uv2[j]);
				list5.Add(colorsFlowMap[j]);
				list6.Add(colors[j]);
			}
			if (list.Count <= 0)
			{
				continue;
			}
			Vector3 vector = list[0];
			for (int k = 0; k < list.Count; k++)
			{
				list[k] -= vector;
			}
			for (int l = 0; l < list.Count / vertsInShape - 1; l++)
			{
				int num2 = l * vertsInShape;
				for (int m = 0; m < vertsInShape - 1; m++)
				{
					int item = num2 + m;
					int item2 = num2 + m + vertsInShape;
					int item3 = num2 + m + 1 + vertsInShape;
					int item4 = num2 + m + 1;
					list7.Add(item);
					list7.Add(item2);
					list7.Add(item3);
					list7.Add(item3);
					list7.Add(item4);
					list7.Add(item);
				}
			}
			gameObject.transform.position += vector;
			mesh.vertices = list.ToArray();
			mesh.triangles = list7.ToArray();
			mesh.normals = list2.ToArray();
			mesh.uv = list3.ToArray();
			mesh.uv3 = list4.ToArray();
			mesh.uv4 = list5.ToArray();
			mesh.colors = list6.ToArray();
			mesh.RecalculateTangents();
			meshFilter.mesh = mesh;
		}
	}

	public void AddNoiseToWidths()
	{
		for (int i = 0; i < controlPoints.Count; i++)
		{
			Vector4 value = controlPoints[i];
			value.w += (noiseWidth ? (noiseMultiplierWidth * (Mathf.PerlinNoise(noiseSizeWidth * (float)i, 0f) - 0.5f)) : 0f);
			if (value.w < 0f)
			{
				value.w = 0f;
			}
			controlPoints[i] = value;
		}
	}

	public void SimulateRiver(bool generate = true)
	{
		if (meshGO != null)
		{
			if (Application.isEditor)
			{
				UnityEngine.Object.DestroyImmediate(meshGO);
			}
			else
			{
				UnityEngine.Object.Destroy(meshGO);
			}
		}
		if (controlPoints.Count == 0)
		{
			Debug.Log("Add one point to start Simulating River");
			return;
		}
		Ray ray = default(Ray);
		Vector3 vector = base.transform.TransformPoint(controlPoints[controlPoints.Count - 1]);
		List<Vector3> list = new List<Vector3>();
		if (controlPoints.Count > 1)
		{
			list.Add(base.transform.TransformPoint(controlPoints[controlPoints.Count - 2]));
			list.Add(vector);
		}
		List<Vector3> list2 = new List<Vector3>();
		list2.Add(vector);
		float num = 0f;
		int num2 = -1;
		int num3 = 0;
		bool flag = false;
		float num4 = 0f;
		num4 = ((controlPoints.Count <= 0) ? width : controlPoints[controlPoints.Count - 1].w);
		do
		{
			num2++;
			if (num2 <= 0)
			{
				continue;
			}
			Vector3 vector2 = Vector3.zero;
			float num5 = float.MinValue;
			bool flag2 = false;
			for (float num6 = simulatedMinStepSize; num6 < 10f; num6 += 0.1f)
			{
				for (int i = 0; i < 36; i++)
				{
					float x = num6 * Mathf.Cos(i);
					float z = num6 * Mathf.Sin(i);
					ray.origin = vector + new Vector3(0f, 1000f, 0f) + new Vector3(x, 0f, z);
					ray.direction = Vector3.down;
					if (!Physics.Raycast(ray, out var hitInfo, 10000f) || !(hitInfo.distance > num5))
					{
						continue;
					}
					bool flag3 = true;
					foreach (Vector3 item2 in list)
					{
						if (Vector3.Distance(item2, vector) > Vector3.Distance(item2, hitInfo.point) + 0.5f)
						{
							flag3 = false;
							break;
						}
					}
					if (flag3)
					{
						flag2 = true;
						num5 = hitInfo.distance;
						vector2 = hitInfo.point;
					}
				}
				if (flag2)
				{
					break;
				}
			}
			if (!flag2)
			{
				break;
			}
			if (vector2.y > vector.y)
			{
				if (simulatedNoUp)
				{
					vector2.y = vector.y;
				}
				if (simulatedBreakOnUp)
				{
					flag = true;
				}
			}
			num += Vector3.Distance(vector2, vector);
			if (num2 % simulatedRiverPoints == 0 || simulatedRiverLength <= num || flag)
			{
				list2.Add(vector2);
				if (generate)
				{
					num3++;
					Vector4 item = vector2 - base.transform.position;
					item.w = num4 + (noiseWidth ? (noiseMultiplierWidth * (Mathf.PerlinNoise(noiseSizeWidth * (float)num3, 0f) - 0.5f)) : 0f);
					controlPointsRotations.Add(Quaternion.identity);
					controlPoints.Add(item);
					controlPointsSnap.Add(0f);
					controlPointsMeshCurves.Add(new AnimationCurve(meshCurve.keys));
				}
			}
			list.Add(vector);
			vector = vector2;
		}
		while (simulatedRiverLength > num && !flag);
		if (generate)
		{
			return;
		}
		num4 = ((controlPoints.Count <= 0) ? width : controlPoints[controlPoints.Count - 1].w);
		float num7 = 0f;
		List<List<Vector4>> list3 = new List<List<Vector4>>();
		Vector3 vector3 = default(Vector3);
		for (num2 = 0; num2 < list2.Count - 1; num2++)
		{
			num7 = num4 + (noiseWidth ? (noiseMultiplierWidth * (Mathf.PerlinNoise(noiseSizeWidth * (float)num2, 0f) - 0.5f)) : 0f);
			vector3 = Vector3.Cross(list2[num2 + 1] - list2[num2], Vector3.up).normalized;
			if (num2 > 0)
			{
				Vector3 normalized = Vector3.Cross(list2[num2] - list2[num2 - 1], Vector3.up).normalized;
				vector3 = (vector3 + normalized).normalized;
			}
			List<Vector4> list4 = new List<Vector4>();
			list4.Add(list2[num2] + vector3 * num7 * 0.5f);
			list4.Add(list2[num2] - vector3 * num7 * 0.5f);
			list3.Add(list4);
		}
		num7 = num4 + (noiseWidth ? (noiseMultiplierWidth * (Mathf.PerlinNoise(noiseSizeWidth * (float)num2, 0f) - 0.5f)) : 0f);
		List<Vector4> list5 = new List<Vector4>();
		list5.Add(list2[num2] + vector3 * num7 * 0.5f);
		list5.Add(list2[num2] - vector3 * num7 * 0.5f);
		list3.Add(list5);
		Mesh mesh = new Mesh();
		mesh.indexFormat = IndexFormat.UInt32;
		List<Vector3> list6 = new List<Vector3>();
		List<int> list7 = new List<int>();
		foreach (List<Vector4> item3 in list3)
		{
			foreach (Vector4 item4 in item3)
			{
				list6.Add(item4);
			}
		}
		for (num2 = 0; num2 < list3.Count - 1; num2++)
		{
			int count = list3[num2].Count;
			for (int j = 0; j < count - 1; j++)
			{
				list7.Add(j + num2 * count);
				list7.Add(j + (num2 + 1) * count);
				list7.Add(j + 1 + num2 * count);
				list7.Add(j + 1 + num2 * count);
				list7.Add(j + (num2 + 1) * count);
				list7.Add(j + 1 + (num2 + 1) * count);
			}
		}
		mesh.SetVertices(list6);
		mesh.SetTriangles(list7, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		mesh.RecalculateBounds();
		meshGO = new GameObject("TerrainMesh");
		meshGO.hideFlags = HideFlags.HideAndDontSave;
		meshGO.AddComponent<MeshFilter>();
		meshGO.transform.parent = base.transform;
		MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = new Material(Shader.Find("Debug Terrain Carve"));
		meshRenderer.sharedMaterial.color = new Color(0f, 0.5f, 0f);
		meshGO.transform.position = Vector3.zero;
		meshGO.GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	public void ShowTerrainCarve(float differentSize = 0f)
	{
		if (Application.isEditor && meshGO == null)
		{
			Transform transform = base.transform.Find("TerrainMesh");
			if (transform != null)
			{
				meshGO = transform.gameObject;
			}
		}
		if (meshGO != null)
		{
			if (Application.isEditor)
			{
				UnityEngine.Object.DestroyImmediate(meshGO);
			}
			else
			{
				UnityEngine.Object.Destroy(meshGO);
			}
		}
		_ = meshfilter.sharedMesh;
		detailTerrainForward = 2;
		detailTerrain = 10;
		if (differentSize == 0f)
		{
			terrainAdditionalWidth = distSmooth + distSmoothStart;
		}
		else
		{
			terrainAdditionalWidth = differentSize;
		}
		List<List<Vector4>> list = new List<List<Vector4>>();
		float num = 0f;
		for (int i = 0; i < pointsDown.Count - 1; i++)
		{
			for (int j = 0; j <= detailTerrainForward; j++)
			{
				List<Vector4> list2 = new List<Vector4>();
				Vector3 vector = Vector3.Lerp(pointsDown[i], pointsDown[i + 1], (float)j / (float)detailTerrainForward);
				Vector3 vector2 = Vector3.Lerp(pointsUp[i], pointsUp[i + 1], (float)j / (float)detailTerrainForward);
				Vector3 vector3 = vector - vector2;
				float magnitude = vector3.magnitude;
				vector += vector3 * 0.05f;
				vector2 -= vector3 * 0.05f;
				vector3.Normalize();
				Vector3 a = vector + vector3 * terrainAdditionalWidth * 0.5f;
				Vector3 b = vector2 - vector3 * terrainAdditionalWidth * 0.5f;
				RaycastHit hitInfo;
				if (terrainAdditionalWidth > 0f)
				{
					for (int k = 0; k < detailTerrain; k++)
					{
						Vector3 vector4 = Vector3.Lerp(a, vector, (float)k / (float)detailTerrain) + base.transform.position;
						if (Physics.Raycast(vector4 + Vector3.up * 500f, Vector3.down, out hitInfo, 10000f, maskCarve.value))
						{
							num = ((!noiseCarve) ? 0f : (Mathf.PerlinNoise(vector4.x * noiseSizeX, vector4.z * noiseSizeZ) * noiseMultiplierOutside - noiseMultiplierOutside * 0.5f));
							float num2 = 1f - (float)k / (float)detailTerrain;
							num2 *= terrainAdditionalWidth;
							float b2 = vector4.y + terrainCarve.Evaluate(0f - num2) + terrainCarve.Evaluate(0f - num2) * num;
							float f = (float)k / (float)detailTerrain;
							f = Mathf.Pow(f, terrainSmoothMultiplier);
							b2 = Mathf.Lerp(hitInfo.point.y, b2, f);
							Vector4 item = new Vector4(hitInfo.point.x, b2, hitInfo.point.z, 0f - num2);
							list2.Add(item);
						}
						else
						{
							list2.Add(vector4);
						}
					}
				}
				for (int l = 0; l <= detailTerrain; l++)
				{
					Vector3 vector4 = Vector3.Lerp(vector, vector2, (float)l / (float)detailTerrain) + base.transform.position;
					if (Physics.Raycast(vector4 + Vector3.up * 500f, Vector3.down, out hitInfo, 10000f, maskCarve.value))
					{
						num = ((!noiseCarve) ? 0f : (Mathf.PerlinNoise(vector4.x * noiseSizeX, vector4.z * noiseSizeZ) * noiseMultiplierInside - noiseMultiplierInside * 0.5f));
						float num3 = magnitude * (0.5f - Mathf.Abs(0.5f - (float)l / (float)detailTerrain));
						float b3 = vector4.y + terrainCarve.Evaluate(num3) + terrainCarve.Evaluate(num3) * num;
						Mathf.Pow(1f - 2f * Mathf.Abs((float)l / (float)detailTerrain - 0.5f), terrainSmoothMultiplier);
						b3 = Mathf.Lerp(hitInfo.point.y, b3, 1f);
						Vector4 item2 = new Vector4(hitInfo.point.x, b3, hitInfo.point.z, num3);
						list2.Add(item2);
					}
					else
					{
						list2.Add(vector4);
					}
				}
				if (terrainAdditionalWidth > 0f)
				{
					for (int m = 1; m <= detailTerrain; m++)
					{
						Vector3 vector4 = Vector3.Lerp(vector2, b, (float)m / (float)detailTerrain) + base.transform.position;
						if (Physics.Raycast(vector4 + Vector3.up * 50f, Vector3.down, out hitInfo, 10000f, maskCarve.value))
						{
							num = ((!noiseCarve) ? 0f : (Mathf.PerlinNoise(vector4.x * noiseSizeX, vector4.z * noiseSizeZ) * noiseMultiplierOutside - noiseMultiplierOutside * 0.5f));
							float num4 = (float)m / (float)detailTerrain;
							num4 *= terrainAdditionalWidth;
							float b4 = vector4.y + terrainCarve.Evaluate(0f - num4) + terrainCarve.Evaluate(0f - num4) * num;
							float f2 = 1f - (float)m / (float)detailTerrain;
							f2 = Mathf.Pow(f2, terrainSmoothMultiplier);
							b4 = Mathf.Lerp(hitInfo.point.y, b4, f2);
							Vector4 item3 = new Vector4(hitInfo.point.x, b4, hitInfo.point.z, 0f - num4);
							list2.Add(item3);
						}
						else
						{
							list2.Add(vector4);
						}
					}
				}
				list.Add(list2);
			}
		}
		Mesh mesh = new Mesh();
		mesh.indexFormat = IndexFormat.UInt32;
		List<Vector3> list3 = new List<Vector3>();
		List<int> list4 = new List<int>();
		List<Vector2> list5 = new List<Vector2>();
		foreach (List<Vector4> item4 in list)
		{
			foreach (Vector4 item5 in item4)
			{
				list3.Add(item5);
			}
		}
		for (int n = 0; n < list.Count - 1; n++)
		{
			int count = list[n].Count;
			for (int num5 = 0; num5 < count - 1; num5++)
			{
				list4.Add(num5 + n * count);
				list4.Add(num5 + (n + 1) * count);
				list4.Add(num5 + 1 + n * count);
				list4.Add(num5 + 1 + n * count);
				list4.Add(num5 + (n + 1) * count);
				list4.Add(num5 + 1 + (n + 1) * count);
			}
		}
		foreach (List<Vector4> item6 in list)
		{
			foreach (Vector4 item7 in item6)
			{
				list5.Add(new Vector2(item7.w, 0f));
			}
		}
		mesh.SetVertices(list3);
		mesh.SetTriangles(list4, 0);
		mesh.SetUVs(0, list5);
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		mesh.RecalculateBounds();
		meshGO = new GameObject("TerrainMesh");
		meshGO.transform.parent = base.transform;
		meshGO.hideFlags = HideFlags.HideAndDontSave;
		meshGO.AddComponent<MeshFilter>();
		meshGO.transform.parent = base.transform;
		MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = new Material(Shader.Find("Debug Terrain Carve"));
		meshRenderer.sharedMaterial.color = new Color(0f, 0.5f, 0f);
		meshGO.transform.position = Vector3.zero;
		meshGO.GetComponent<MeshFilter>().sharedMesh = mesh;
		if (overrideRiverRender)
		{
			meshGO.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 5000;
		}
		else
		{
			meshGO.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 2980;
		}
	}

	public void TerrainCarve()
	{
		bool flag = false;
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		Physics.autoSyncTransforms = false;
		Terrain[] activeTerrains = Terrain.activeTerrains;
		foreach (Terrain terrain in activeTerrains)
		{
			TerrainData terrainData = terrain.terrainData;
			float y = terrain.transform.position.y;
			float x = terrain.terrainData.size.x;
			float y2 = terrain.terrainData.size.y;
			float z = terrain.terrainData.size.z;
			float num = 1f / z * (float)(terrainData.heightmapResolution - 1);
			float num2 = 1f / x * (float)(terrainData.heightmapResolution - 1);
			MeshCollider meshCollider = meshGO.gameObject.AddComponent<MeshCollider>();
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			int num3 = 5;
			int num4 = 0;
			_ = Vector3.zero;
			_ = Vector3.zero;
			for (num4 = 0; num4 < pointsUp.Count; num4 = Mathf.Clamp(num4 + num3 - 1, 0, pointsUp.Count))
			{
				int num5 = Mathf.Min(num4 + num3, pointsUp.Count);
				list.Clear();
				list2.Clear();
				for (int j = num4; j < num5; j++)
				{
					list.Add(base.transform.TransformPoint(pointsUp[j]));
					list2.Add(base.transform.TransformPoint(pointsDown[j]));
				}
				float num6 = float.MaxValue;
				float num7 = float.MinValue;
				float num8 = float.MaxValue;
				float num9 = float.MinValue;
				for (int k = 0; k < list.Count; k++)
				{
					Vector3 vector = list[k];
					if (num6 > vector.x)
					{
						num6 = vector.x;
					}
					if (num7 < vector.x)
					{
						num7 = vector.x;
					}
					if (num8 > vector.z)
					{
						num8 = vector.z;
					}
					if (num9 < vector.z)
					{
						num9 = vector.z;
					}
				}
				for (int l = 0; l < list2.Count; l++)
				{
					Vector3 vector2 = list2[l];
					if (num6 > vector2.x)
					{
						num6 = vector2.x;
					}
					if (num7 < vector2.x)
					{
						num7 = vector2.x;
					}
					if (num8 > vector2.z)
					{
						num8 = vector2.z;
					}
					if (num9 < vector2.z)
					{
						num9 = vector2.z;
					}
				}
				num6 -= terrain.transform.position.x + distSmooth;
				num7 -= terrain.transform.position.x - distSmooth;
				num8 -= terrain.transform.position.z + distSmooth;
				num9 -= terrain.transform.position.z - distSmooth;
				num6 *= num2;
				num7 *= num2;
				num8 *= num;
				num9 *= num;
				num7 = Mathf.Ceil(Mathf.Clamp(num7 + 1f, 0f, terrainData.heightmapResolution));
				num8 = Mathf.Floor(Mathf.Clamp(num8, 0f, terrainData.heightmapResolution));
				num9 = Mathf.Ceil(Mathf.Clamp(num9 + 1f, 0f, terrainData.heightmapResolution));
				num6 = Mathf.Floor(Mathf.Clamp(num6, 0f, terrainData.heightmapResolution));
				float[,] heights = terrainData.GetHeights((int)num6, (int)num8, (int)(num7 - num6), (int)(num9 - num8));
				Vector3 zero = Vector3.zero;
				_ = Vector3.zero;
				for (int m = 0; m < heights.GetLength(0); m++)
				{
					for (int n = 0; n < heights.GetLength(1); n++)
					{
						zero.x = ((float)n + num6) / num2 + terrain.transform.position.x;
						zero.z = ((float)m + num8) / num + terrain.transform.position.z;
						Ray ray = new Ray(zero + Vector3.up * 3000f, Vector3.down);
						if (meshCollider.Raycast(ray, out var hitInfo, 10000f))
						{
							float num10 = hitInfo.point.y - y;
							heights[m, n] = num10 / y2;
							if (flag)
							{
								Debug.DrawLine(hitInfo.point, hitInfo.point + Vector3.up * 0.5f, Color.magenta, 10f);
							}
						}
					}
				}
				terrainData.SetHeights((int)num6, (int)num8, heights);
			}
			UnityEngine.Object.DestroyImmediate(meshCollider);
			terrain.Flush();
		}
		Physics.autoSyncTransforms = autoSyncTransforms;
		if (meshGO != null)
		{
			UnityEngine.Object.DestroyImmediate(meshGO);
		}
	}

	public void TerrainPaintMeshBased()
	{
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		Physics.autoSyncTransforms = false;
		Terrain[] activeTerrains = Terrain.activeTerrains;
		foreach (Terrain terrain in activeTerrains)
		{
			TerrainData terrainData = terrain.terrainData;
			float x = terrain.terrainData.size.x;
			_ = terrain.terrainData.size;
			float z = terrain.terrainData.size.z;
			float num = 1f / z * (float)(terrainData.alphamapWidth - 1);
			float num2 = 1f / x * (float)(terrainData.alphamapHeight - 1);
			MeshCollider meshCollider = meshGO.gameObject.AddComponent<MeshCollider>();
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			int num3 = 5;
			int num4 = 0;
			_ = Vector3.zero;
			_ = Vector3.zero;
			for (num4 = 0; num4 < pointsUp.Count; num4 = Mathf.Clamp(num4 + num3 - 1, 0, pointsUp.Count))
			{
				int num5 = Mathf.Min(num4 + num3, pointsUp.Count);
				list.Clear();
				list2.Clear();
				for (int j = num4; j < num5; j++)
				{
					list.Add(base.transform.TransformPoint(pointsUp[j]));
					list2.Add(base.transform.TransformPoint(pointsDown[j]));
				}
				float num6 = float.MaxValue;
				float num7 = float.MinValue;
				float num8 = float.MaxValue;
				float num9 = float.MinValue;
				for (int k = 0; k < list.Count; k++)
				{
					Vector3 vector = list[k];
					if (num6 > vector.x)
					{
						num6 = vector.x;
					}
					if (num7 < vector.x)
					{
						num7 = vector.x;
					}
					if (num8 > vector.z)
					{
						num8 = vector.z;
					}
					if (num9 < vector.z)
					{
						num9 = vector.z;
					}
				}
				for (int l = 0; l < list2.Count; l++)
				{
					Vector3 vector2 = list2[l];
					if (num6 > vector2.x)
					{
						num6 = vector2.x;
					}
					if (num7 < vector2.x)
					{
						num7 = vector2.x;
					}
					if (num8 > vector2.z)
					{
						num8 = vector2.z;
					}
					if (num9 < vector2.z)
					{
						num9 = vector2.z;
					}
				}
				num6 -= terrain.transform.position.x + distSmooth;
				num7 -= terrain.transform.position.x - distSmooth;
				num8 -= terrain.transform.position.z + distSmooth;
				num9 -= terrain.transform.position.z - distSmooth;
				num6 *= num2;
				num7 *= num2;
				num8 *= num;
				num9 *= num;
				num6 = Mathf.Floor(Mathf.Clamp(num6, 0f, terrainData.alphamapWidth));
				num7 = Mathf.Ceil(Mathf.Clamp(num7 + 1f, 0f, terrainData.alphamapWidth));
				num8 = Mathf.Floor(Mathf.Clamp(num8, 0f, terrainData.alphamapHeight));
				num9 = Mathf.Ceil(Mathf.Clamp(num9 + 1f, 0f, terrainData.alphamapHeight));
				float[,,] alphamaps = terrainData.GetAlphamaps((int)num6, (int)num8, (int)(num7 - num6), (int)(num9 - num8));
				Vector3 zero = Vector3.zero;
				_ = Vector3.zero;
				float num10 = 0f;
				for (int m = 0; m < alphamaps.GetLength(0); m++)
				{
					for (int n = 0; n < alphamaps.GetLength(1); n++)
					{
						zero.x = ((float)n + num6) / num2 + terrain.transform.position.x;
						zero.z = ((float)m + num8) / num + terrain.transform.position.z;
						Ray ray = new Ray(zero + Vector3.up * 3000f, Vector3.down);
						if (!meshCollider.Raycast(ray, out var hitInfo, 10000f))
						{
							continue;
						}
						float x2 = hitInfo.textureCoord.x;
						if (!mixTwoSplatMaps)
						{
							num10 = ((!noisePaint) ? 0f : ((!(x2 >= 0f)) ? (Mathf.PerlinNoise(hitInfo.point.x * noiseSizeXPaint, hitInfo.point.z * noiseSizeZPaint) * noiseMultiplierOutsidePaint - noiseMultiplierOutsidePaint * 0.5f) : (Mathf.PerlinNoise(hitInfo.point.x * noiseSizeXPaint, hitInfo.point.z * noiseSizeZPaint) * noiseMultiplierInsidePaint - noiseMultiplierInsidePaint * 0.5f)));
							float num11 = alphamaps[m, n, currentSplatMap];
							alphamaps[m, n, currentSplatMap] = Mathf.Clamp01(Mathf.Lerp(alphamaps[m, n, currentSplatMap], 1f, terrainPaintCarve.Evaluate(x2) + terrainPaintCarve.Evaluate(x2) * num10));
							for (int num12 = 0; num12 < terrainData.terrainLayers.Length; num12++)
							{
								if (num12 != currentSplatMap)
								{
									alphamaps[m, n, num12] = ((num11 == 1f) ? 0f : Mathf.Clamp01(alphamaps[m, n, num12] * ((1f - alphamaps[m, n, currentSplatMap]) / (1f - num11))));
								}
							}
						}
						else
						{
							num10 = ((!(x2 >= 0f)) ? (Mathf.PerlinNoise(hitInfo.point.x * noiseSizeXPaint, hitInfo.point.z * noiseSizeZPaint) * noiseMultiplierOutsidePaint - noiseMultiplierOutsidePaint * 0.5f) : (Mathf.PerlinNoise(hitInfo.point.x * noiseSizeXPaint, hitInfo.point.z * noiseSizeZPaint) * noiseMultiplierInsidePaint - noiseMultiplierInsidePaint * 0.5f));
							float num13 = alphamaps[m, n, currentSplatMap];
							alphamaps[m, n, currentSplatMap] = Mathf.Clamp01(Mathf.Lerp(alphamaps[m, n, currentSplatMap], 1f, terrainPaintCarve.Evaluate(x2)));
							for (int num14 = 0; num14 < terrainData.terrainLayers.Length; num14++)
							{
								if (num14 != currentSplatMap)
								{
									alphamaps[m, n, num14] = ((num13 == 1f) ? 0f : Mathf.Clamp01(alphamaps[m, n, num14] * ((1f - alphamaps[m, n, currentSplatMap]) / (1f - num13))));
								}
							}
							if (num10 > 0f)
							{
								num13 = alphamaps[m, n, secondSplatMap];
								alphamaps[m, n, secondSplatMap] = Mathf.Clamp01(Mathf.Lerp(alphamaps[m, n, secondSplatMap], 1f, num10));
								for (int num15 = 0; num15 < terrainData.terrainLayers.Length; num15++)
								{
									if (num15 != secondSplatMap)
									{
										alphamaps[m, n, num15] = ((num13 == 1f) ? 0f : Mathf.Clamp01(alphamaps[m, n, num15] * ((1f - alphamaps[m, n, secondSplatMap]) / (1f - num13))));
									}
								}
							}
						}
						if (!addCliffSplatMap)
						{
							continue;
						}
						if (x2 >= 0f)
						{
							if (!(Vector3.Angle(hitInfo.normal, Vector3.up) > cliffAngle))
							{
								continue;
							}
							float num16 = alphamaps[m, n, cliffSplatMap];
							alphamaps[m, n, cliffSplatMap] = cliffBlend;
							for (int num17 = 0; num17 < terrainData.terrainLayers.Length; num17++)
							{
								if (num17 != cliffSplatMap)
								{
									alphamaps[m, n, num17] = ((num16 == 1f) ? 0f : Mathf.Clamp01(alphamaps[m, n, num17] * ((1f - alphamaps[m, n, cliffSplatMap]) / (1f - num16))));
								}
							}
						}
						else
						{
							if (!(Vector3.Angle(hitInfo.normal, Vector3.up) > cliffAngleOutside))
							{
								continue;
							}
							float num18 = alphamaps[m, n, cliffSplatMapOutside];
							alphamaps[m, n, cliffSplatMapOutside] = cliffBlendOutside;
							for (int num19 = 0; num19 < terrainData.terrainLayers.Length; num19++)
							{
								if (num19 != cliffSplatMapOutside)
								{
									alphamaps[m, n, num19] = ((num18 == 1f) ? 0f : Mathf.Clamp01(alphamaps[m, n, num19] * ((1f - alphamaps[m, n, cliffSplatMapOutside]) / (1f - num18))));
								}
							}
						}
					}
				}
				terrainData.SetAlphamaps((int)num6, (int)num8, alphamaps);
			}
			UnityEngine.Object.DestroyImmediate(meshCollider);
			terrain.Flush();
		}
		Physics.autoSyncTransforms = autoSyncTransforms;
		if (meshGO != null)
		{
			UnityEngine.Object.DestroyImmediate(meshGO);
		}
	}

	public void TerrainClearFoliage(bool details = true)
	{
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		Physics.autoSyncTransforms = false;
		Terrain[] activeTerrains = Terrain.activeTerrains;
		foreach (Terrain terrain in activeTerrains)
		{
			TerrainData terrainData = terrain.terrainData;
			Transform transform = terrain.transform;
			_ = terrain.transform.position;
			float x = terrain.terrainData.size.x;
			_ = terrain.terrainData.size;
			float z = terrain.terrainData.size.z;
			float num = 1f / z * (float)terrainData.detailWidth;
			float num2 = 1f / x * (float)terrainData.detailHeight;
			MeshCollider meshCollider = meshGO.gameObject.AddComponent<MeshCollider>();
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			int num3 = 5;
			int num4 = 0;
			_ = Vector3.zero;
			_ = Vector3.zero;
			Vector3 zero = Vector3.zero;
			if (details)
			{
				for (num4 = 0; num4 < pointsUp.Count; num4 = Mathf.Clamp(num4 + num3 - 1, 0, pointsUp.Count))
				{
					int num5 = Mathf.Min(num4 + num3, pointsUp.Count);
					Mathf.Min(num3, pointsUp.Count - num4);
					list.Clear();
					list2.Clear();
					for (int j = num4; j < num5; j++)
					{
						list.Add(base.transform.TransformPoint(pointsUp[j]));
						list2.Add(base.transform.TransformPoint(pointsDown[j]));
					}
					float num6 = float.MaxValue;
					float num7 = float.MinValue;
					float num8 = float.MaxValue;
					float num9 = float.MinValue;
					for (int k = 0; k < list.Count; k++)
					{
						Vector3 vector = list[k];
						if (num6 > vector.x)
						{
							num6 = vector.x;
						}
						if (num7 < vector.x)
						{
							num7 = vector.x;
						}
						if (num8 > vector.z)
						{
							num8 = vector.z;
						}
						if (num9 < vector.z)
						{
							num9 = vector.z;
						}
					}
					for (int l = 0; l < list2.Count; l++)
					{
						Vector3 vector2 = list2[l];
						if (num6 > vector2.x)
						{
							num6 = vector2.x;
						}
						if (num7 < vector2.x)
						{
							num7 = vector2.x;
						}
						if (num8 > vector2.z)
						{
							num8 = vector2.z;
						}
						if (num9 < vector2.z)
						{
							num9 = vector2.z;
						}
					}
					num6 -= transform.position.x + distanceClearFoliage;
					num7 -= transform.position.x - distanceClearFoliage;
					num8 -= transform.position.z + distanceClearFoliage;
					num9 -= transform.position.z - distanceClearFoliage;
					num6 *= num2;
					num7 *= num2;
					num8 *= num;
					num9 *= num;
					num6 = Mathf.Floor(Mathf.Clamp(num6, 0f, terrainData.detailWidth));
					num7 = Mathf.Ceil(Mathf.Clamp(num7 + 1f, 0f, terrainData.detailWidth));
					num8 = Mathf.Floor(Mathf.Clamp(num8, 0f, terrainData.detailHeight));
					num9 = Mathf.Ceil(Mathf.Clamp(num9 + 1f, 0f, terrainData.detailHeight));
					if (num7 - num6 > 0f && num9 - num8 > 0f)
					{
						for (int m = 0; m < terrainData.detailPrototypes.Length; m++)
						{
							int[,] detailLayer = terrainData.GetDetailLayer((int)num6, (int)num8, (int)(num7 - num6), (int)(num9 - num8), m);
							for (int n = 0; n < detailLayer.GetLength(0); n++)
							{
								for (int num10 = 0; num10 < detailLayer.GetLength(1); num10++)
								{
									zero.x = ((float)num10 + num6) / num2 + terrain.transform.position.x;
									zero.z = ((float)n + num8) / num + terrain.transform.position.z;
									Ray ray = new Ray(zero + Vector3.up * 3000f, Vector3.down);
									if (meshCollider.Raycast(ray, out var _, 10000f))
									{
										detailLayer[n, num10] = 0;
									}
								}
							}
							terrainData.SetDetailLayer((int)num6, (int)num8, m, detailLayer);
						}
					}
				}
			}
			else
			{
				List<TreeInstance> list3 = new List<TreeInstance>();
				TreeInstance[] treeInstances = terrainData.treeInstances;
				for (int num11 = 0; num11 < treeInstances.Length; num11++)
				{
					TreeInstance item = treeInstances[num11];
					zero.x = item.position.x * x + transform.position.x;
					zero.z = item.position.z * z + transform.position.z;
					Ray ray2 = new Ray(zero + Vector3.up * 3000f, Vector3.down);
					if (!meshCollider.Raycast(ray2, out var _, 10000f))
					{
						list3.Add(item);
					}
				}
				terrainData.treeInstances = list3.ToArray();
			}
			UnityEngine.Object.DestroyImmediate(meshCollider);
			terrain.Flush();
		}
		Physics.autoSyncTransforms = autoSyncTransforms;
		if (meshGO != null)
		{
			UnityEngine.Object.DestroyImmediate(meshGO);
		}
	}

	private float FlowCalculate(float u, float normalY, Vector3 vertice)
	{
		float num = (noiseflowMap ? (Mathf.PerlinNoise(vertice.x * noiseSizeXflowMap, vertice.z * noiseSizeZflowMap) * noiseMultiplierflowMap - noiseMultiplierflowMap * 0.5f) : 0f) * Mathf.Pow(Mathf.Clamp(normalY, 0f, 1f), 5f);
		return Mathf.Lerp(flowWaterfall.Evaluate(u), flowFlat.Evaluate(u) + num, Mathf.Clamp(normalY, 0f, 1f));
	}
}
