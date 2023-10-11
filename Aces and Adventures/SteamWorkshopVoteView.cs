using Steamworks;
using TMPro;
using UnityEngine;

public class SteamWorkshopVoteView : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/SteamWorkshopVoteView";

	public BoolEvent onIsUpVoteChange;

	public BoolEvent onIsCurrentVoteChange;

	private bool _isUpVote;

	private Steam.Ugc.ItemType _itemType;

	private Steam.Ugc.Query.Result _result;

	public static SteamWorkshopVoteView Create(Steam.Ugc.Query.Result result, bool upVote, Transform parent, Steam.Ugc.ItemType itemType = Steam.Ugc.ItemType.Standard)
	{
		return DirtyPools.Unpool(_Blueprint, parent).GetComponent<SteamWorkshopVoteView>()._SetData(result, upVote, itemType);
	}

	private SteamWorkshopVoteView _SetData(Steam.Ugc.Query.Result result, bool upVote, Steam.Ugc.ItemType itemType)
	{
		_result = result;
		onIsUpVoteChange.Invoke(_isUpVote = upVote);
		_itemType = itemType;
		TooltipCreator.CreateTextTooltip(base.transform, upVote ? "I like this" : "I dislike this", beginShowTimer: false, 0.2f, backgroundEnabled: true, TextAlignmentOptions.Center, 0f, 12f, TooltipDirection.Horizontal, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator: true);
		_GetCurrentVote();
		return this;
	}

	private async void _GetCurrentVote()
	{
		SetCurrentVote((await Steam.Ugc.GetUserItemVoteAsync(_result)).CurrentVote());
	}

	private void _OnItemVoteChange(PublishedFileId_t publishedFileId, Steam.Ugc.UserItemVoteResult previousVote, Steam.Ugc.UserItemVoteResult currentVote)
	{
		if (publishedFileId == _result)
		{
			SetCurrentVote(currentVote.CurrentVote());
		}
	}

	private string _GetAuthorConfirmMessage()
	{
		if (_isUpVote)
		{
			return "Add this creator to your list of Liked Creators? This list is used by [" + EnumUtil.FriendlyName(Steam.Ugc.Query.QueryType.LikedCreators) + "] workshop searches.";
		}
		return "Add this creator to your list of Disliked Creators? <b><u>Content made by this creator will not show up in future searches!</b></u>";
	}

	private void Awake()
	{
		onIsCurrentVoteChange.Invoke(arg0: false);
	}

	private void OnEnable()
	{
		Steam.Ugc.OnItemVoteChange += _OnItemVoteChange;
	}

	private void OnDisable()
	{
		Steam.Ugc.OnItemVoteChange -= _OnItemVoteChange;
	}

	public void SetCurrentVote(bool? currentVote)
	{
		if (this.IsActiveAndEnabled())
		{
			onIsCurrentVoteChange.Invoke(currentVote == _isUpVote);
		}
	}

	public async void Vote()
	{
		bool shouldCastVote = _itemType == Steam.Ugc.ItemType.Standard;
		if (_itemType == Steam.Ugc.ItemType.Author)
		{
			string confirm = (_isUpVote ? "Like Creator" : "Dislike Creator");
			await new AwaitCoroutine<object>(Job.WaitTillDestroyed(UIUtil.CreatePopup(confirm, UIUtil.CreateMessageBox(_GetAuthorConfirmMessage(), TextAlignmentOptions.MidlineLeft), null, parent: base.transform, buttons: new string[2] { "Cancel", confirm }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				shouldCastVote = s == confirm;
			})));
		}
		if (shouldCastVote && !(await Steam.Ugc.SetUserItemVoteAsync(_result, _isUpVote)).m_eResult.Failure())
		{
			if (_isUpVote)
			{
				await Steam.Ugc.AddItemToFavoritesAsync(_result);
			}
			else
			{
				await Steam.Ugc.RemoveItemFromFavoritesAsync(_result);
			}
		}
	}
}
