using Gameplay.GameControllers.Environment;
using UnityEngine;

namespace Gameplay.UI.Others.UIGameEditor;

[ExecuteInEditMode]
public class ClimbPreview : MonoBehaviour
{
	public SpriteRenderer preview;

	public CliffLede cliffLede;

	private void Start()
	{
		if (Application.isPlaying)
		{
			preview.enabled = false;
		}
		else if (!Application.isPlaying)
		{
			preview.flipX = cliffLede.cliffLedeSide == CliffLede.CliffLedeSide.Left;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying && preview != null)
		{
			preview.flipX = cliffLede.cliffLedeSide == CliffLede.CliffLedeSide.Left;
		}
	}
}
