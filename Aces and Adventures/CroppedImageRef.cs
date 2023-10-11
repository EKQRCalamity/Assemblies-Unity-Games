using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
[UIDeepValidate]
public class CroppedImageRef
{
	[ProtoMember(1)]
	[UIField(validateOnChange = true, onValueChangedMethod = "_OnImageChanged", dynamicInitMethod = "_InitImage", collapse = UICollapseType.Open)]
	private ImageRef _image;

	[ProtoMember(2)]
	[UIField(dynamicInitMethod = "_InitCrop")]
	[UIHideIf("_hideImageCrop")]
	private ImageRefUVCoords _crop;

	[ProtoMember(3)]
	private Ushort2 _preferredSize;

	[ProtoMember(4, IsRequired = true)]
	[UIField(">Image Library To Use", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true, excludedValuesMethod = "_ExcludedCategory", order = 1u)]
	[UIHideIf("_hideCategory")]
	private ImageCategoryType _category;

	private ImageCategoryType _defaultCategory;

	private ImageCategoryFlags? _categoryFlags;

	public ImageRef image => _image;

	public ImageRefUVCoords crop => _crop ?? (_crop = new ImageRefUVCoords());

	public Rect uvRect => this;

	private bool _hideImageCrop => !image.IsValid();

	private bool _hideCategory => !_categoryFlags.HasValue;

	private CroppedImageRef()
	{
	}

	public CroppedImageRef(ImageCategoryType category, Ushort2 preferredSize, ImageCategoryFlags? additionalCategories = null)
	{
		_category = category;
		_defaultCategory = category;
		_preferredSize = preferredSize;
		_categoryFlags = (additionalCategories.HasValue ? new ImageCategoryFlags?(additionalCategories.Value | EnumUtil<ImageCategoryType>.ConvertToFlag<ImageCategoryFlags>(_category)) : null);
	}

	public CroppedImageRef SetData(ImageRef imageRef)
	{
		if (ContentRef.Equal(_image, imageRef))
		{
			return this;
		}
		_image = imageRef;
		crop.uvCoords = UVCoords.FromPreferredAndActualSize(_preferredSize, _image.dimensions);
		return this;
	}

	public override string ToString()
	{
		if (!_image.IsValid())
		{
			return "Null Cropped Image Reference";
		}
		return _image.friendlyName;
	}

	public static implicit operator bool(CroppedImageRef croppedImageRef)
	{
		return croppedImageRef?.image.IsValid() ?? false;
	}

	public static implicit operator ImageRef(CroppedImageRef croppedImageRef)
	{
		return croppedImageRef?.image;
	}

	public static implicit operator UVCoords?(CroppedImageRef croppedImageRef)
	{
		return croppedImageRef?.crop.uvCoords;
	}

	public static implicit operator UVCoords(CroppedImageRef croppedImageRef)
	{
		return ((UVCoords?)croppedImageRef) ?? UVCoords.Default;
	}

	public static implicit operator Rect(CroppedImageRef croppedImageRef)
	{
		return (UVCoords)croppedImageRef;
	}

	public static implicit operator Texture2D(CroppedImageRef croppedImageRef)
	{
		if (!croppedImageRef)
		{
			return null;
		}
		return croppedImageRef.image.texture;
	}

	public void OnValidateUI()
	{
		if (!_hideImageCrop)
		{
			if (image.category == _category)
			{
				crop.imageRef = image;
			}
			else
			{
				_image = null;
			}
		}
	}

	private void _InitImage(UIFieldAttribute uiField)
	{
		uiField.filter = _category;
		uiField.defaultValue = _defaultCategory;
	}

	private void _InitCrop(UIFieldAttribute uiField)
	{
		uiField.filter = new Int2(_preferredSize.x, _preferredSize.y);
	}

	private void _OnImageChanged()
	{
		crop.uvCoords = null;
		if ((bool)image)
		{
			_category = image.category;
		}
	}

	private bool _ExcludedCategory(ImageCategoryType category)
	{
		if (_categoryFlags.HasValue)
		{
			return !EnumUtil.HasFlagConvert(_categoryFlags.Value, category);
		}
		return false;
	}
}
