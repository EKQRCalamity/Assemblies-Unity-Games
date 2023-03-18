using System;
using System.Runtime.InteropServices;
using AOT;
using FMOD;
using FMOD.Studio;
using Gameplay.GameControllers.Bosses.BossFight;
using UnityEngine;

public class BossAudioSyncHelper : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential)]
	private class TimelineInfo
	{
		public int currentMusicBar;

		public StringWrapper lastMarker = default(StringWrapper);

		public bool updatedMarker;
	}

	private TimelineInfo timelineInfo;

	private GCHandle timelineHandle;

	private EVENT_CALLBACK beatCallback;

	private EventInstance musicInstance;

	[HideInInspector]
	public BossFightAudio bossfightAudio;

	public int LastBar;

	private string lastMarker = string.Empty;

	private int lastMarketBar;

	public event Action OnBar;

	public event Action<string> OnMarker;

	private void Awake()
	{
		bossfightAudio = GetComponent<BossFightAudio>();
		timelineInfo = new TimelineInfo();
		if ((bool)bossfightAudio)
		{
			bossfightAudio.OnBossMusicStarts += BossfightAudio_OnBossMusicStarts;
		}
	}

	private void BossfightAudio_OnBossMusicStarts()
	{
		bossfightAudio.OnBossMusicStarts -= BossfightAudio_OnBossMusicStarts;
		InitCallback();
	}

	private void InitCallback()
	{
		beatCallback = BeatEventCallback;
		musicInstance = bossfightAudio.GetCurrentMusicInstance();
		timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
		musicInstance.setUserData(GCHandle.ToIntPtr(timelineHandle));
		musicInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER | EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
	}

	private void OnDestroy()
	{
		musicInstance.setUserData(IntPtr.Zero);
		musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		musicInstance.release();
		timelineHandle.Free();
	}

	private void OnGUI()
	{
	}

	private void Update()
	{
		if (LastBar != timelineInfo.currentMusicBar)
		{
			LastBar = timelineInfo.currentMusicBar;
			if (this.OnBar != null)
			{
				this.OnBar();
			}
		}
		if (timelineInfo.updatedMarker)
		{
			lastMarker = timelineInfo.lastMarker;
			lastMarketBar = LastBar;
			if (this.OnMarker != null)
			{
				this.OnMarker(lastMarker);
			}
			timelineInfo.updatedMarker = false;
		}
	}

	[MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
	private static RESULT BeatEventCallback(EVENT_CALLBACK_TYPE type, EventInstance instance, IntPtr parameterPtr)
	{
		IntPtr userdata;
		RESULT userData = instance.getUserData(out userdata);
		if (userData != 0)
		{
			UnityEngine.Debug.LogError("Timeline Callback error: " + userData);
		}
		else if (userdata != IntPtr.Zero)
		{
			TimelineInfo timelineInfo = (TimelineInfo)GCHandle.FromIntPtr(userdata).Target;
			switch (type)
			{
			case EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
				timelineInfo.currentMusicBar = ((TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_BEAT_PROPERTIES))).bar;
				break;
			case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
				timelineInfo.lastMarker = ((TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES))).name;
				timelineInfo.updatedMarker = true;
				break;
			}
		}
		return RESULT.OK;
	}
}
