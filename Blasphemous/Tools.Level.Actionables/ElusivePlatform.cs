using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Audio;
using Tools.Level.Layout;
using UnityEngine;

namespace Tools.Level.Actionables;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(LayoutElement))]
public class ElusivePlatform : MonoBehaviour, IActionable, INoSafePosition
{
	private readonly int _stepOverAnim = Animator.StringToHash("StepOver");

	private readonly int _collapseAnim = Animator.StringToHash("Collapse");

	private readonly int _appearAnim = Animator.StringToHash("Appear");

	private string _materialFxPrefix;

	public LayerMask TargetLayer;

	public LayerMask GrabTriggerLayer;

	public float RemainTime = 2f;

	public float RecoverTime = 2f;

	private bool _targetIsOnPlatform;

	private bool _platformIsActive;

	private bool _isTransitioning;

	private bool _disappear;

	private Collider2D _collider;

	private Collider2D[] _grabColliders;

	private Animator _animator;

	private SpriteRenderer _spriteRenderer;

	private LayoutElement _layoutElement;

	private Entity _entityOnTop;

	private EventInstance _soundEventInstance;

	public bool Locked { get; set; }

	private void Awake()
	{
		_collider = GetComponent<Collider2D>();
		_layoutElement = GetComponent<LayoutElement>();
		_animator = GetComponentInChildren<Animator>();
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	private void Start()
	{
		_grabColliders = GetGlimbLedes();
		SetFxAudio();
	}

	private void Update()
	{
		if (_targetIsOnPlatform)
		{
			if (!_isTransitioning)
			{
				_isTransitioning = true;
				StartCoroutine((!(_animator != null)) ? DisappearLayout() : DisappearDeco());
			}
		}
		else if (_disappear)
		{
			_disappear = !_disappear;
			_isTransitioning = true;
			StartCoroutine((!(_animator != null)) ? AppearLayout() : AppearDeco());
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0)
		{
			_entityOnTop = other.GetComponentInParent<Entity>();
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0 && other.bounds.min.y + 0.1f >= _collider.bounds.max.y && _entityOnTop.Status.IsGrounded && !_entityOnTop.Status.Dead && !_targetIsOnPlatform)
		{
			_targetIsOnPlatform = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((TargetLayer.value & (1 << other.gameObject.layer)) > 0 && _targetIsOnPlatform)
		{
			_targetIsOnPlatform = !_targetIsOnPlatform;
		}
	}

	private IEnumerator DisappearLayout()
	{
		float remainTime = RemainTime;
		Color c = _layoutElement.SpriteRenderer.material.color;
		Color newColor = new Color(c.r, c.g, c.b);
		while (remainTime >= 0f)
		{
			remainTime -= Time.deltaTime;
			float percentageRemain = Mathf.Clamp01(remainTime / RemainTime);
			newColor.a = percentageRemain;
			_layoutElement.SpriteRenderer.material.color = newColor;
			yield return new WaitForEndOfFrame();
		}
		_collider.enabled = false;
		_targetIsOnPlatform = false;
		EnableGrabColliders(enable: false);
		_isTransitioning = false;
		_disappear = true;
	}

	private IEnumerator AppearLayout()
	{
		yield return new WaitForSeconds(RecoverTime);
		Color c = _layoutElement.SpriteRenderer.material.color;
		Color newColor = new Color(c.r, c.g, c.b)
		{
			a = 1f
		};
		_layoutElement.SpriteRenderer.material.color = newColor;
		_collider.enabled = true;
		EnableGrabColliders();
		_isTransitioning = false;
	}

	private IEnumerator DisappearDeco()
	{
		float remainTime = RemainTime;
		_animator.Play(_stepOverAnim);
		PlayFxAudio(_materialFxPrefix + "STEP");
		while (remainTime >= 0f)
		{
			remainTime -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		_animator.Play(_collapseAnim);
		_collider.enabled = false;
		_targetIsOnPlatform = false;
		EnableGrabColliders(enable: false);
		_isTransitioning = false;
		_disappear = true;
		if (_spriteRenderer.isVisible)
		{
			PlayFxAudio(_materialFxPrefix + "COLLAPSE");
		}
	}

	private IEnumerator AppearDeco()
	{
		yield return new WaitForSeconds(RecoverTime);
		SpriteFadeIn();
		_animator.Play(_appearAnim);
		_collider.enabled = true;
		EnableGrabColliders();
		_isTransitioning = false;
		if (_spriteRenderer.isVisible)
		{
			PlayFxAudio(_materialFxPrefix + "RESPAWN");
		}
	}

	public void SpriteFadeOut()
	{
		if (!(_spriteRenderer == null))
		{
			_spriteRenderer.DOFade(0f, 0.1f);
		}
	}

	public void SpriteFadeIn()
	{
		if (!(_spriteRenderer == null))
		{
			_spriteRenderer.DOFade(1f, 0.1f);
		}
	}

	public void Use()
	{
		StartCoroutine(DisappearLayout());
	}

	private Collider2D[] GetGlimbLedes()
	{
		return Physics2D.OverlapAreaAll(_collider.bounds.min, _collider.bounds.max, GrabTriggerLayer);
	}

	private void EnableGrabColliders(bool enable = true)
	{
		Collider2D[] grabColliders = _grabColliders;
		foreach (Collider2D collider2D in grabColliders)
		{
			collider2D.enabled = enable;
		}
	}

	private void SetFxAudio()
	{
		if (base.tag.Equals("Material:Stone"))
		{
			_materialFxPrefix = "BP_STONE_";
		}
		else if (base.tag.Equals("Material:Wood"))
		{
			_materialFxPrefix = "BP_WOOD_";
		}
		else if (base.tag.Equals("Material:Glass"))
		{
			_materialFxPrefix = "BP_GLASS_";
		}
		else if (base.tag.Equals("Material:Demake"))
		{
			_materialFxPrefix = "BP_DEMAKE_";
		}
	}

	private void PlayFxAudio(string idEvent)
	{
		_soundEventInstance = Core.Audio.CreateCatalogEvent(idEvent);
		_soundEventInstance.setCallback(EntityAudio.SetPanning(_soundEventInstance, base.transform.position), EVENT_CALLBACK_TYPE.CREATED);
		_soundEventInstance.start();
		_soundEventInstance.release();
	}
}
