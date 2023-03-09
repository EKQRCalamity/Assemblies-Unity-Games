using System.Collections;
using UnityEngine;

public class AstralPlaneSpinner : MonoBehaviour
{
	[SerializeField]
	private Transform[] spaceLayers;

	private void Start()
	{
		StartCoroutine(rotate_space_bg_cr());
	}

	private IEnumerator rotate_space_bg_cr()
	{
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
			spaceLayers[0].Rotate(new Vector3(0f, 0f, 0.3f));
			spaceLayers[1].Rotate(new Vector3(0f, 0f, 0.5f));
			spaceLayers[2].Rotate(new Vector3(0f, 0f, 1f));
		}
	}
}
