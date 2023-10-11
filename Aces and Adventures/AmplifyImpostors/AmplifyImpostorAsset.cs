using System.Collections.Generic;
using UnityEngine;

namespace AmplifyImpostors;

[CreateAssetMenu(fileName = "New Impostor", order = 85)]
public class AmplifyImpostorAsset : ScriptableObject
{
	[SerializeField]
	public Material Material;

	[SerializeField]
	public Mesh Mesh;

	[HideInInspector]
	[SerializeField]
	public int Version;

	[SerializeField]
	public ImpostorType ImpostorType = ImpostorType.Octahedron;

	[HideInInspector]
	[SerializeField]
	public bool LockedSizes = true;

	[HideInInspector]
	[SerializeField]
	public int SelectedSize = 2048;

	[SerializeField]
	public Vector2 TexSize = new Vector2(2048f, 2048f);

	[HideInInspector]
	[SerializeField]
	public bool DecoupleAxisFrames;

	[SerializeField]
	[Range(1f, 32f)]
	public int HorizontalFrames = 16;

	[SerializeField]
	[Range(1f, 33f)]
	public int VerticalFrames = 16;

	[SerializeField]
	[Range(0f, 64f)]
	public int PixelPadding = 32;

	[SerializeField]
	[Range(4f, 16f)]
	public int MaxVertices = 8;

	[SerializeField]
	[Range(0f, 0.2f)]
	public float Tolerance = 0.15f;

	[SerializeField]
	[Range(0f, 1f)]
	public float NormalScale = 0.01f;

	[SerializeField]
	public Vector2[] ShapePoints = new Vector2[8]
	{
		new Vector2(0.15f, 0f),
		new Vector2(0.85f, 0f),
		new Vector2(1f, 0.15f),
		new Vector2(1f, 0.85f),
		new Vector2(0.85f, 1f),
		new Vector2(0.15f, 1f),
		new Vector2(0f, 0.85f),
		new Vector2(0f, 0.15f)
	};

	[SerializeField]
	public AmplifyImpostorBakePreset Preset;

	[SerializeField]
	public List<TextureOutput> OverrideOutput = new List<TextureOutput>();
}
