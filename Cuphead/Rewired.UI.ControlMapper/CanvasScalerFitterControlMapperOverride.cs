using UnityEngine;

namespace Rewired.UI.ControlMapper;

[RequireComponent(typeof(CanvasScalerExt))]
public class CanvasScalerFitterControlMapperOverride : MonoBehaviour
{
	[SerializeField]
	private Vector2 targetResolution = new Vector2(1885f, 600f);

	private CanvasScalerExt canvasScaler;

	private void OnEnable()
	{
		canvasScaler = GetComponent<CanvasScalerExt>();
	}

	private void LateUpdate()
	{
		canvasScaler.referenceResolution = targetResolution;
	}
}
