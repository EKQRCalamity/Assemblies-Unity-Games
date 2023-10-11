using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "LakePolygonProfile", menuName = "LakePolygonProfile", order = 1)]
public class LakePolygonProfile : ScriptableObject
{
	public Material lakeMaterial;

	public float distSmooth = 5f;

	public float uvScale = 1f;

	public float maximumTriangleSize = 50f;

	public float traingleDensity = 0.2f;

	public bool receiveShadows;

	public ShadowCastingMode shadowCastingMode;

	public float automaticFlowMapScale = 0.2f;

	public bool noiseflowMap;

	public float noiseMultiplierflowMap = 1f;

	public float noiseSizeXflowMap = 0.2f;

	public float noiseSizeZflowMap = 0.2f;

	public AnimationCurve terrainCarve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(10f, -2f));

	public float terrainSmoothMultiplier = 1f;

	public AnimationCurve terrainPaintCarve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public bool noiseCarve;

	public float noiseMultiplierInside = 1f;

	public float noiseMultiplierOutside = 0.25f;

	public float noiseSizeX = 0.2f;

	public float noiseSizeZ = 0.2f;

	public int currentSplatMap = 1;

	public bool noisePaint;

	public float noiseMultiplierInsidePaint = 1f;

	public float noiseMultiplierOutsidePaint = 0.5f;

	public float noiseSizeXPaint = 0.2f;

	public float noiseSizeZPaint = 0.2f;

	public bool mixTwoSplatMaps;

	public int secondSplatMap = 1;

	public bool addCliffSplatMap;

	public int cliffSplatMap = 1;

	public float cliffAngle = 25f;

	public float cliffBlend = 1f;

	public int cliffSplatMapOutside = 1;

	public float cliffAngleOutside = 25f;

	public float cliffBlendOutside = 1f;

	public float distanceClearFoliage = 1f;

	public float distanceClearFoliageTrees = 1f;

	public int biomeType;
}
