using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public static class FmodResonanceAudio
{
	private struct RoomProperties
	{
		public float positionX;

		public float positionY;

		public float positionZ;

		public float rotationX;

		public float rotationY;

		public float rotationZ;

		public float rotationW;

		public float dimensionsX;

		public float dimensionsY;

		public float dimensionsZ;

		public FmodResonanceAudioRoom.SurfaceMaterial materialLeft;

		public FmodResonanceAudioRoom.SurfaceMaterial materialRight;

		public FmodResonanceAudioRoom.SurfaceMaterial materialBottom;

		public FmodResonanceAudioRoom.SurfaceMaterial materialTop;

		public FmodResonanceAudioRoom.SurfaceMaterial materialFront;

		public FmodResonanceAudioRoom.SurfaceMaterial materialBack;

		public float reflectionScalar;

		public float reverbGain;

		public float reverbTime;

		public float reverbBrightness;
	}

	public const float maxGainDb = 24f;

	public const float minGainDb = -24f;

	public const float maxReverbBrightness = 1f;

	public const float minReverbBrightness = -1f;

	public const float maxReverbTime = 3f;

	public const float maxReflectivity = 2f;

	private static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));

	private static readonly string listenerPluginName = "Resonance Audio Listener";

	private static readonly int roomPropertiesSize = Marshal.SizeOf(typeof(RoomProperties));

	private static readonly int roomPropertiesIndex = 1;

	private static Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

	private static List<FmodResonanceAudioRoom> enabledRooms = new List<FmodResonanceAudioRoom>();

	private static VECTOR listenerPositionFmod = default(VECTOR);

	private static DSP listenerPlugin;

	private static DSP ListenerPlugin
	{
		get
		{
			if (!listenerPlugin.hasHandle())
			{
				listenerPlugin = Initialize();
			}
			return listenerPlugin;
		}
	}

	public static void UpdateAudioRoom(FmodResonanceAudioRoom room, bool roomEnabled)
	{
		if (roomEnabled)
		{
			if (!enabledRooms.Contains(room))
			{
				enabledRooms.Add(room);
			}
		}
		else
		{
			enabledRooms.Remove(room);
		}
		if (enabledRooms.Count > 0)
		{
			FmodResonanceAudioRoom room2 = enabledRooms[enabledRooms.Count - 1];
			RoomProperties roomProperties = GetRoomProperties(room2);
			IntPtr intPtr = Marshal.AllocHGlobal(roomPropertiesSize);
			Marshal.StructureToPtr(roomProperties, intPtr, fDeleteOld: false);
			ListenerPlugin.setParameterData(roomPropertiesIndex, GetBytes(intPtr, roomPropertiesSize));
			Marshal.FreeHGlobal(intPtr);
		}
		else
		{
			ListenerPlugin.setParameterData(roomPropertiesIndex, GetBytes(IntPtr.Zero, 0));
		}
	}

	public static bool IsListenerInsideRoom(FmodResonanceAudioRoom room)
	{
		RuntimeManager.LowlevelSystem.get3DListenerAttributes(0, out listenerPositionFmod, out var vel, out vel, out vel);
		Vector3 vector = new Vector3(listenerPositionFmod.x, listenerPositionFmod.y, listenerPositionFmod.z);
		Vector3 vector2 = vector - room.transform.position;
		Quaternion quaternion = Quaternion.Inverse(room.transform.rotation);
		bounds.size = Vector3.Scale(room.transform.lossyScale, room.size);
		return bounds.Contains(quaternion * vector2);
	}

	private static float ConvertAmplitudeFromDb(float db)
	{
		return Mathf.Pow(10f, 0.05f * db);
	}

	private static void ConvertAudioTransformFromUnity(ref Vector3 position, ref Quaternion rotation)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		matrix4x = flipZ * matrix4x * flipZ;
		position = matrix4x.GetColumn(3);
		rotation = Quaternion.LookRotation(matrix4x.GetColumn(2), matrix4x.GetColumn(1));
	}

	private static byte[] GetBytes(IntPtr ptr, int length)
	{
		if (ptr != IntPtr.Zero)
		{
			byte[] array = new byte[length];
			Marshal.Copy(ptr, array, 0, length);
			return array;
		}
		return new byte[1];
	}

	private static RoomProperties GetRoomProperties(FmodResonanceAudioRoom room)
	{
		Vector3 position = room.transform.position;
		Quaternion rotation = room.transform.rotation;
		Vector3 vector = Vector3.Scale(room.transform.lossyScale, room.size);
		ConvertAudioTransformFromUnity(ref position, ref rotation);
		RoomProperties result = default(RoomProperties);
		result.positionX = position.x;
		result.positionY = position.y;
		result.positionZ = position.z;
		result.rotationX = rotation.x;
		result.rotationY = rotation.y;
		result.rotationZ = rotation.z;
		result.rotationW = rotation.w;
		result.dimensionsX = vector.x;
		result.dimensionsY = vector.y;
		result.dimensionsZ = vector.z;
		result.materialLeft = room.leftWall;
		result.materialRight = room.rightWall;
		result.materialBottom = room.floor;
		result.materialTop = room.ceiling;
		result.materialFront = room.frontWall;
		result.materialBack = room.backWall;
		result.reverbGain = ConvertAmplitudeFromDb(room.reverbGainDb);
		result.reverbTime = room.reverbTime;
		result.reverbBrightness = room.reverbBrightness;
		result.reflectionScalar = room.reflectivity;
		return result;
	}

	private static DSP Initialize()
	{
		int count = 0;
		DSP dsp = default(DSP);
		Bank[] array = null;
		RuntimeManager.StudioSystem.getBankCount(out count);
		RuntimeManager.StudioSystem.getBankList(out array);
		for (int i = 0; i < count; i++)
		{
			int count2 = 0;
			Bus[] array2 = null;
			array[i].getBusCount(out count2);
			array[i].getBusList(out array2);
			RuntimeManager.StudioSystem.flushCommands();
			for (int j = 0; j < count2; j++)
			{
				string path = null;
				array2[j].getPath(out path);
				RuntimeManager.StudioSystem.getBus(path, out array2[j]);
				RuntimeManager.StudioSystem.flushCommands();
				array2[j].getChannelGroup(out var group);
				RuntimeManager.StudioSystem.flushCommands();
				if (!group.hasHandle())
				{
					continue;
				}
				int numdsps = 0;
				group.getNumDSPs(out numdsps);
				for (int k = 0; k < numdsps; k++)
				{
					group.getDSP(k, out dsp);
					int channels = 0;
					uint version = 0u;
					dsp.getInfo(out var name, out version, out channels, out channels, out channels);
					if (name.ToString().Equals(listenerPluginName) && dsp.hasHandle())
					{
						return dsp;
					}
				}
			}
		}
		UnityEngine.Debug.LogError(listenerPluginName + " not found in the FMOD project.");
		return dsp;
	}
}
