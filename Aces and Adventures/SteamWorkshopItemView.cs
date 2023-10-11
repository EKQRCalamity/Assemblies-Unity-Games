using System;
using System.Threading.Tasks;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SelectableItem))]
public abstract class SteamWorkshopItemView : MonoBehaviour, ILayoutReady
{
	private static TextBuilder _Builder;

	public RectTransform upVoteContainer;

	public RectTransform downVoteContainer;

	private Steam.Ugc.Query.Result _result;

	private string _creatorName;

	private string _inspectPreviewUrl;

	protected ContentRef _contentRef;

	private static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	public Steam.Ugc.Query.Result result
	{
		get
		{
			return _result;
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _result, value))
			{
				_OnResultChange();
			}
		}
	}

	public virtual bool layoutIsReady => true;

	public static SteamWorkshopItemView Create(ContentRef contentRef, Steam.Ugc.Query.Result result, Transform parent)
	{
		return DirtyPools.Unpool((contentRef is ImageRef) ? SteamWorkshopImageView.Blueprint : ((contentRef is AudioRef) ? SteamWorkshopAudioView.Blueprint : (result.previewImageUrl.IsNullOrEmpty() ? SteamWorkshopDataView.Blueprint : SteamWorkshopPreviewView.Blueprint)), parent).GetComponent<SteamWorkshopItemView>()._SetData(contentRef, result);
	}

	private SteamWorkshopItemView _SetData(ContentRef contentRef, Steam.Ugc.Query.Result result)
	{
		_contentRef = contentRef;
		this.result = result;
		return this;
	}

	private void _OnResultChange()
	{
		_creatorName = result.details.m_ulSteamIDOwner.ToString();
		result.GetCreatorNameAsync().ContinueWith((Task<string> s) => _creatorName = s.Result);
		_OnResultSetUnique();
		_CreateVoteButtons();
		_inspectPreviewUrl = result.GetAdditionalPreviewURL("I");
	}

	private string _GetTooltipText()
	{
		Builder.Italic().Append("Title: ").EndItalic()
			.Append(result.name)
			.NewLine();
		Builder.Italic().Append("Creator: ").EndItalic()
			.Append(_creatorName)
			.NewLine();
		if (EnumUtil.FlagCount(result.mature) != 0)
		{
			Builder.Italic().Append("Mature Content: ").EndItalic()
				.Append(EnumUtil.FriendlyName(result.mature))
				.NewLine();
		}
		if (result.description.HasVisibleCharacter())
		{
			Builder.Italic().Append("Description: ").EndItalic()
				.Append(result.description)
				.NewLine();
		}
		Builder.Italic().Append("Last Updated: ").EndItalic()
			.Append($"{MathUtil.FromUnixEpoch(result.timeUpdated).ToLocalTime():f}")
			.NewLine();
		Builder.Italic().Append("Install Status: ").EndItalic()
			.Append(EnumUtil.FriendlyName(ContentRef.GetPublishedIdInstallStatus(result.id, result.timeUpdated)));
		if (!_inspectPreviewUrl.IsNullOrEmpty())
		{
			Builder.NewLine().Append("[Shift + Right Click] To View Workshop Item Details");
		}
		if (!SteamWorkshopSearcher.InDownloadQueue(result))
		{
			Builder.NewLine().Color(new Color32(128, byte.MaxValue, 128, byte.MaxValue)).Append("[Control + Left Click] To Add To Download Queue")
				.EndColor();
		}
		else
		{
			Builder.NewLine().Color(new Color32(byte.MaxValue, 64, 64, byte.MaxValue)).Append("[Control + Left Click] To Remove From Download Queue")
				.EndColor();
		}
		return Builder;
	}

	private void _CreateVoteButtons()
	{
		if ((bool)upVoteContainer && (bool)downVoteContainer)
		{
			SteamWorkshopVotesView.CreateVoteViews(_result, upVoteContainer, downVoteContainer);
		}
	}

	private void _AddToDownloadQueue(PointerEventData eventData)
	{
		if (InputManager.I[KeyModifiers.Control] && (bool)SteamWorkshopSearcher.Instance)
		{
			SteamWorkshopSearcher.Instance.ToggleToDownloadQueue(result);
		}
	}

	protected virtual void Awake()
	{
		Transform tooltipCreator = base.transform;
		Func<string> dynamicText = _GetTooltipText;
		TooltipCreator.CreateTextTooltip(tooltipCreator, "", beginShowTimer: false, 0.5f, backgroundEnabled: true, TextAlignmentOptions.Left, 0f, 12f, TooltipDirection.Vertical, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator: true, ignoreEventsWhileDragging: false, ignorePointerEvents: false, blockRayCasts: false, null, useProxyTooltipCreator: false, ignoreClear: false, useOppositeDirection: false, clearOnDisable: false, dynamicText);
		PointerClick3D componentInChildren = GetComponentInChildren<PointerClick3D>();
		componentInChildren.OnClick.AddListener(_AddToDownloadQueue);
		componentInChildren.OnRightClick.AddListener(delegate
		{
			CreateDetailsPopup();
		});
	}

	protected abstract void _OnResultSetUnique();

	public virtual Vector2 GetPreferredSize()
	{
		return (base.transform as RectTransform).GetPreferredSize();
	}

	public void CreateDetailsPopup()
	{
		if (_inspectPreviewUrl.IsNullOrEmpty() || InputManager.I[KeyModifiers.Shift])
		{
			UIUtil.CreateSteamWorkshopItemInspectPopup(_result, base.transform, _contentRef.usesWorkshopMetaData, _contentRef.usesAdditionalPreviews);
		}
		else
		{
			FullScreenImageRequestView.Create(_inspectPreviewUrl, base.transform.GetComponentInParent<Canvas>().transform, _result.audioPreviewPublishedFileId);
		}
	}

	public static implicit operator Steam.Ugc.Query.Result(SteamWorkshopItemView view)
	{
		if (!view)
		{
			return default(Steam.Ugc.Query.Result);
		}
		return view.result;
	}

	public static implicit operator PublishedFileId_t(SteamWorkshopItemView view)
	{
		return ((Steam.Ugc.Query.Result)view).details.m_nPublishedFileId;
	}
}
