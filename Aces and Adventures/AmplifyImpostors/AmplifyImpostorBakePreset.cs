using System.Collections.Generic;
using UnityEngine;

namespace AmplifyImpostors;

[CreateAssetMenu(fileName = "New Bake Preset", order = 86)]
public class AmplifyImpostorBakePreset : ScriptableObject
{
	[SerializeField]
	public Shader BakeShader;

	[SerializeField]
	public Shader RuntimeShader;

	[SerializeField]
	public PresetPipeline Pipeline;

	[SerializeField]
	public int AlphaIndex;

	[SerializeField]
	public List<TextureOutput> Output = new List<TextureOutput>();
}
