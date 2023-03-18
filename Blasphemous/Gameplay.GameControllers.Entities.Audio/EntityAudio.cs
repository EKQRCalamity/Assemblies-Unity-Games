using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Audio;

public class EntityAudio : MonoBehaviour
{
	public enum FxSoundCategory
	{
		Motion,
		Attack,
		Damage,
		Climb
	}

	[Tooltip("This was added on DLC3 to fix issue #83018. Messing with the audio logic at this point is dangerous so we prefer to use the new behaviour just for the specific case we know instead of risking on breaking the audio system elsewhere - DEG")]
	public bool reuseEventInstances;

	protected bool InitilizationError;

	protected Entity Owner;

	private bool mute;

	protected float DeathValue;

	protected float WallWoodValue;

	protected float WallStoneValue;

	protected float DirtValue;

	protected float SnowValue;

	protected float WaterValue;

	protected float StoneValue;

	protected float FleshValue;

	protected float WoodValue;

	protected float MarbleValue;

	protected float MetalValue;

	protected float MudValue;

	protected float SecretValue;

	protected float GrassValue;

	protected float DemakeValue;

	protected float PalioValue;

	protected float SnakeValue;

	public const string LabelParamDirt = "Dirt";

	public const string LabelParamSnow = "Snow";

	public const string LabelParamStone = "Stone";

	public const string LabelParamWood = "Wood";

	public const string LabelParamMarble = "Marble";

	public const string LabelParamMetal = "Metal";

	public const string LabelParamMud = "Mud";

	public const string LabelParamWater = "Water";

	public const string LabelParamSecret = "SecretFloor";

	public const string LabelParamGrass = "Grass";

	public const string LabelParamDemake = "Demake";

	public const string LabelParamPalio = "Palio";

	public const string LabelParamSnake = "Snake";

	public const string LabelParamFlesh = "Flesh";

	public const string LabelDeath = "Death";

	public const string LabelPanning = "Panning";

	[SerializeField]
	protected GameObject FloorCollider;

	protected ICollisionEmitter FloorSensorEmitter;

	[SerializeField]
	protected GameObject WeaponCollider;

	protected ICollisionEmitter WeaponSensorEmitter;

	public LayerMask FloorLayerMask;

	public LayerMask WeaponLayerMask;

	protected List<EventInstance> EventInstances;

	public bool Mute
	{
		get
		{
			return mute;
		}
		set
		{
			mute = value;
			if ((bool)Owner)
			{
				Owner.Mute = value;
			}
		}
	}

	public FMODAudioManager AudioManager { get; protected set; }

	protected virtual void OnWake()
	{
	}

	private void Awake()
	{
		Owner = GetComponentInParent<Entity>();
		OnWake();
	}

	protected virtual void OnStart()
	{
	}

	private void Start()
	{
		AudioManager = Core.Audio;
		OnStart();
	}

	protected virtual void OnUpdate()
	{
	}

	private void Update()
	{
		OnUpdate();
	}

	protected void SetDeathHitParam(EventInstance eventInstance)
	{
		try
		{
			eventInstance.getParameter("Death", out var instance);
			if (instance.isValid())
			{
				instance.setValue(DeathValue);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message + ex.StackTrace);
			throw;
		}
	}

	private void SetWeaponHitMaterial(EventInstance eventInstance)
	{
		try
		{
			eventInstance.getParameter("Dirt", out var instance);
			if (instance.isValid())
			{
				instance.setValue(DirtValue);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}

	public void SetWallMaterialParams(EventInstance eventInstance)
	{
		try
		{
			eventInstance.getParameter("Wood", out var instance);
			instance.setValue(WallWoodValue);
			eventInstance.getParameter("Stone", out var instance2);
			instance2.setValue(WallStoneValue);
		}
		catch
		{
		}
	}

	private void SetFloorMaterialParams(EventInstance eventInstance)
	{
		try
		{
			eventInstance.getParameter("Dirt", out var instance);
			instance.setValue(DirtValue);
			eventInstance.getParameter("Water", out var instance2);
			instance2.setValue(WaterValue);
			eventInstance.getParameter("Snow", out var instance3);
			instance3.setValue(SnowValue);
			eventInstance.getParameter("Stone", out var instance4);
			instance4.setValue(StoneValue);
			eventInstance.getParameter("Wood", out var instance5);
			instance5.setValue(WoodValue);
			eventInstance.getParameter("Marble", out var instance6);
			instance6.setValue(MarbleValue);
			eventInstance.getParameter("Flesh", out var instance7);
			instance7.setValue(FleshValue);
			eventInstance.getParameter("Metal", out var instance8);
			instance8.setValue(MetalValue);
			eventInstance.getParameter("Mud", out var instance9);
			instance9.setValue(MudValue);
			eventInstance.getParameter("SecretFloor", out var instance10);
			instance10.setValue(SecretValue);
			eventInstance.getParameter("Grass", out var instance11);
			instance11.setValue(GrassValue);
			eventInstance.getParameter("Demake", out var instance12);
			instance12.setValue(DemakeValue);
			eventInstance.getParameter("Palio", out var instance13);
			instance13.setValue(PalioValue);
			eventInstance.getParameter("Snake", out var instance14);
			instance14.setValue(SnakeValue);
		}
		catch
		{
		}
	}

	public void PlayOneShotEvent(string eventKey, FxSoundCategory fxSoundCategory)
	{
		if (AudioManager == null || mute)
		{
			return;
		}
		EventInstance eventInstance = AudioManager.CreateCatalogEvent(eventKey);
		if (!eventInstance.isValid())
		{
			Debug.LogError($"ERROR: Couldn't find catalog sound event called <{eventKey}>");
			return;
		}
		eventInstance.setCallback(SetPanning(eventInstance), EVENT_CALLBACK_TYPE.CREATED);
		switch (fxSoundCategory)
		{
		case FxSoundCategory.Attack:
			SetWeaponHitMaterial(eventInstance);
			break;
		case FxSoundCategory.Motion:
			SetFloorMaterialParams(eventInstance);
			break;
		case FxSoundCategory.Damage:
			SetDeathHitParam(eventInstance);
			break;
		case FxSoundCategory.Climb:
			SetWallMaterialParams(eventInstance);
			break;
		}
		eventInstance.start();
		eventInstance.release();
	}

	protected void ReleaseAudioEvents()
	{
		if (EventInstances == null)
		{
			return;
		}
		for (int i = 0; i < EventInstances.Count; i++)
		{
			if (EventInstances[i].isValid())
			{
				EventInstances[i].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				EventInstances[i].release();
			}
		}
		EventInstances.Clear();
	}

	public void PlayEvent(ref EventInstance eventInstance, string eventKey, bool checkSpriteRendererVisible = true)
	{
		if ((checkSpriteRendererVisible && !Owner.SpriteRenderer.isVisible) || mute)
		{
			return;
		}
		if (reuseEventInstances)
		{
			PlayEventReuse(ref eventInstance, eventKey);
		}
		else if (!eventInstance.isValid())
		{
			eventInstance = AudioManager.CreateCatalogEvent(eventKey);
			if (eventInstance.isValid())
			{
				eventInstance.start();
			}
		}
	}

	private void PlayEventReuse(ref EventInstance eventInstance, string eventKey)
	{
		if (!eventInstance.isValid())
		{
			eventInstance = AudioManager.CreateCatalogEvent(eventKey);
		}
		if (eventInstance.isValid())
		{
			eventInstance.start();
		}
	}

	public void UpdateEvent(ref EventInstance eventInstance)
	{
		if (Owner.SpriteRenderer.isVisible && !mute && eventInstance.isValid())
		{
			SetPanning(eventInstance);
		}
	}

	public void StopEvent(ref EventInstance eventInstance)
	{
		if (eventInstance.isValid() && !mute)
		{
			eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			eventInstance.release();
			eventInstance = default(EventInstance);
		}
	}

	protected EVENT_CALLBACK SetPanning(EventInstance e)
	{
		if (!e.isValid())
		{
			return null;
		}
		e.getParameter("Panning", out var instance);
		if (instance.isValid() && (bool)Owner)
		{
			float panningValueByPosition = FMODAudioManager.GetPanningValueByPosition(Owner.transform.position);
			instance.setValue(panningValueByPosition);
		}
		return null;
	}

	public static EVENT_CALLBACK SetPanning(EventInstance e, Vector3 pos)
	{
		e.getParameter("Panning", out var instance);
		if (instance.isValid())
		{
			float panningValueByPosition = FMODAudioManager.GetPanningValueByPosition(pos);
			instance.setValue(panningValueByPosition);
		}
		return null;
	}
}
