using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
public class NodeGraphFlags
{
	[ProtoContract]
	[UIField]
	[UIDeepValidate]
	public class FlagMedia
	{
		public static readonly FlagMedia Default = new FlagMedia();

		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		[DefaultValue(FlagVisibility.Hidden)]
		private FlagVisibility _flagVisibility = FlagVisibility.Hidden;

		[ProtoMember(2)]
		[UIField("Icon", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Hide)]
		[UIHideIf("_hideCommon")]
		private CroppedImageRef _image = new CroppedImageRef(ImageCategoryType.Ability, default(Ushort2));

		[ProtoMember(3)]
		[UIField(view = "UI/Input Field Multiline", collapse = UICollapseType.Open, max = 512)]
		[UIHideIf("_hideCommon")]
		private string _description;

		[ProtoMember(5)]
		[UIField]
		private string _category;

		[ProtoMember(4)]
		[UIField]
		private string _nameOverride;

		public bool isHidden => _flagVisibility != FlagVisibility.Shown;

		public FlagVisibility visibility => _flagVisibility;

		public CroppedImageRef image => _image;

		public string description
		{
			get
			{
				if (!_description.IsNullOrEmpty())
				{
					return _description;
				}
				return null;
			}
		}

		public string nameOverride
		{
			get
			{
				if (!_nameOverride.IsNullOrEmpty())
				{
					return _nameOverride;
				}
				return null;
			}
		}

		public string category => _category ?? "";

		private string _categoryToString
		{
			get
			{
				if (category.IsNullOrEmpty())
				{
					return "";
				}
				return $", <b>Category:</b> {category}";
			}
		}

		private string _nameOverrideToString
		{
			get
			{
				if (nameOverride.IsNullOrEmpty())
				{
					return "";
				}
				return $", <b>Alias:</b> {nameOverride}";
			}
		}

		private bool _hideCommon => isHidden;

		public void PrepareForSave()
		{
			_description = _description.SetRedundantStringNull();
			_nameOverride = _nameOverride.SetRedundantStringNull();
			_category = _category.SetRedundantStringNull();
		}

		public string GetCategory(string fallback = "")
		{
			if (!category.HasVisibleCharacter())
			{
				return fallback;
			}
			return category;
		}

		public override string ToString()
		{
			return (isHidden ? "<i>Hidden Flag</i>" : $"<b>Icon:</b> {image.image.GetFriendlyName()}") + _categoryToString + _nameOverrideToString;
		}

		public static implicit operator CroppedImageRef(FlagMedia media)
		{
			if (media == null || !media.image)
			{
				return null;
			}
			return media.image;
		}

		private void OnValidateUI()
		{
			image.OnValidateUI();
		}
	}

	[ProtoContract(EnumPassthru = true)]
	public enum FlagVisibility : byte
	{
		Shown,
		Hidden,
		HiddenOverride
	}

	private const int MAX_LENGTH = 24;

	[ProtoMember(1)]
	[UIField(showAddData = true, maxCount = 1000, collapse = UICollapseType.Open, filter = 10)]
	[UIFieldKey(defaultValue = "New Flag Name", max = 24, inCollection = UIElementType.ReadOnly)]
	[UIFieldValue(flexibleWidth = 4f)]
	[UIFieldCollectionItem]
	private Dictionary<string, FlagMedia> _flags = new Dictionary<string, FlagMedia>(StringComparer.OrdinalIgnoreCase);

	[ProtoMember(2)]
	private NodeDataFlagType _flagType;

	private Dictionary<string, FlagMedia> flags => _flags ?? (_flags = new Dictionary<string, FlagMedia>(StringComparer.OrdinalIgnoreCase));

	public NodeDataFlagType flagType
	{
		get
		{
			return _flagType;
		}
		set
		{
			_flagType = value;
		}
	}

	public int count => flags.Count;

	private NodeGraphFlags()
	{
	}

	public NodeGraphFlags(NodeDataFlagType flagType)
	{
		_flagType = flagType;
	}

	public void PrepareForSave()
	{
		foreach (FlagMedia value in flags.Values)
		{
			value.PrepareForSave();
		}
	}

	public FlagMedia GetMedia(string flag)
	{
		if (!flags.ContainsKey(flag))
		{
			return FlagMedia.Default;
		}
		return flags[flag];
	}

	public void SetMedia(string flag, FlagMedia media)
	{
		flags[flag] = media;
	}

	public Dictionary<string, string> GetUIListData()
	{
		return flags.ToDictionary((KeyValuePair<string, FlagMedia> p) => p.Key, (KeyValuePair<string, FlagMedia> p) => p.Key, StringComparer.OrdinalIgnoreCase);
	}

	public Dictionary<string, string> GetUIListCategoryData()
	{
		return flags.ToDictionary((KeyValuePair<string, FlagMedia> p) => p.Key, (KeyValuePair<string, FlagMedia> p) => GetMedia(p.Key).GetCategory(" <i>No Category"), StringComparer.OrdinalIgnoreCase);
	}

	public void Add(string newFlag, FlagMedia flagMedia = null)
	{
		if (!flags.ContainsKey(newFlag))
		{
			flags.Add(newFlag, flagMedia ?? new FlagMedia());
		}
	}

	public string GetFirstFlag()
	{
		return flags.Keys.FirstOrDefault();
	}

	public void CopyMediaFrom(NodeGraphFlags copyFrom)
	{
		foreach (KeyValuePair<string, FlagMedia> flag in copyFrom.flags)
		{
			if (GetMedia(flag.Key).visibility == FlagVisibility.Hidden)
			{
				SetMedia(flag.Key, flag.Value);
			}
		}
	}

	public bool HasFlag(string flag)
	{
		return flags.ContainsKey(flag);
	}
}
