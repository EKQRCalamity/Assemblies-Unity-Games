using UnityEngine;

[RequireComponent(typeof(SpriteMask))]
public class AnimatedMask : MonoBehaviour
{
	public Sprite maskRequest;

	private Sprite currentMask;

	private SpriteMask mask;

	private void Start()
	{
		currentMask = maskRequest;
		mask = GetComponent<SpriteMask>();
		mask.sprite = currentMask;
	}

	private void LateUpdate()
	{
		if (currentMask != maskRequest)
		{
			currentMask = maskRequest;
			mask.sprite = currentMask;
		}
	}
}
