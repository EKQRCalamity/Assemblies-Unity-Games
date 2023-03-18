using UnityEngine;

[ExecuteInEditMode]
public class PIDI_2DSimpleReflection : MonoBehaviour
{
	[Range(-5f, 5f)]
	public float SurfaceLevel;

	private void OnWillRenderObject()
	{
		if (GetComponent<SpriteRenderer>().sharedMaterial.HasProperty("_SurfaceLevel"))
		{
			GetComponent<SpriteRenderer>().sharedMaterial.SetFloat("_SurfaceLevel", SurfaceLevel);
		}
	}
}
