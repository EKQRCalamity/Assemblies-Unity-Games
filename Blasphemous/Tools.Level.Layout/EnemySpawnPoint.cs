using System;
using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using Tools.Level.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tools.Level.Layout;

[SelectionBase]
public class EnemySpawnPoint : MonoBehaviour
{
	[FoldoutGroup("Spawning Options", 0)]
	[Tooltip("Spawns the enemy when the level is loaded.")]
	[OnValueChanged("SpawnEnabledEnemyValueChanged", false)]
	public bool SpawnEnabledEnemy = true;

	[FoldoutGroup("Spawning Options", 0)]
	[Tooltip("Delays enemy spawn and instantiate a VFX when is eventually spawned.")]
	[OnValueChanged("SpawnOnArenaValueChanged", false)]
	public bool SpawnOnArena;

	[FoldoutGroup("Spawning Options", 0)]
	[ShowIf("SpawnOnArena", true)]
	public float SpawningDelayOnArena = 0.8f;

	private WaitForSeconds _spawnOnArenaAwaiting;

	[FoldoutGroup("Spawning Options", 0)]
	[ShowIf("SpawnOnArena", true)]
	public GameObject SpawnVfx;

	[FoldoutGroup("Spawning Options", 0)]
	[ShowIf("SpawnOnArena", true)]
	public Vector2 SpawnEffectOffsetPosition;

	[FoldoutGroup("Spawning Options", 0)]
	[Tooltip("Spawns the enemy behaviour enabled.")]
	public bool EnableBehaviourOnLoad = true;

	[FoldoutGroup("Spawning Options", 0)]
	[Tooltip("Enables the enemy spawning persistence.")]
	public bool EnablePersistence = true;

	[FoldoutGroup("Spawning Options", 0)]
	[Tooltip("Disables enemy spawn. Only for Cherubs")]
	public bool EnemySpawnDisabled;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private SpriteRenderer preview;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[FormerlySerializedAs("enemy")]
	[InlineEditor(InlineEditorModes.LargePreview)]
	private GameObject selectedEnemy;

	private bool isEnemyEventsSuscribed;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Transform spawnPoint;

	[ValidateInput("ValidateInput", "Name must have more than 3 characters!", InfoMessageType.Error)]
	public string EntityName;

	public bool EnableInfluenceArea;

	[ShowIf("EnableInfluenceArea", true)]
	public float InfluenceAreaRadius = 1f;

	public Enemy SpawnedEnemy { get; private set; }

	public bool Consumed { get; set; }

	public int SpawningId { get; private set; }

	public Vector3 Position => spawnPoint.transform.position;

	public bool HasEnemySpawned { get; private set; }

	public GameObject SelectedEnemy => selectedEnemy;

	public event Action<EnemySpawnPoint, Enemy> OnEnemySpawned;

	private void Awake()
	{
		HasEnemySpawned = false;
		SpawningId = base.gameObject.GetHashCode();
		preview.enabled = false;
	}

	private void Start()
	{
		if (SpawnOnArena)
		{
			_spawnOnArenaAwaiting = new WaitForSeconds(SpawningDelayOnArena);
		}
		if (SpawnOnArena && SpawnVfx != null)
		{
			PoolManager.Instance.CreatePool(SpawnVfx, 1);
		}
	}

	public void CreateEnemy()
	{
		if (EnemySpawnDisabled)
		{
			return;
		}
		Consumed = Core.Logic.EnemySpawner.IsSpawnerConsumed(this);
		if (!EnablePersistence)
		{
			Consumed = false;
		}
		if (HasEnemySpawned || !selectedEnemy || !spawnPoint || Consumed)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(selectedEnemy, spawnPoint.position, Quaternion.identity);
		gameObject.transform.parent = Core.Logic.CurrentLevelConfig.transform;
		Enemy componentInChildren = gameObject.GetComponentInChildren<Enemy>();
		if ((bool)componentInChildren)
		{
			SpawnedEnemy = componentInChildren;
			SpawnedEnemy.SpawningId = SpawningId;
			EnemyBehaviour componentInChildren2 = SpawnedEnemy.GetComponentInChildren<EnemyBehaviour>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.EnableBehaviourOnLoad = EnableBehaviourOnLoad;
			}
			if (this.OnEnemySpawned != null)
			{
				this.OnEnemySpawned(this, SpawnedEnemy);
			}
			SetEntityName(SpawnedEnemy);
			RegisterEnemyEvents();
			if (!SpawnEnabledEnemy)
			{
				SpawnedEnemy.gameObject.SetActive(value: false);
			}
		}
		else
		{
			Log.Warning("Level", "Triying to create an enemy whitout entity component.");
		}
		Core.Logic.CurrentLevelConfig.EnemyStatsImporter?.SetEnemyStats(SpawnedEnemy);
		HasEnemySpawned = true;
	}

	private void RegisterEnemyEvents()
	{
		isEnemyEventsSuscribed = true;
		SpawnedEnemy.OnDeath += OnEntityDie;
		SpawnedEnemy.OnDestroyEntity += OnDestroyEntity;
	}

	private void UnregisterEnemyEvents()
	{
		if (isEnemyEventsSuscribed && (bool)SpawnedEnemy)
		{
			isEnemyEventsSuscribed = false;
			SpawnedEnemy.OnDeath -= OnEntityDie;
			SpawnedEnemy.OnDestroyEntity -= OnDestroyEntity;
		}
	}

	private bool ValidateInput(string entityName)
	{
		return (float)entityName.Length > 3f;
	}

	private void SetEntityName(Entity instance)
	{
		if (!(instance == null) && ValidateInput(EntityName))
		{
			int hashCode = instance.gameObject.GetHashCode();
			instance.gameObject.name = EntityName + hashCode;
		}
	}

	private void OnDestroyEntity()
	{
		UnregisterEnemyEvents();
	}

	private void OnEntityDie()
	{
		UnregisterEnemyEvents();
		HasEnemySpawned = false;
		Core.Logic.EnemySpawner.AddConsumedSpawner(this);
	}

	private void OnValidate()
	{
		if (selectedEnemy == null && preview != null)
		{
			preview.sprite = null;
		}
	}

	public void SpawnEnemyOnArena()
	{
		if ((bool)SpawnVfx)
		{
			PoolManager.Instance.ReuseObject(SpawnVfx, Position + (Vector3)SpawnEffectOffsetPosition, Quaternion.identity);
		}
		StartCoroutine(SetEnemyActive());
	}

	private IEnumerator SetEnemyActive(bool isActive = true)
	{
		yield return _spawnOnArenaAwaiting;
		if ((bool)SpawnedEnemy && (bool)SpawnedEnemy.gameObject)
		{
			SpawnedEnemy.gameObject.SetActive(isActive);
		}
	}

	private void SpawnOnArenaValueChanged()
	{
		if (SpawnOnArena)
		{
			SpawnEnabledEnemy = false;
		}
	}

	private void SpawnEnabledEnemyValueChanged()
	{
		if (SpawnEnabledEnemy)
		{
			SpawnOnArena = false;
		}
	}

	private void OnDestroy()
	{
		UnregisterEnemyEvents();
	}

	private void OnDrawGizmos()
	{
		if (EnableInfluenceArea)
		{
			DrawCircle(InfluenceAreaRadius, Color.magenta);
		}
	}

	private void DrawPreview()
	{
		SpriteRenderer componentInChildren = selectedEnemy.GetComponentInChildren<SpriteRenderer>();
		preview.sprite = componentInChildren.sprite;
		preview.transform.localPosition = componentInChildren.transform.localPosition;
		EnemySpawnConfigurator component = GetComponent<EnemySpawnConfigurator>();
		if ((bool)component)
		{
			preview.flipX = component.facingLeft;
		}
	}

	private void DrawCircle(float radius, Color color)
	{
		Gizmos.color = color;
		float f = 0f;
		float x = radius * Mathf.Cos(f);
		float y = radius * Mathf.Sin(f);
		Vector3 vector = base.transform.position + new Vector3(x, y);
		Vector3 to = vector;
		for (f = 0.1f; f < (float)Math.PI * 2f; f += 0.1f)
		{
			x = radius * Mathf.Cos(f);
			y = radius * Mathf.Sin(f);
			Vector3 vector2 = base.transform.position + new Vector3(x, y);
			Gizmos.DrawLine(vector, vector2);
			vector = vector2;
		}
		Gizmos.DrawLine(vector, to);
	}
}
