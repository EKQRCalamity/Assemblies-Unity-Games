using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "SplineProfile", menuName = "SplineProfile", order = 1)]
public class SplineProfile : ScriptableObject
{
	public Material splineMaterial;

	public AnimationCurve meshCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float minVal = 0.5f;

	public float maxVal = 0.5f;

	public int vertsInShape = 3;

	public float traingleDensity = 0.2f;

	public float uvScale = 3f;

	public bool uvRotation = true;

	public bool receiveShadows;

	public ShadowCastingMode shadowCastingMode;

	public AnimationCurve flowFlat = new AnimationCurve(new Keyframe(0f, 0.025f), new Keyframe(0.5f, 0.05f), new Keyframe(1f, 0.025f));

	public AnimationCurve flowWaterfall = new AnimationCurve(new Keyframe(0f, 0.25f), new Keyframe(1f, 0.25f));

	public bool noiseflowMap;

	public float noiseMultiplierflowMap = 0.1f;

	public float noiseSizeXflowMap = 2f;

	public float noiseSizeZflowMap = 2f;

	public float floatSpeed = 10f;

	public AnimationCurve terrainCarve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(10f, -2f));

	public float distSmooth = 5f;

	public float distSmoothStart = 1f;

	public AnimationCurve terrainPaintCarve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public LayerMask maskCarve;

	public bool noiseCarve;

	public float noiseMultiplierInside = 1f;

	public float noiseMultiplierOutside = 0.25f;

	public float noiseSizeX = 0.2f;

	public float noiseSizeZ = 0.2f;

	public float terrainSmoothMultiplier = 5f;

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

	public bool noisePaint;

	public float noiseMultiplierInsidePaint = 0.25f;

	public float noiseMultiplierOutsidePaint = 0.25f;

	public float noiseSizeXPaint = 0.2f;

	public float noiseSizeZPaint = 0.2f;

	public float simulatedRiverLength = 100f;

	public int simulatedRiverPoints = 10;

	public float simulatedMinStepSize = 1f;

	public bool simulatedNoUp;

	public bool simulatedBreakOnUp = true;

	public bool noiseWidth;

	public float noiseMultiplierWidth = 4f;

	public float noiseSizeWidth = 0.5f;

	public int biomeType;
}
