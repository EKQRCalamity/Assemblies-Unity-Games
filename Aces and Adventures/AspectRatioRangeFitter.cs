using UnityEngine;
using UnityEngine.UI;

public class AspectRatioRangeFitter : AspectRatioFitter
{
	[Range(0.1f, 10f)]
	public float minAspect = 1.7777778f;

	[Range(0.1f, 10f)]
	public float maxAspect = 2.3703704f;

	private Int2 _previousScreenSize;

	protected override void Update()
	{
		base.Update();
		Int2 @int = new Int2(Screen.width, Screen.height);
		if (!(_previousScreenSize == @int))
		{
			base.aspectRatio = Mathf.Clamp(@int.aspectRatio, minAspect, maxAspect);
			_previousScreenSize = @int;
		}
	}
}
