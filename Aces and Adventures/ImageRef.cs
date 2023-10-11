using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class ImageRef : ContentRef
{
	public enum ImageFormat
	{
		png,
		jpg
	}

	public new static readonly Func<ImageRef, string> GetSearchStringFunc;

	private static HashSet<ImageCategoryType> _LoadedCategoryTypes;

	[ProtoMember(1)]
	private ImageCategoryType _category;

	[ProtoMember(3)]
	private string _name;

	[ProtoMember(4)]
	private string _tags;

	[ProtoMember(2)]
	public Short2 dimensions { get; private set; }

	public override string name
	{
		get
		{
			return _Reference<ImageRef>()._name ?? (name = base.friendlyName);
		}
		set
		{
			_Reference<ImageRef>()._name = value;
		}
	}

	public override string tags
	{
		get
		{
			return _Reference<ImageRef>()._tags;
		}
		set
		{
			_Reference<ImageRef>()._tags = value;
		}
	}

	public ImageCategoryType category
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

	public override string categoryName => EnumUtil.Name(_category);

	public Texture2D texture => GetContentImmediate() as Texture2D;

	private int AnisoLevel => 16;

	private TextureWrapMode WrapMode => TextureWrapMode.Clamp;

	private FilterMode FilterMode => FilterMode.Trilinear;

	protected virtual int MaxResolutionOnImport => category.MaxImportResolution();

	protected virtual bool usesAlpha => category.UsesAlpha();

	protected virtual bool useLinearColorOnLoad
	{
		get
		{
			if (!usesAlpha)
			{
				return !base.committed;
			}
			return base.committed;
		}
	}

	protected ImageFormat format
	{
		get
		{
			if (!usesAlpha && !base.isResource)
			{
				if (!base.committed)
				{
					return ImageFormat.png;
				}
				return ImageFormat.jpg;
			}
			return ImageFormat.png;
		}
	}

	public override string friendlyName => name;

	public override string type => "Image";

	protected override byte _keyCategoryId => (byte)(_category + 32);

	public override string specificType => EnumUtil.FriendlyName(category) + " Image";

	public override Type dataType => typeof(Texture2D);

	public override ContentRefType contentType => ContentRefType.Image;

	protected override string _previewFilePath => base.savePath;

	private bool _nameSpecified => base.IsSavingReference;

	private bool _tagsSpecified => base.IsSavingReference;

	static ImageRef()
	{
		_LoadedCategoryTypes = new HashSet<ImageCategoryType>();
		GetSearchStringFunc = (ImageRef imageRef) => imageRef.GetSearchString();
	}

	public static IEnumerable<ImageRef> Search(ImageCategoryType category)
	{
		LoadAll(category);
		foreach (ContentRef value in ContentRef.ContentReferences.Values)
		{
			if (value is ImageRef imageRef && imageRef.category == category && (bool)imageRef)
			{
				yield return imageRef;
			}
		}
	}

	public static void LoadAll(ImageCategoryType category)
	{
		if (_LoadedCategoryTypes.Add(category))
		{
			ContentRef._LoadContentReferencesInFolder("Image/" + category);
		}
	}

	public static void ClearLoadedCache()
	{
		_LoadedCategoryTypes.Clear();
	}

	protected ImageRef()
	{
	}

	public ImageRef(ImageCategoryType category)
	{
		this.category = category;
	}

	protected override string GetExtension()
	{
		return format.GetExtension();
	}

	protected override IEnumerator Import(string inputPath, string contentSavePath, bool isResource)
	{
		name = _Reference<ImageRef>()._name ?? Path.GetFileNameWithoutExtension(inputPath).FriendlyFromLowerCaseUnderscore().MaxLengthOf(maxNameLength);
		yield return ToBackgroundThread.Create();
		NConvert.Convert("png", inputPath, contentSavePath, MaxResolutionOnImport, preserveAspect: true, NConvertResizeSide.longest, NConvertResizeType.lanczos, NConvertResizeFlag.decr, overwrite: true, category.MaxImportDimensions());
	}

	protected override void _OnFirstTrackedUnique(ContentRef loadedReferenceFile)
	{
		if (loadedReferenceFile is ImageRef imageRef)
		{
			tags = imageRef._tags;
			name = imageRef._name;
		}
	}

	protected override IEnumerator LoadResource()
	{
		ResourceRequest request = Resources.LoadAsync<Texture2D>(base.loadPath);
		while (request.asset == null)
		{
			yield return null;
		}
		yield return request.asset;
	}

	protected override IEnumerator LoadUGC()
	{
		byte[] bytes = File.ReadAllBytes(base.loadPath);
		yield return null;
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, category.UseMipmap(), useLinearColorOnLoad);
		texture2D.LoadImage(bytes, base.committed);
		texture2D.anisoLevel = AnisoLevel;
		texture2D.wrapMode = WrapMode;
		texture2D.filterMode = FilterMode;
		yield return texture2D;
	}

	protected override IEnumerator SaveContent()
	{
		byte[] bytes = GetContentBytesInMemory();
		yield return ToBackgroundThread.Create();
		IOUtil.WriteBytes(base.savePath, bytes);
		_CleanDirectory();
	}

	protected override void OnLoadValidation()
	{
		_UpdateDimensions();
	}

	protected override void OnSaveRefValidation()
	{
		_UpdateDimensions();
	}

	public override byte[] GetContentBytesInMemory()
	{
		if (format != 0)
		{
			return texture.EncodeToJPG(90);
		}
		return texture.EncodeToPNG();
	}

	public override IEnumerable<ContentRef> SearchSimilar()
	{
		return Search(category);
	}

	protected override void _LoadSimilarReferences()
	{
		LoadAll(category);
	}

	protected override async Task<bool> _IsContentValidForUpload()
	{
		if (NConvert.ResizeIfNecessary(base.loadPath, category.MaxSaveResolution()) && Steam.DEBUG.LogWarning())
		{
			Log.Warning($"<b>{base.detailedName}</b> has been resized in order to conform to max resolution of {category.MaxSaveResolution()}.");
		}
		return true;
	}

	private void _UpdateDimensions()
	{
		dimensions = new Short2(texture.width, texture.height);
	}

	public Texture2D SetTexture(Texture2D texture)
	{
		base.content = texture;
		_UpdateDimensions();
		_MarkAsUncommitted();
		return texture;
	}

	public void GetTexture2D(Action<Texture2D> onTexture2DRetrieved, bool forceImmediate = false)
	{
		_GetContent(onTexture2DRetrieved, forceImmediate);
	}

	public void GetGeneratedFromContent<O>(Func<Texture2D, IEnumerator> generateLogic, Action<O> onGeneratedContentRetrieved, bool forceImmediate = false, string name = "")
	{
		_GetGeneratedFromContent(generateLogic, name, onGeneratedContentRetrieved, forceImmediate);
	}

	public O GetGeneratedContent<O>(string name)
	{
		return _GetGeneratedContent<Texture2D, O>(name);
	}
}
