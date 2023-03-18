using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Audio;

public class AudioCatalog : MonoBehaviour
{
	[Serializable]
	public class IndexedClip
	{
		public AudioClip Clip;

		public string Id;

		public override string ToString()
		{
			return Id;
		}
	}

	[Serializable]
	public class IndexedClipList
	{
		public AudioClip[] Clips;

		public string Id;

		public bool InmediateCC;

		public override string ToString()
		{
			return Id;
		}
	}

	[Serializable]
	public class PlaylistDef
	{
		public string Id;

		[NonSerialized]
		public int Index;

		public string[] Songs;

		public override string ToString()
		{
			return Id;
		}
	}

	public IndexedClipList[] SoundEffects;

	public IndexedClip[] Songs;

	public PlaylistDef[] Playlists;

	private Dictionary<string, IndexedClipList> sfxDictionary;

	private Dictionary<string, IndexedClip> songsDictionary;

	private Dictionary<string, PlaylistDef> playlistDictionary;

	public void Initialize()
	{
		indexPlaylists();
		indexSfx();
		indexSongs();
	}

	private void indexSfx()
	{
		sfxDictionary = new Dictionary<string, IndexedClipList>();
		for (int i = 0; i < SoundEffects.Length; i++)
		{
			sfxDictionary.Add(SoundEffects[i].Id, SoundEffects[i]);
		}
	}

	public IndexedClipList GetSfx(string Id)
	{
		IndexedClipList value = null;
		sfxDictionary.TryGetValue(Id, out value);
		return value;
	}

	private void indexSongs()
	{
		songsDictionary = new Dictionary<string, IndexedClip>();
		for (int i = 0; i < Songs.Length; i++)
		{
			songsDictionary.Add(Songs[i].Id, Songs[i]);
		}
	}

	public IndexedClip GetSong(string Id)
	{
		IndexedClip value = null;
		songsDictionary.TryGetValue(Id, out value);
		return value;
	}

	private void indexPlaylists()
	{
		playlistDictionary = new Dictionary<string, PlaylistDef>();
		for (int i = 0; i < Playlists.Length; i++)
		{
			playlistDictionary.Add(Playlists[i].Id, Playlists[i]);
		}
	}

	public PlaylistDef GetPlaylist(string Id)
	{
		PlaylistDef value = null;
		playlistDictionary.TryGetValue(Id, out value);
		return value;
	}
}
