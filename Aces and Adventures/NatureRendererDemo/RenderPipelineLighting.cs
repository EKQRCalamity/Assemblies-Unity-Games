using UnityEngine;

namespace NatureRendererDemo;

[ExecuteInEditMode]
public class RenderPipelineLighting : MonoBehaviour
{
	[SerializeField]
	private GameObject _standardLighting;

	[SerializeField]
	private Material _standardSky;

	[SerializeField]
	private Material _standardTerrain;

	[SerializeField]
	private GameObject _standardVolume;

	[SerializeField]
	private GameObject _universalLighting;

	[SerializeField]
	private Material _universalSky;

	[SerializeField]
	private Material _universalTerrain;

	[SerializeField]
	private GameObject _universalVolume;

	[SerializeField]
	private GameObject _highDefinitionLighting;

	[SerializeField]
	private Material _highDefinitionSky;

	[SerializeField]
	private GameObject _highDefinitionVolume;

	[SerializeField]
	private Material _highDefinitionTerrain;

	private void OnValidate()
	{
		Awake();
	}

	private void Awake()
	{
	}
}
