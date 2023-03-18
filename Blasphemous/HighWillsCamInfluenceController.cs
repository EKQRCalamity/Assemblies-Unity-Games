using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.OdinInspector;
using UnityEngine;

public class HighWillsCamInfluenceController : MonoBehaviour
{
	[BoxGroup("Cam Settings", true, false, 0)]
	public Vector2 NormalInfluence = new Vector2(5f, 0f);

	private void Update()
	{
		ProCamera2D.Instance.ApplyInfluence(NormalInfluence);
	}
}
