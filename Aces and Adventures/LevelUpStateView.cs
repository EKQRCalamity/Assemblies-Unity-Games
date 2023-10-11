using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class LevelUpStateView : MonoBehaviour
{
	public LevelUpDeckLayout main;

	[SerializeField]
	protected PointerClick3D _backColliderPointerClick;

	[SerializeField]
	protected PointerClick3D _vialStandPointerClick;

	[SerializeField]
	protected PointerClick3D _potHolderPointerClick;

	[SerializeField]
	protected PointerOver3D _standDisplayPointerOver;

	public BoolEvent onStateEnabledChange;

	public BoolEvent onLevelDisplayOpenChange;

	public IntEvent onLevelAmountChange;

	public BoolEvent onBackColliderEnableChange;

	[Range(1f, 100f)]
	public float focalDistanceEaseSpeed = 2f;

	public Action onBackColliderClick;

	public Action onVialStandClick;

	private bool _levelDisplayOpen;

	private int _levelAmount;

	private bool _backColliderEnabled;

	private DepthOfField _dof;

	private float _initialFocalDistance = 1f;

	public bool levelDisplayOpen
	{
		get
		{
			return _levelDisplayOpen;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _levelDisplayOpen, value))
			{
				onLevelDisplayOpenChange?.Invoke(value);
			}
		}
	}

	public int levelAmount
	{
		get
		{
			return _levelAmount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _levelAmount, value))
			{
				onLevelAmountChange?.Invoke(value);
			}
		}
	}

	public bool backColliderEnabled
	{
		get
		{
			return _backColliderEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _backColliderEnabled, value))
			{
				onBackColliderEnableChange?.Invoke(value);
			}
		}
	}

	public float initialFocalDistance => _initialFocalDistance;

	public float targetFocalDistance { get; set; }

	public float closeupFocalDistance => 0.5f;

	public GameObject bubblePotHolderPointerClickTo
	{
		set
		{
			BubblePointerClick3D component = _potHolderPointerClick.GetComponent<BubblePointerClick3D>();
			if ((object)component != null)
			{
				component.otherGameObject = value;
			}
		}
	}

	private void _OnStandDisplayPointerOver(PointerEventData eventData)
	{
		ProjectedTooltipFitter.Create(LevelUpMessages.AvailableLevelUps.Localize().Localize(), _standDisplayPointerOver.gameObject, GameStateView.Instance?.tooltipCanvas, TooltipAlignment.MiddleRight, 24);
	}

	private void _OnStandDisplayPointerExit(PointerEventData eventData)
	{
		ProjectedTooltipFitter.Finish(_standDisplayPointerOver.gameObject);
	}

	private void _OnLevelDisplayOpenChange(bool open)
	{
		_standDisplayPointerOver.GetComponent<Collider>().enabled = open;
	}

	private void _OnExperienceChange()
	{
		(from c in main.vial.GetCards().AsEnumerable()
			select c.card).OfType<ExperienceVial>().FirstOrDefault()?.SetExperience(ProfileManager.progress.experience.read.currentVialXP);
		levelAmount = ProfileManager.progress.experience.read.pendingLevelUps;
	}

	private void Awake()
	{
		_backColliderPointerClick?.OnClick.AddListener(delegate
		{
			onBackColliderClick?.Invoke();
		});
		_vialStandPointerClick?.OnClick.AddListener(delegate
		{
			onVialStandClick?.Invoke();
		});
		_standDisplayPointerOver?.OnEnter.AddListener(_OnStandDisplayPointerOver);
		_standDisplayPointerOver?.OnExit.AddListener(_OnStandDisplayPointerExit);
		onLevelDisplayOpenChange?.AddListener(_OnLevelDisplayOpenChange);
		Experience.OnExperienceChange += _OnExperienceChange;
	}

	private void Start()
	{
		Volume volume = GameManager.Instance?.levelUpCamera.GetComponent<Volume>();
		if ((object)volume != null)
		{
			VolumeProfile volumeProfile2 = (volume.profile = GameUtil.InstantiateIfNotAnInstance(volume.profile));
			volumeProfile2.TryGet<DepthOfField>(out _dof);
		}
		if ((bool)_dof)
		{
			targetFocalDistance = (_initialFocalDistance = _dof.focusDistance.value);
		}
	}

	private void Update()
	{
		if ((bool)_dof)
		{
			float value = _dof.focusDistance.value;
			MathUtil.EaseSnap(ref value, targetFocalDistance, focalDistanceEaseSpeed, Time.deltaTime, 0.001f);
			_dof.focusDistance.value = value;
		}
	}

	private void OnDestroy()
	{
		Experience.OnExperienceChange -= _OnExperienceChange;
	}

	public void SetState(LevelUpState state)
	{
		main.deck = state.main;
		main.GetLayout(LevelUpPile.Seals).DisableInput();
		onStateEnabledChange?.Invoke(state.enabled);
		bubblePotHolderPointerClickTo = state.main.NextInPile(LevelUpPile.Pot)?.view.gameObject;
	}

	public void ResetFocalDistance()
	{
		targetFocalDistance = initialFocalDistance;
	}

	public void SetCloseupFocalDistance()
	{
		targetFocalDistance = closeupFocalDistance;
	}
}
