using FMODUnity;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.GhostTrap;

public class GhostTriggerTrap : MonoBehaviour
{
	public LayerMask TargetMask;

	public GameObject GhostTriggerPrefab;

	public int NumPages = 3;

	public Vector2 Offset;

	public float TriggerTimeOffset = 0.1f;

	private float _currentTriggerLapse;

	private float _distanceOffset = 0.3f;

	[FoldoutGroup("Audio", 0)]
	[EventRef]
	public string BlowPaperSheetFx;

	private BoxCollider2D TrapCollider { get; set; }

	private GameObject Target { get; set; }

	private bool TargetIsInsideTrap { get; set; }

	private Vector3 OldSpawnPosition { get; set; }

	private void Awake()
	{
		TrapCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		if (GhostTriggerPrefab != null)
		{
			PoolManager.Instance.CreatePool(GhostTriggerPrefab, NumPages * 5);
		}
	}

	private void Update()
	{
		if (TargetIsInsideTrap)
		{
			_currentTriggerLapse -= Time.deltaTime;
			float num = Vector2.Distance(Target.transform.position, OldSpawnPosition);
			if (_currentTriggerLapse <= 0f && num > _distanceOffset)
			{
				SpawnFlyingPages(Target.transform.position);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!(GhostTriggerPrefab == null) && (TargetMask.value & (1 << other.gameObject.layer)) > 0)
		{
			Target = other.gameObject;
			TargetIsInsideTrap = true;
			Vector3 position = Target.transform.position;
			SpawnFlyingPages(position);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((TargetMask.value & (1 << other.gameObject.layer)) > 0)
		{
			TargetIsInsideTrap = false;
		}
	}

	public void SpawnFlyingPages(Vector3 position)
	{
		Vector3 position2 = GetSpawnPagesPosition(position) + (Vector3)Offset;
		OldSpawnPosition = new Vector2(position2.x, position2.y);
		_currentTriggerLapse = TriggerTimeOffset;
		int num = Random.Range(1, NumPages + 1);
		PlayBlowPaperSheets();
		for (byte b = 0; b < num; b = (byte)(b + 1))
		{
			PoolManager.Instance.ReuseObject(GhostTriggerPrefab, position2, Quaternion.identity);
		}
	}

	private Vector3 GetSpawnPagesPosition(Vector3 targetPosition)
	{
		float min = TrapCollider.bounds.min.x + 0.1f;
		float max = TrapCollider.bounds.max.x - 0.1f;
		targetPosition.x = Mathf.Clamp(targetPosition.x, min, max);
		return targetPosition;
	}

	private void PlayBlowPaperSheets()
	{
		if (!string.IsNullOrEmpty(BlowPaperSheetFx))
		{
			Core.Audio.PlaySfx(BlowPaperSheetFx);
		}
	}
}
