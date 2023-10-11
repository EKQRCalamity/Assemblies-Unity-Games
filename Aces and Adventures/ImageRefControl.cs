using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageRefControl : MonoBehaviour
{
	public static readonly ResourceBlueprint<Texture2D> DefaultAlphaBackground = "UI/Ability/AbilityAlphaBack";

	public ImageCategoryType category;

	public ImageCategoryType? defaultCategory;

	public Texture2DEvent onImageChanged;

	public Vector2Event onImageDimensionsChanged;

	public ObjectEvent onImageRefChanged;

	[Header("Buttons")]
	public GameObject clearButton;

	public GameObject overwriteButton;

	public GameObject renameButton;

	public GameObject editTagsButton;

	public GameObject uploadButton;

	public GameObject downloadButton;

	public GameObject inspectButton;

	public Action onValidateImportCategory;

	private ImageRef _imageRef;

	private Texture2D _nullImage;

	private string _originalLabel;

	private bool _isOverwrite;

	private string originalLabel => _originalLabel ?? (_originalLabel = base.gameObject.GetUILabel());

	public void SetImageRef(ImageRef imageRef)
	{
		if (ReflectionUtil.SafeEquals(_imageRef, imageRef))
		{
			return;
		}
		imageRef = ProtoUtil.Clone(imageRef);
		_imageRef = imageRef;
		_imageRef.category = category;
		if (_imageRef.IsValid())
		{
			_imageRef.GetTexture2D(delegate
			{
				_InvokeImageEvents();
			}, forceImmediate: true);
		}
		else
		{
			_OnImageRefChanged();
			onImageChanged.Invoke(_nullImage);
		}
	}

	private void _SetCategory(ImageCategoryType newCategory)
	{
		if (newCategory != category)
		{
			category = newCategory;
			_imageRef = new ImageRef(newCategory);
		}
	}

	public void SetCategoryAndPickFirstImage(ImageCategoryType newCategory)
	{
		if (newCategory != category)
		{
			category = newCategory;
			SetImageRef(ImageRef.Search(newCategory).FirstOrDefault() ?? new ImageRef(newCategory));
		}
	}

	public void OnSelect()
	{
		UIUtil.CreateImageSearchPopup(category, SetImageRef, base.transform);
	}

	public void ShowBrowser()
	{
		UIUtil.CreateImageBrowserPopup(category, _OnImport, base.transform, _isOverwrite ? _imageRef.texture : null);
	}

	public void SetIsOverwrite(bool isOverwrite)
	{
		_isOverwrite = isOverwrite;
	}

	public void OnImport(string path)
	{
		ImageRef imageRef = ((_isOverwrite && (bool)_imageRef) ? _imageRef._Reference<ImageRef>() : null);
		_imageRef = imageRef ?? new ImageRef(category);
		UIUtil.BeginProcessJob(base.transform).Afterward().DoJob(_imageRef.Import(path))
			.Then()
			.DoJob(_imageRef.RetrieveContent())
			.Then()
			.ResultAction(delegate(Texture2D image)
			{
				ImageCategoryType imageCategoryType = category;
				if (imageCategoryType <= ImageCategoryType.Adventure)
				{
					Action onFinished = delegate
					{
						_imageRef.SetTexture(GPUImage.PadTextureToPow2(image, 0.5f, 0.5f, 8f));
						_DoSaveProcess();
					};
					if (category.PreferredSize().HasValue)
					{
						Ushort2 value = category.PreferredSize().Value;
						_imageRef.SetTexture(image.ToRenderTexture().ToTexture2D(mipmap: true, 9, FilterMode.Trilinear, TextureWrapMode.Clamp, TextureFormat.ARGB32, false));
						ImageRefUVCoords imageRefCrop = new ImageRefUVCoords
						{
							imageRef = _imageRef
						};
						UIUtil.CreatePopup("Crop Image To (" + value.x + " x " + value.y + ")", UIUtil.CreateReflectedObject(new Ref<ImageRefUVCoords>(imageRefCrop), (int)value.x, (int)value.y, persistUI: false, "UI/Reflection/UIReflectedObject", null, autoHideScrollbar: true, Vector4.zero), null, parent: base.transform, buttons: new string[1] { "Apply Crop" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: false, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate
						{
							Int4 @int = (imageRefCrop.uvCoords ?? UVCoords.Default).ToPixelRect(NConvert.GetImageInfo(path));
							NConvert.CropAndResize(path, _imageRef.savePath, @int.x, @int.y, @int.z, @int.w, category.MaxSaveResolution());
							UIUtil.BeginProcessJob(base.transform).Afterward().DoJob(_imageRef.ReloadContent())
								.Then()
								.ResultAction(delegate(Texture2D croppedImage)
								{
									image = croppedImage;
									onFinished();
								})
								.Afterward()
								.Do(UIUtil.EndProcess);
						});
					}
					else
					{
						onFinished();
					}
				}
			})
			.Afterward()
			.Do(UIUtil.EndProcess);
	}

	public void OnEditTags()
	{
		if ((bool)_imageRef)
		{
			UIUtil.CreateContentRefTagsPopup(_imageRef, base.transform);
		}
	}

	public void OnRename()
	{
		if ((bool)_imageRef)
		{
			UIUtil.CreateContentRefRenamePopup(_imageRef, base.transform, delegate
			{
				_UpdateLabel();
			});
		}
	}

	public void Clear()
	{
		SetImageRef(new ImageRef(category));
	}

	public void Upload()
	{
		if (_imageRef.CanUpload())
		{
			UIUtil.CreateContentRefUploadPopup(_imageRef, base.transform, _OnUpload);
		}
	}

	public void Download()
	{
		UIUtil.CreateWorkshopImageSearchPopup(_imageRef ?? new ImageRef(category), SetImageRef, base.transform);
	}

	public void InspectWorkshopItem()
	{
		if (_imageRef.CanInspectWorkshopItem())
		{
			UIUtil.CreateSteamWorkshopItemInspectPopup(_imageRef.workshopFileId, base.transform);
		}
	}

	private void _DoSaveProcess()
	{
		UIUtil.BeginProcessJob(base.transform).ChainJobs(_imageRef.Save, _imageRef.ReloadContent, () => Job.Action(delegate
		{
			onImageRefChanged.Invoke(new ImageRef(category));
			_InvokeImageEvents();
			if (onValidateImportCategory != null)
			{
				onValidateImportCategory();
			}
			UIUtil.EndProcess();
		}));
	}

	private void _InvokeImageEvents()
	{
		_OnImageRefChanged();
		Texture2D texture = _imageRef.texture;
		if (texture != null)
		{
			texture.name = _imageRef.name;
			onImageChanged.Invoke(texture);
			onImageDimensionsChanged.Invoke(new Vector2(texture.width, texture.height));
		}
	}

	private void _UpdateLabel()
	{
		if ((bool)this)
		{
			base.gameObject.SetUILabel(_imageRef ? (originalLabel + ": " + _imageRef.name) : originalLabel);
		}
	}

	private void _UpdateButtons()
	{
		bool flag = _imageRef;
		bool active = flag && _imageRef.belongsToCurrentCreator;
		clearButton.SetActive(flag);
		overwriteButton.SetActive(active);
		renameButton.SetActive(active);
		editTagsButton.SetActive(active);
		uploadButton.SetActive(_imageRef.CanUpload());
		downloadButton.SetActive(Steam.CanUseWorkshop);
		inspectButton.SetActive(_imageRef.CanInspectWorkshopItem());
	}

	private void _OnImageRefChanged()
	{
		onImageRefChanged.Invoke(_imageRef);
		if ((bool)this)
		{
			_UpdateButtons();
			_UpdateLabel();
		}
	}

	private void _OnImport(string path)
	{
		UIUtil.ConfirmImportPopup(path, OnImport, _SetCategory, _isOverwrite, category, defaultCategory, "Image", base.transform);
	}

	private void _OnUpload(bool success)
	{
		if (success)
		{
			_UpdateButtons();
		}
	}

	private void Awake()
	{
		RawImage componentInChildren = GetComponentInChildren<RawImage>();
		_nullImage = componentInChildren.texture as Texture2D;
		DataRefControl.SetWorkshopButtonTooltips(uploadButton, downloadButton, inspectButton);
	}

	private void Start()
	{
		_imageRef = _imageRef ?? new ImageRef(category);
		_UpdateButtons();
	}
}
