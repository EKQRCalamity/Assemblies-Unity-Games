using UnityEngine;

public class ControlMapperResizer : MonoBehaviour
{
	private float cachedSize;

	private void Update()
	{
		float num = Mathf.Clamp((float)Screen.width / (float)Screen.height / 1.7777778f, 0f, 1f);
		if (cachedSize != num)
		{
			cachedSize = num;
			base.transform.localScale = new Vector3(num, num);
		}
	}
}
