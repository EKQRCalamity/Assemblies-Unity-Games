using System;
using System.Linq;
using Framework.Managers;
using Tools.DataContainer;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.InputSystem;

public class EntityRumble : MonoBehaviour
{
	[Serializable]
	public struct RumblePreset
	{
		public string Id;

		public RumbleData RumbleAsset;

		public RumblePreset(string id, RumbleData rumbleAsset)
		{
			Id = id;
			RumbleAsset = rumbleAsset;
		}
	}

	[SerializeField]
	public RumblePreset[] RumblePresets;

	public void UsePreset(string presetId)
	{
		try
		{
			RumblePreset rumblePreset = RumblePresets.Single((RumblePreset x) => x.Id.Equals(presetId));
			Core.Input.ApplyRumble(rumblePreset.RumbleAsset);
		}
		catch (ArgumentNullException value)
		{
			Console.WriteLine(value);
		}
		catch (InvalidOperationException value2)
		{
			Console.WriteLine(value2);
		}
	}
}
