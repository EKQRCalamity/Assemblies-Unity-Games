using System;
using System.Linq;
using UnityEngine;

public class ProjectileMediaViewTester : MonoBehaviour
{
	public CardTargetTransforms activator;

	public CardTargetTransforms target;

	public bool loop = true;

	[Range(0f, 5f)]
	public float loopDelay = 1f;

	public bool restartOnDataChange = true;

	private ProjectileMediaData _projectileMediaData;

	private ProjectileMediaView _projectileMediaView;

	private System.Random _random = new System.Random();

	private bool _finishedLooping;

	private float _loopDelayElapsed;

	private UIGeneratorType _ui;

	private readonly Func<ProjectileFlightSFX, bool> projectileIsFading = (ProjectileFlightSFX p) => p.isFading;

	public static ProjectileMediaViewTester Instance { get; private set; }

	private void _UpdateUI()
	{
		UIGeneratorType activeUIForType = UIGeneratorType.GetActiveUIForType(typeof(ProjectileMediaData));
		if (!(_ui == activeUIForType) && (bool)activeUIForType)
		{
			_ui = activeUIForType;
			_ui.OnGenerate.AddListener(OnGenerate);
			_ui.OnValueChanged.AddListener(OnValueChanged);
			OnGenerate(_ui.currentObject);
		}
	}

	private void _OnFinish()
	{
		_projectileMediaView = null;
		if (loop)
		{
			_loopDelayElapsed = 0f;
		}
		else
		{
			_finishedLooping = true;
		}
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void Start()
	{
		UIGeneratorType.ValidateAllOfType<ProjectileMediaData>();
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		_UpdateUI();
		if (!_finishedLooping && !_projectileMediaView)
		{
			if (base.gameObject.GetComponentsInChildrenPooled<ProjectileFlightSFX>().AsEnumerable().All(projectileIsFading) && !base.gameObject.GetComponentsInChildrenPooled<ProjectileBurstSFX>().AsEnumerable().Any())
			{
				_loopDelayElapsed += Time.deltaTime;
			}
			if (_loopDelayElapsed >= loopDelay)
			{
				Play();
			}
		}
	}

	public void OnGenerate(object obj)
	{
		_projectileMediaData = obj as ProjectileMediaData;
		OnValueChanged();
	}

	public void OnValueChanged()
	{
		if (restartOnDataChange)
		{
			foreach (ProjectileMediaView item in base.gameObject.GetComponentsInChildrenPooled<ProjectileMediaView>())
			{
				item.canceled = true;
			}
			foreach (ProjectileBurstSFX item2 in base.gameObject.GetComponentsInChildrenPooled<ProjectileBurstSFX>())
			{
				item2.gameObject.SetActive(value: false);
			}
			foreach (ProjectileFlightSFX item3 in base.gameObject.GetComponentsInChildrenPooled<ProjectileFlightSFX>())
			{
				item3.gameObject.SetActive(value: false);
			}
			foreach (ProjectileBurstView item4 in base.gameObject.GetComponentsInChildrenPooled<ProjectileBurstView>())
			{
				item4.gameObject.SetActive(value: false);
			}
			foreach (ProjectileLauncherView item5 in base.gameObject.GetComponentsInChildrenPooled<ProjectileLauncherView>())
			{
				item5.gameObject.SetActive(value: false);
			}
			foreach (ProjectileMediaView item6 in base.gameObject.GetComponentsInChildrenPooled<ProjectileMediaView>())
			{
				item6.gameObject.SetActive(value: false);
			}
		}
		_finishedLooping = false;
		_loopDelayElapsed = loopDelay;
	}

	public void Play()
	{
		if (_projectileMediaData != null)
		{
			_projectileMediaView = Pools.Unpool(ProjectileMediaView.Blueprint, base.transform).GetComponent<ProjectileMediaView>();
			_projectileMediaView.onFinish = _OnFinish;
			_projectileMediaView.LaunchProjectiles(_random, _projectileMediaData, activator, target);
		}
	}

	public void UpdateLighting()
	{
	}
}
