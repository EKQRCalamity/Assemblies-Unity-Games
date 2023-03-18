using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class TileableGeo : MonoBehaviour, IActionable
{
	[Serializable]
	public struct LayerPerGeoDirection
	{
		public TILEABLE_GEO_DIRECTIONS direction;

		public string layer;
	}

	public enum TILEABLE_GEO_STATES
	{
		HIDDEN,
		SHOWING,
		SHOWN,
		HIDING
	}

	public enum TILEABLE_GEO_DIRECTIONS
	{
		UP,
		RIGHT,
		DOWN,
		LEFT
	}

	[SerializeField]
	[FoldoutGroup("Attached References", 0)]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	[FoldoutGroup("Attached References", 0)]
	private Collider2D collider;

	[SerializeField]
	[FoldoutGroup("Attached References", 0)]
	private Animator animator;

	[SerializeField]
	[FoldoutGroup("Attached References", 0)]
	private Transform rotationParent;

	[SerializeField]
	[FoldoutGroup("Attached References", 0)]
	private Animator rootCoreAnimator;

	[SerializeField]
	[BoxGroup("Relic Settings", true, false, 0)]
	private bool affectedByRelic = true;

	[ShowIf("affectedByRelic", true)]
	[SerializeField]
	[BoxGroup("Relic Settings", true, false, 0)]
	private float relicEffectiveRadius = 9f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private TILEABLE_GEO_DIRECTIONS geoDirection;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private int maxSize;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float secondsToGrow;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private AnimationCurve growCurve;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public List<LayerPerGeoDirection> layersPerGeoDirection;

	[FoldoutGroup("Graphic Settings", false, 0)]
	public bool flipGraphic;

	[FoldoutGroup("Graphic Settings", false, 0)]
	public int distanceBetweenBodySprites = 2;

	[FoldoutGroup("Graphic Settings", false, 0)]
	public List<GameObject> bodyParts;

	[FoldoutGroup("Graphic Settings", false, 0)]
	public GameObject bodyPartPrefab;

	[FoldoutGroup("Graphic Settings", false, 0)]
	public Transform bodyPartsParent;

	[FoldoutGroup("Graphic Settings", false, 0)]
	public string growAnimatorBool = "GROW";

	[FoldoutGroup("Audio", false, 0)]
	[EventRef]
	public string growAnimationFx;

	[SerializeField]
	[FoldoutGroup("Debug", 0)]
	public TILEABLE_GEO_STATES currentState;

	[SerializeField]
	[FoldoutGroup("Debug", 0)]
	public float currentSize = 5f;

	private float secondsBetweenTileAnim = 0.5f;

	private float timePerTile;

	private float animationTimePerTile;

	private float animatorSpeedPerTile;

	private Coroutine currentCoroutine;

	private EventInstance grownAnimationAudioInstance;

	public bool Locked { get; set; }

	private void Awake()
	{
		SetSize(0f);
		collider.enabled = false;
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		Debug.Log(other.collider.tag);
	}

	private void Update()
	{
		if (affectedByRelic)
		{
			CheckDistanceToPenitent();
		}
	}

	private void CheckDistanceToPenitent()
	{
		if (Core.Logic.Penitent != null)
		{
			string idRelic = "RE10";
			Vector2 a = Core.Logic.Penitent.transform.position;
			float num = Vector2.Distance(a, base.transform.position);
			if (num < relicEffectiveRadius && currentState == TILEABLE_GEO_STATES.HIDDEN && Core.InventoryManager.IsRelicEquipped(idRelic))
			{
				Show();
			}
			else if (currentState == TILEABLE_GEO_STATES.SHOWN && (!Core.InventoryManager.IsRelicEquipped(idRelic) || num > relicEffectiveRadius))
			{
				Hide();
			}
		}
	}

	private void SetSize(float newSize)
	{
		currentSize = newSize;
		spriteRenderer.size = new Vector2(spriteRenderer.size.x, newSize);
	}

	private void SetLayer(TILEABLE_GEO_DIRECTIONS dir)
	{
		LayerPerGeoDirection layerPerGeoDirection = layersPerGeoDirection.Find((LayerPerGeoDirection x) => x.direction == dir);
		GetComponentInChildren<Collider2D>().gameObject.layer = LayerMask.NameToLayer(layerPerGeoDirection.layer);
	}

	private void SetDirection(TILEABLE_GEO_DIRECTIONS dir)
	{
		float z = 0f;
		switch (dir)
		{
		case TILEABLE_GEO_DIRECTIONS.UP:
			z = 180f;
			break;
		case TILEABLE_GEO_DIRECTIONS.RIGHT:
			z = 90f;
			break;
		case TILEABLE_GEO_DIRECTIONS.DOWN:
			z = 0f;
			break;
		case TILEABLE_GEO_DIRECTIONS.LEFT:
			z = -90f;
			break;
		}
		rotationParent.transform.rotation = Quaternion.Euler(0f, 0f, z);
	}

	[BoxGroup("Design Settings", true, false, 0)]
	[Button("Apply direction and size", ButtonSizes.Large)]
	public void SetGrownState()
	{
		SetDirection(geoDirection);
		SetSize(maxSize);
		SetLayer(geoDirection);
	}

	private IEnumerator ChangeSizeCoroutine(float duration, float targetSize, AnimationCurve curve, Action callback)
	{
		float counter = 0f;
		float originSize = currentSize;
		while (counter < duration)
		{
			float normalizedValue = counter / duration;
			float curveValue = curve.Evaluate(normalizedValue);
			float newSize = Mathf.Lerp(originSize, targetSize, curveValue);
			SetSize(newSize);
			counter += Time.deltaTime;
			yield return null;
		}
		SetSize(targetSize);
		callback();
	}

	private void Show()
	{
		rootCoreAnimator.SetBool("ACTIVE", value: true);
		StartCoroutine(ChangeSizeCoroutine(secondsToGrow, maxSize, growCurve, OnGrowFinished));
		collider.enabled = true;
		currentState = TILEABLE_GEO_STATES.SHOWING;
		TriggerGrow();
		if (rootCoreAnimator.GetComponent<SpriteRenderer>().isVisible)
		{
			PlayGrowRootsFx();
		}
	}

	private void OnGrowFinished()
	{
		rootCoreAnimator.SetBool("ACTIVE", value: false);
		currentState = TILEABLE_GEO_STATES.SHOWN;
		StopGrowRootsFx();
	}

	public void Hide()
	{
		rootCoreAnimator.SetBool("ACTIVE", value: true);
		StartCoroutine(ChangeSizeCoroutine(secondsToGrow, 0f, growCurve, OnShortenFinished));
		currentState = TILEABLE_GEO_STATES.HIDING;
		TriggerShrink();
		if (rootCoreAnimator.GetComponent<SpriteRenderer>().isVisible)
		{
			PlayGrowRootsFx();
		}
	}

	private void OnShortenFinished()
	{
		collider.enabled = false;
		rootCoreAnimator.SetBool("ACTIVE", value: false);
		currentState = TILEABLE_GEO_STATES.HIDDEN;
		StopGrowRootsFx();
	}

	public void Use()
	{
		if (currentState == TILEABLE_GEO_STATES.HIDDEN)
		{
			Show();
		}
		else if (currentState == TILEABLE_GEO_STATES.SHOWN)
		{
			Hide();
		}
	}

	private void Start()
	{
		CreateBeamBodies();
		ChangeAnimationSpeed();
		SetLayer(geoDirection);
	}

	private void ChangeAnimationSpeed()
	{
		float num = 0.5f;
		timePerTile = secondsToGrow / (float)bodyParts.Count;
		animatorSpeedPerTile = num / timePerTile;
		animationTimePerTile = num / animatorSpeedPerTile;
		foreach (GameObject bodyPart in bodyParts)
		{
			bodyPart.GetComponent<Animator>().speed = animatorSpeedPerTile;
		}
		secondsBetweenTileAnim = animationTimePerTile;
	}

	private void CreateBeamBodies()
	{
		bodyParts = new List<GameObject>();
		int num = maxSize / distanceBetweenBodySprites;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(bodyPartPrefab, bodyPartsParent);
			bodyParts.Add(gameObject);
			gameObject.transform.localPosition = new Vector3(i * distanceBetweenBodySprites, 0f);
			gameObject.GetComponentInChildren<SpriteRenderer>().flipY = flipGraphic;
		}
		bodyParts[bodyParts.Count - 1].GetComponentInChildren<Animator>().SetBool("LAST_TILE", value: true);
	}

	private IEnumerator DelayedShrinkCoroutine(float delayBetweenTiles)
	{
		for (int i = bodyParts.Count - 1; i >= 0; i--)
		{
			bodyParts[i].GetComponentInChildren<Animator>().SetBool(growAnimatorBool, value: false);
			yield return new WaitForSeconds(delayBetweenTiles);
		}
	}

	private IEnumerator DelayedGrowCoroutine(float delayBetweenTiles)
	{
		for (int i = 0; i < bodyParts.Count; i++)
		{
			bodyParts[i].GetComponentInChildren<Animator>().SetBool(growAnimatorBool, value: true);
			yield return new WaitForSeconds(delayBetweenTiles);
		}
		rootCoreAnimator.SetBool("ACTIVE", value: false);
	}

	public void TriggerGrow()
	{
		StartCoroutine(DelayedGrowCoroutine(secondsBetweenTileAnim));
	}

	public void TriggerShrink()
	{
		StartCoroutine(DelayedShrinkCoroutine(secondsBetweenTileAnim));
	}

	public void PlayGrowRootsFx()
	{
		StopGrowRootsFx();
		if (!grownAnimationAudioInstance.isValid() && !string.IsNullOrEmpty(growAnimationFx))
		{
			grownAnimationAudioInstance = Core.Audio.CreateEvent(growAnimationFx);
			grownAnimationAudioInstance.start();
		}
	}

	public void StopGrowRootsFx()
	{
		if (grownAnimationAudioInstance.isValid())
		{
			grownAnimationAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			grownAnimationAudioInstance.release();
			grownAnimationAudioInstance = default(EventInstance);
		}
	}

	private void OnDestroy()
	{
		StopGrowRootsFx();
	}
}
