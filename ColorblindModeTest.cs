using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ColorblindModeTest : AbstractMonoBehaviour
{
	private Material mat;

	private void Start()
	{
		mat = GetComponent<SpriteRenderer>().material;
		StartCoroutine(flash_cr());
	}

	private IEnumerator flash_cr()
	{
		bool goingUp = true;
		float valMin = 0f;
		float valMax = 2f;
		float start = valMin;
		float end = valMax;
		float t = 0f;
		float time = 0.2f;
		while (true)
		{
			if (t < time)
			{
				t += (float)CupheadTime.Delta;
				mat.SetFloat("_Intensity", Mathf.Lerp(start, end, t / time));
				yield return null;
				continue;
			}
			mat.SetFloat("_Intensity", end);
			goingUp = !goingUp;
			start = ((!goingUp) ? valMax : valMin);
			end = ((!goingUp) ? valMin : valMax);
			t = 0f;
			yield return null;
		}
	}
}
