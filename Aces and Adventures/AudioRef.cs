using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Networking;

[ProtoContract]
[UIField]
public class AudioRef : ContentRef
{
	public const float MAX_TIME = 600f;

	public const int WAVEFORM_RESOLUTION = 300;

	public const float WAVEFORM_BAR_WIDTH = 0.5f;

	public const int WAVEFORM_SAMPLES = 20;

	public const AudioCategoryTypeFlags ENTITY_AUDIO = AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal;

	public new static readonly Func<AudioRef, string> GetSearchStringFunc;

	private static HashSet<AudioCategoryType> _LoadedCategoryTypes;

	[ProtoMember(1)]
	private AudioCategoryType _category;

	[ProtoMember(2, OverwriteList = true)]
	private byte[] _waveFormData;

	[ProtoMember(3)]
	private string _name;

	[ProtoMember(4)]
	private string _tags;

	public AudioCategoryType category
	{
		get
		{
			return _category;
		}
		set
		{
			_category = value;
		}
	}

	public byte[] waveFormData
	{
		get
		{
			return _Reference<AudioRef>()._waveFormData;
		}
		set
		{
			_Reference<AudioRef>()._waveFormData = value;
		}
	}

	public AudioClip loadedAudioClip => base.content as AudioClip;

	public AudioClip audioClip
	{
		get
		{
			object obj = loadedAudioClip;
			if (obj == null)
			{
				object obj3 = (base.content = Resources.Load<AudioClip>(base.loadPath));
				obj = obj3 as AudioClip;
			}
			return (AudioClip)obj;
		}
	}

	public bool forceMono => category.ForceMono();

	public float? loudnessTarget => category.LoudnessTarget();

	public float maxPeak => category.MaxPeak();

	public bool dualMono => category.IsStereo();

	public bool compressOnLoad => base.committed;

	public bool isStreaming
	{
		get
		{
			if (base.committed)
			{
				return category.IsStreaming();
			}
			return false;
		}
	}

	public AudioClipLoadType loadType
	{
		get
		{
			if (!isStreaming)
			{
				if (!compressOnLoad)
				{
					return AudioClipLoadType.DecompressOnLoad;
				}
				return AudioClipLoadType.CompressedInMemory;
			}
			return AudioClipLoadType.Streaming;
		}
	}

	public bool is3D => true;

	public int quality
	{
		get
		{
			if (!base.isResource)
			{
				return 3 + category.IsStreaming().ToInt(2) + category.IsStereo().ToInt();
			}
			return 10;
		}
	}

	public float assetCompressionQuality
	{
		get
		{
			if (!base.committed)
			{
				return 1f;
			}
			return category.AssetCompressionQuality();
		}
	}

	public float maxClipLength
	{
		get
		{
			if (!base.isResource)
			{
				return category.MaxLength();
			}
			return 600f;
		}
	}

	public override string categoryName => EnumUtil.Name(_category);

	public override string friendlyName => name;

	public override string name
	{
		get
		{
			return _Reference<AudioRef>()._name ?? (name = base.friendlyName);
		}
		set
		{
			_Reference<AudioRef>()._name = value;
		}
	}

	public override string tags
	{
		get
		{
			return _Reference<AudioRef>()._tags;
		}
		set
		{
			_Reference<AudioRef>()._tags = value;
		}
	}

	public override byte maxNameLength => 42;

	public override string type => "Audio";

	protected override byte _keyCategoryId => (byte)_category;

	public override string specificType => EnumUtil.FriendlyName(category) + " Audio";

	public override Type dataType => typeof(AudioClip);

	public override ContentRefType contentType => ContentRefType.Audio;

	public override bool usesWorkshopMetaData => true;

	private bool _waveFormDataSpecified => base.IsSavingReference;

	private bool _tagsSpecified => base.IsSavingReference;

	private bool _nameSpecified => base.IsSavingReference;

	static AudioRef()
	{
		_LoadedCategoryTypes = new HashSet<AudioCategoryType>();
		GetSearchStringFunc = (AudioRef audioRef) => audioRef.GetSearchString();
	}

	public static IEnumerable<AudioRef> Search(AudioCategoryType category)
	{
		LoadAll(category);
		foreach (ContentRef value in ContentRef.ContentReferences.Values)
		{
			if (value is AudioRef audioRef && audioRef.category == category && (bool)audioRef)
			{
				yield return audioRef;
			}
		}
	}

	public static void LoadAll(AudioCategoryType category)
	{
		if (_LoadedCategoryTypes.Add(category))
		{
			ContentRef._LoadContentReferencesInFolder("Audio/" + category);
		}
	}

	public static void ClearLoadedCache()
	{
		_LoadedCategoryTypes.Clear();
	}

	protected AudioRef()
	{
	}

	public AudioRef(AudioCategoryType category)
	{
		this.category = category;
	}

	protected override string GetExtension()
	{
		if (!base.committed)
		{
			return ".wav";
		}
		return ".ogg";
	}

	protected override IEnumerator Import(string inputFile, string contentSavePath, bool isResource)
	{
		name = _Reference<AudioRef>()._name ?? Path.GetFileNameWithoutExtension(inputFile).FriendlyFromLowerCaseUnderscore().MaxLengthOf(maxNameLength);
		waveFormData = null;
		yield return ToBackgroundThread.Create();
		FFMpeg.Transcode(inputFile, contentSavePath, null, forceMono);
	}

	protected override void _OnFirstTrackedUnique(ContentRef loadedReferenceFile)
	{
		if (loadedReferenceFile is AudioRef audioRef)
		{
			waveFormData = audioRef._waveFormData;
			tags = audioRef._tags;
			name = audioRef._name;
		}
	}

	protected override IEnumerator LoadResource()
	{
		ResourceRequest request = Resources.LoadAsync<AudioClip>(base.loadPath);
		while (request.asset == null)
		{
			yield return null;
		}
		AudioClip clip = (AudioClip)request.asset;
		if (clip.loadType != AudioClipLoadType.Streaming)
		{
			clip.LoadAudioData();
			while (clip.loadState == AudioDataLoadState.Loading)
			{
				yield return null;
			}
		}
		yield return clip;
	}

	protected override IEnumerator LoadUGC()
	{
		return _LoadUGC(loadType);
	}

	private IEnumerator _LoadUGC(AudioClipLoadType loadType)
	{
		using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(base.loadPath.ToURI(), AudioType.UNKNOWN);
		((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = isStreaming;
		((DownloadHandlerAudioClip)request.downloadHandler).compressed = compressOnLoad;
		request.SendWebRequest();
		while (!request.downloadHandler.isDone)
		{
			yield return null;
		}
		yield return DownloadHandlerAudioClip.GetContent(request);
	}

	protected override void OnSaveRefValidation()
	{
		GetWaveFormData(delegate(Vector2[] data)
		{
			byte[] array = new byte[data.Length / 2];
			int num = 0;
			for (int i = 0; i < array.Length; i += 2)
			{
				Alpha2 alpha = Alpha2.Signed(new Vector2(data[num].y, data[num + 1].y));
				array[i] = alpha.xByte;
				array[i + 1] = alpha.yByte;
				num += 4;
			}
			waveFormData = (_waveFormData = array);
		}, forceImmediate: true);
	}

	protected override IEnumerator SaveContent()
	{
		_CleanDirectory();
		yield break;
	}

	public void GetAudioClip(Action<AudioClip> onAudioClipRetrieved)
	{
		_GetContent(onAudioClipRetrieved, forceImmediate: false);
	}

	public void GetGeneratedFromRef<O>(Func<AudioRef, IEnumerator> generateLogic, Action<O> onGeneratedContentRetrieved, bool forceImmediate = false, string name = "")
	{
		_GetGeneratedFromRef(generateLogic, name, onGeneratedContentRetrieved, forceImmediate);
	}

	public void GetGeneratedFromContent<O>(Func<AudioClip, IEnumerator> generateLogic, Action<O> onGeneratedContentRetrieved, bool forceImmediate = false, string name = "")
	{
		_GetGeneratedFromContent(generateLogic, name, onGeneratedContentRetrieved, forceImmediate);
	}

	public Job GenerateFromContent<O>(Func<AudioClip, IEnumerator> generateLogic, string name = "")
	{
		return _GenerateFromContent<AudioClip, O>(generateLogic, name);
	}

	public override IEnumerable<ContentRef> SearchSimilar()
	{
		return Search(category);
	}

	protected override void _LoadSimilarReferences()
	{
		LoadAll(category);
	}

	protected override byte[] _GetWorkshopMetaData()
	{
		return waveFormData;
	}

	protected override async Task<bool> _IsContentValidForUpload()
	{
		return FFMpeg.GetLength(base.loadPath) <= maxClipLength;
	}

	private static IEnumerator _GetAudioMeshVertexPositions(AudioRef audioRef)
	{
		if (!audioRef.hasContent)
		{
			yield return null;
			yield break;
		}
		int channels = audioRef.audioClip.channels;
		yield return ToBackgroundThread.Create();
		Vector2[] positions = null;
		UIAudioMesh.GetUnitizedBarGraph(ref positions, FFMpeg.GetSamples(Path.ChangeExtension(audioRef.savePath, ".ogg"), (ushort)2205).ToArray(), channels, 300, 0.5f, 0);
		yield return positions;
	}

	private static IEnumerator _GetAudioMeshFromWaveForm(AudioRef audioRef)
	{
		yield return AudioUtil.CreateAudioVertexDataFromBytes(audioRef.waveFormData);
	}

	private static IEnumerator _GetWaveFormTexture(AudioRef audioRef)
	{
		IEnumerator retrieveWaveForm = FFMpeg.GetWaveFormTextureProcess(audioRef.savePath, 128, 32, nonReadable: false);
		object output = null;
		while (retrieveWaveForm.MoveNext())
		{
			object current;
			output = (current = retrieveWaveForm.Current);
			yield return current;
		}
		Texture2D texture2D = output as Texture2D;
		Texture2D texture2D2 = new Texture2D(texture2D.width, texture2D.height, TextureFormat.Alpha8, mipChain: false)
		{
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp
		};
		texture2D2.SetPixels32(texture2D.GetPixels32());
		texture2D2.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		UnityEngine.Object.Destroy(texture2D);
		yield return texture2D2;
	}

	public void GetWaveFormData(Action<Vector2[]> onData, bool forceImmediate = false)
	{
		if (waveFormData != null)
		{
			GetGeneratedFromRef(_GetAudioMeshFromWaveForm, onData, forceImmediate, "WaveForm");
		}
		else
		{
			GetGeneratedFromRef(_GetAudioMeshVertexPositions, onData, forceImmediate);
		}
	}

	public void GetWaveFormTexture(Action<Texture2D> onTexture)
	{
		GetGeneratedFromRef(_GetWaveFormTexture, onTexture);
	}
}
