using UnityEngine;

public class SampleTextureColor : MonoBehaviour
{
	public Texture2D texture;

	public Vector2 uv;

	public Vector2 tiling = new Vector2(1f, 1f);

	public Vector2 offsetVelocity;

	public float saturation = 1f;

	public float value = 1f;

	public WrapMethod wrapMethod;

	public ColorEvent onColorChange;

	private void Update()
	{
		if ((bool)texture)
		{
			Vector2 vector = new Vector2(uv.x * tiling.x + offsetVelocity.x * Time.time, uv.y * tiling.y + offsetVelocity.y * Time.time);
			onColorChange?.Invoke(texture.GetPixelBilinear(wrapMethod.WrapShift(vector.x), wrapMethod.WrapShift(vector.y)).AdjustSaturation(saturation).AdjustBrightness(value));
		}
	}
}
