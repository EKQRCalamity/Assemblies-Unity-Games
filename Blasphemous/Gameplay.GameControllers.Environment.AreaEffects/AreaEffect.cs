using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.AreaEffects;

[RequireComponent(typeof(Collider2D))]
public class AreaEffect : MonoBehaviour
{
	protected Collider2D AreaCollider;

	public LayerMask AffectedEntities;

	[FoldoutGroup("Area Settings", true, 0)]
	public float EffectLapse;

	private float _currentEffectLapse;

	[FoldoutGroup("Area Settings", true, 0)]
	[SerializeField]
	public bool IsDisabled;

	protected List<GameObject> Population = new List<GameObject>();

	private bool levelReady;

	public bool IsPopulated { get; set; }

	private void Awake()
	{
		AreaCollider = GetComponent<Collider2D>();
		levelReady = false;
		LevelManager.OnLevelLoaded += LevelManager_OnLevelLoaded;
		OnAwake();
	}

	private void LevelManager_OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		levelReady = true;
	}

	private void Start()
	{
		OnStart();
	}

	private void Update()
	{
		if (levelReady)
		{
			OnUpdate();
		}
	}

	protected virtual void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= LevelManager_OnLevelLoaded;
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
		if (!IsPopulated)
		{
			return;
		}
		_currentEffectLapse += Time.deltaTime;
		if (_currentEffectLapse >= EffectLapse)
		{
			_currentEffectLapse = 0f;
			if (!IsDisabled)
			{
				OnStayAreaEffect();
			}
		}
	}

	protected virtual void OnEnterAreaEffect(Collider2D other)
	{
	}

	protected virtual void OnExitAreaEffect(Collider2D other)
	{
	}

	protected virtual void OnStayAreaEffect()
	{
	}

	public virtual void EnableEffect(bool enableEffect = true)
	{
		IsDisabled = !enableEffect;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((AffectedEntities.value & (1 << other.gameObject.layer)) > 0)
		{
			IsPopulated = true;
			AddEntityToAreaPopulation(other.transform.gameObject);
			if (!IsDisabled)
			{
				OnEnterAreaEffect(other);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((AffectedEntities.value & (1 << other.gameObject.layer)) > 0)
		{
			IsPopulated = false;
			_currentEffectLapse = 0f;
			RemoveEntityToAreaPopulation(other.transform.gameObject);
			if (!IsDisabled)
			{
				OnExitAreaEffect(other);
			}
		}
	}

	private void AddEntityToAreaPopulation(GameObject entity)
	{
		if (!Population.Contains(entity))
		{
			Population.Add(entity);
		}
	}

	protected void RemoveEntityToAreaPopulation(GameObject entity)
	{
		if (Population.Contains(entity))
		{
			Population.Remove(entity);
		}
	}

	private void OnDisable()
	{
		IsPopulated = false;
		Population.Clear();
	}
}
