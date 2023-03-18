using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.NPCs.BloodDecals;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class BloodDecal : MonoBehaviour
{
	private Animator bloodDecalAnimator;

	private SpriteRenderer bloodDecalSprite;

	private BloodDecalPumper bloodDecalPumper;

	[SerializeField]
	private float bloodDecalFreezeTime = 0.1f;

	private float deltaBloodDecalFreezeTime;

	public bool hasPermaBlood;

	[SerializeField]
	protected PermaBlood permaBlood;

	private LevelEffectsStore levelEffectStore;

	public PermaBlood PermaBlood
	{
		get
		{
			return permaBlood;
		}
		set
		{
			permaBlood = value;
		}
	}

	private void Awake()
	{
		bloodDecalAnimator = GetComponent<Animator>();
		bloodDecalSprite = GetComponent<SpriteRenderer>();
	}

	public void OnEnable()
	{
		deltaBloodDecalFreezeTime = 0f;
		if (bloodDecalAnimator != null)
		{
			bloodDecalAnimator.speed = 0f;
		}
	}

	private void Start()
	{
		bloodDecalPumper = GetComponentInParent<BloodDecalPumper>();
		levelEffectStore = Core.Logic.CurrentLevelConfig.LevelEffectsStore;
	}

	private void Update()
	{
		deltaBloodDecalFreezeTime += Time.deltaTime;
		if (deltaBloodDecalFreezeTime >= bloodDecalFreezeTime)
		{
			bloodDecalAnimator.speed = 1f;
		}
	}

	public void Dispose()
	{
		if (bloodDecalPumper != null)
		{
			LeavePermaBlood();
			bloodDecalPumper.DisposeBloodDecal(this);
		}
	}

	public void PlayBloodDecalAnimation()
	{
		if (bloodDecalAnimator != null)
		{
			bloodDecalAnimator.Play("PumpBlood");
		}
	}

	public void LeavePermaBlood()
	{
		if (permaBlood != null)
		{
			SpawnPoint permaBloodSpawnPoint = bloodDecalPumper.PermaBloodSpawnPoint;
			GameObject gameObject = permaBlood.Instance(permaBloodSpawnPoint.transform.position, base.transform.rotation);
			gameObject.transform.parent = levelEffectStore.transform;
		}
	}

	public void SetOrientation(EntityOrientation orientation)
	{
		bloodDecalSprite.flipX = orientation == EntityOrientation.Left;
	}
}
