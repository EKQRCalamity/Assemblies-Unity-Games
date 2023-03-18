using UnityEngine;

namespace VisualEffects.Scripts;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class FakeLightRandomNoiseOffset : MonoBehaviour
{
	private void OnEnable()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		int nameID = Shader.PropertyToID("_NoiseOffset");
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		component.GetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetFloat(nameID, Random.value);
		component.SetPropertyBlock(materialPropertyBlock);
	}
}
