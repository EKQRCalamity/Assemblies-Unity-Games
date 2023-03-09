using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelBackground : AbstractPausableComponent
{
	[SerializeField]
	private float sunsetDuration;

	[SerializeField]
	private float sunsetTargetY;

	[SerializeField]
	private Transform skyLoopTransform;

	[SerializeField]
	private FlyingCowboyLevelOverlayScrollingSprite initialScrollingMidLayer;

	[SerializeField]
	private ScrollingSpriteSpawner[] initialFGSpawners;

	[SerializeField]
	private GameObject transitionBackground;

	[SerializeField]
	private GameObject phase3Background;

	[SerializeField]
	private ScrollingSprite phase3Scrolling;

	[SerializeField]
	private ScrollingSpriteSpawner[] phase3MidSpawners;

	[SerializeField]
	private GameObject phase3Foreground;

	[SerializeField]
	private GameObject phase3ForegroundStart;

	[SerializeField]
	private ScrollingSprite phase3ForegroundScrolling;

	[SerializeField]
	private ScrollingSpriteSpawner[] phase3FGSpawners;

	private float sunsetTimeElapsed;

	private float sunsetInitialY;

	private bool transitionStarted;

	private void Start()
	{
		sunsetInitialY = skyLoopTransform.position.y;
	}

	private void Update()
	{
		if (sunsetTimeElapsed < sunsetDuration)
		{
			sunsetTimeElapsed += CupheadTime.Delta;
			Vector3 position = skyLoopTransform.position;
			position.y = Mathf.Lerp(sunsetInitialY, sunsetTargetY, sunsetTimeElapsed / sunsetDuration);
			skyLoopTransform.position = position;
		}
	}

	public void BeginTransition()
	{
		if (transitionStarted)
		{
			return;
		}
		transitionStarted = true;
		initialScrollingMidLayer.looping = false;
		SpriteRenderer spriteRenderer = null;
		foreach (SpriteRenderer copyRenderer in initialScrollingMidLayer.copyRenderers)
		{
			if (spriteRenderer == null || spriteRenderer.transform.position.x < copyRenderer.transform.position.x)
			{
				spriteRenderer = copyRenderer;
			}
		}
		float x = spriteRenderer.sprite.bounds.size.x;
		float x2 = transitionBackground.GetComponent<SpriteRenderer>().bounds.size.x;
		Vector3 position = transitionBackground.transform.position;
		position.x = spriteRenderer.transform.position.x + x * 0.5f + x2 * 0.5f - initialScrollingMidLayer.speed * (float)CupheadTime.Delta;
		transitionBackground.transform.position = position;
		transitionBackground.SetActive(value: true);
		Vector3 position2 = phase3Background.transform.position;
		position2.x = position.x + x2 - phase3Scrolling.offset;
		phase3Background.transform.position = position2;
		phase3Background.SetActive(value: true);
		StartCoroutine(transitionScroll_cr(initialScrollingMidLayer.speed, x));
		position2 = phase3Foreground.transform.position;
		position2.x = transitionBackground.transform.position.x;
		phase3Foreground.transform.position = position2;
		phase3Foreground.SetActive(value: true);
		StartCoroutine(foregroundTransitionScroll_cr(initialScrollingMidLayer.speed));
	}

	private IEnumerator transitionScroll_cr(float speed, float size)
	{
		float displacement;
		for (float totalDisplacement = 0f; totalDisplacement < 3f * size; totalDisplacement += displacement)
		{
			yield return null;
			displacement = speed * (float)CupheadTime.Delta;
			Vector3 position = transitionBackground.transform.position;
			position.x -= displacement;
			transitionBackground.transform.position = position;
			if (phase3Scrolling.enabled)
			{
				continue;
			}
			position = phase3Background.transform.position;
			position.x -= displacement;
			phase3Background.transform.position = position;
			if (phase3Background.transform.position.x < 0f)
			{
				phase3Scrolling.enabled = true;
				ScrollingSpriteSpawner[] array = phase3MidSpawners;
				foreach (ScrollingSpriteSpawner scrollingSpriteSpawner in array)
				{
					scrollingSpriteSpawner.StartLoop(ensureInitialOffscreenSpawn: true);
				}
			}
		}
		initialScrollingMidLayer.gameObject.SetActive(value: false);
		transitionBackground.SetActive(value: false);
	}

	private IEnumerator foregroundTransitionScroll_cr(float speed)
	{
		Transform transform = phase3Foreground.transform;
		transform.AddPosition(400f);
		Transform startTransform = phase3ForegroundStart.transform;
		bool initialPropsDisabled = false;
		while (transform.position.x > 0f)
		{
			yield return null;
			float positionX = startTransform.position.x;
			if (!initialPropsDisabled && positionX <= 2480f)
			{
				initialPropsDisabled = true;
				ScrollingSpriteSpawner[] array = initialFGSpawners;
				foreach (ScrollingSpriteSpawner scrollingSpriteSpawner in array)
				{
					scrollingSpriteSpawner.StopAllCoroutines();
					scrollingSpriteSpawner.enabled = false;
				}
			}
			if (speed != phase3ForegroundScrolling.speed && positionX <= 2080f)
			{
				speed = phase3ForegroundScrolling.speed;
			}
			Vector3 position = transform.position;
			position.x -= speed * (float)CupheadTime.Delta;
			transform.position = position;
		}
		phase3ForegroundScrolling.enabled = true;
		ScrollingSpriteSpawner[] array2 = phase3FGSpawners;
		foreach (ScrollingSpriteSpawner scrollingSpriteSpawner2 in array2)
		{
			scrollingSpriteSpawner2.StartLoop(ensureInitialOffscreenSpawn: true);
		}
	}
}
