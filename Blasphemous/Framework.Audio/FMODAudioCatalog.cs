using System;
using System.Collections.Generic;
using FMODUnity;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Audio;

public class FMODAudioCatalog : MonoBehaviour
{
	[Serializable]
	public class FMODIndexedClip
	{
		[EventRef]
		public string FMODKey;

		public string Id;

		public override string ToString()
		{
			return Id;
		}
	}

	[Serializable]
	public class FMODIndexedClipList
	{
		public FMODIndexedClip[] Clips;

		public string Id;

		public bool InmediateCC;

		public override string ToString()
		{
			return Id;
		}
	}

	[ShowIf("HasAssociatedCatalog", true)]
	public FMODAudioCatalog AssociatedCatalog;

	public bool HasAssociatedCatalog;

	public Entity Owner;

	public FMODIndexedClip[] SoundEffects;

	private Dictionary<string, FMODIndexedClip> _sfxDictionary;

	public void Initialize()
	{
		IndexSfx();
	}

	private void IndexSfx()
	{
		_sfxDictionary = new Dictionary<string, FMODIndexedClip>();
		for (int i = 0; i < SoundEffects.Length; i++)
		{
			_sfxDictionary.Add(SoundEffects[i].Id, SoundEffects[i]);
		}
	}

	public FMODIndexedClip GetSfx(string id)
	{
		_sfxDictionary.TryGetValue(id, out var value);
		return value;
	}
}
