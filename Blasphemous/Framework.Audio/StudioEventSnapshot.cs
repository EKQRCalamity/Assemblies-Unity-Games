using System;
using System.Threading;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Framework.Audio;

[AddComponentMenu("FMOD Studio/FMOD Studio Event SnapShot")]
public class StudioEventSnapshot : MonoBehaviour
{
	public bool AllowFadeout = true;

	public string CollisionTag = string.Empty;

	[EventRef]
	public string Event = string.Empty;

	private EventDescription eventDescription;

	private bool hasTriggered;

	private EventInstance instance;

	private bool isQuitting;

	public bool OverrideAttenuation;

	public float OverrideMaxDistance = -1f;

	public float OverrideMinDistance = -1f;

	public ParamRef[] Params = new ParamRef[0];

	public EmitterGameEvent PlayEvent;

	public bool Preload;

	public EmitterGameEvent StopEvent;

	public bool TriggerOnce;

	private void Start()
	{
		RuntimeUtils.EnforceLibraryOrder();
		if (Preload)
		{
			Lookup();
			eventDescription.loadSampleData();
			RuntimeManager.StudioSystem.update();
			eventDescription.getSampleLoadingState(out var state);
			while (state == LOADING_STATE.LOADING)
			{
				Thread.Sleep(1);
				eventDescription.getSampleLoadingState(out state);
			}
		}
		HandleGameEvent(EmitterGameEvent.ObjectStart);
	}

	private void OnApplicationQuit()
	{
		isQuitting = true;
	}

	private void OnDestroy()
	{
		if (!isQuitting)
		{
			HandleGameEvent(EmitterGameEvent.ObjectDestroy);
			if (instance.isValid())
			{
				RuntimeManager.DetachInstanceFromGameObject(instance);
			}
			if (Preload)
			{
				eventDescription.unloadSampleData();
			}
		}
		Stop();
	}

	private void OnEnable()
	{
		HandleGameEvent(EmitterGameEvent.ObjectEnable);
	}

	private void OnDisable()
	{
		HandleGameEvent(EmitterGameEvent.ObjectDisable);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
		{
			HandleGameEvent(EmitterGameEvent.TriggerEnter);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
		{
			HandleGameEvent(EmitterGameEvent.TriggerExit);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
		{
			HandleGameEvent(EmitterGameEvent.TriggerEnter2D);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
		{
			HandleGameEvent(EmitterGameEvent.TriggerExit2D);
		}
	}

	private void OnCollisionEnter()
	{
		HandleGameEvent(EmitterGameEvent.CollisionEnter);
	}

	private void OnCollisionExit()
	{
		HandleGameEvent(EmitterGameEvent.CollisionExit);
	}

	private void OnCollisionEnter2D()
	{
		HandleGameEvent(EmitterGameEvent.CollisionEnter2D);
	}

	private void OnCollisionExit2D()
	{
		HandleGameEvent(EmitterGameEvent.CollisionExit2D);
	}

	private void HandleGameEvent(EmitterGameEvent gameEvent)
	{
		if (PlayEvent == gameEvent)
		{
			Play();
		}
		if (StopEvent == gameEvent)
		{
			Stop();
		}
	}

	private void Lookup()
	{
		eventDescription = RuntimeManager.GetEventDescription(Event);
	}

	public void Play()
	{
		if ((TriggerOnce && hasTriggered) || string.IsNullOrEmpty(Event))
		{
			return;
		}
		if (!eventDescription.isValid())
		{
			Lookup();
		}
		bool oneshot = false;
		if (!Event.StartsWith("snapshot", StringComparison.CurrentCultureIgnoreCase))
		{
			eventDescription.isOneshot(out oneshot);
		}
		eventDescription.is3D(out var is3D);
		if (!instance.isValid())
		{
			instance = default(EventInstance);
		}
		if (oneshot && instance.isValid())
		{
			instance.release();
			instance = default(EventInstance);
		}
		if (instance.isValid())
		{
			eventDescription.createInstance(out instance);
			if (is3D)
			{
				Rigidbody component = GetComponent<Rigidbody>();
				Rigidbody2D component2 = GetComponent<Rigidbody2D>();
				Transform component3 = GetComponent<Transform>();
				if ((bool)component)
				{
					instance.set3DAttributes(RuntimeUtils.To3DAttributes(base.gameObject, component));
					RuntimeManager.AttachInstanceToGameObject(instance, component3, component);
				}
				else
				{
					instance.set3DAttributes(RuntimeUtils.To3DAttributes(base.gameObject, component2));
					RuntimeManager.AttachInstanceToGameObject(instance, component3, component2);
				}
			}
		}
		ParamRef[] @params = Params;
		foreach (ParamRef paramRef in @params)
		{
			instance.setParameterValue(paramRef.Name, paramRef.Value);
		}
		if (is3D && OverrideAttenuation)
		{
			instance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, OverrideMinDistance);
			instance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, OverrideMaxDistance);
		}
		instance.start();
		hasTriggered = true;
	}

	public void Stop()
	{
		if (instance.isValid())
		{
			instance.stop((!AllowFadeout) ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			instance.release();
			instance = default(EventInstance);
		}
	}

	public void SetParameter(string name, float value)
	{
		if (instance.isValid())
		{
			instance.setParameterValue(name, value);
		}
	}

	public bool IsPlaying()
	{
		if (instance.isValid())
		{
			instance.getPlaybackState(out var state);
			return state != PLAYBACK_STATE.STOPPED;
		}
		return false;
	}
}
