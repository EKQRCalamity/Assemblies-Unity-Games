using Steamworks;
using UnityEngine;

public class SteamWorkshopVotesView : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/SteamWorkshopVotesView";

	public RectTransform upVoteContainer;

	public RectTransform downVoteContainer;

	public StringEvent onLikeCountTextChange;

	public StringEvent onDislikeCountTextChange;

	public FloatEvent onLikeRatioChange;

	private Steam.Ugc.Query.Result _result;

	public static SteamWorkshopVotesView Create(Steam.Ugc.Query.Result result, Transform parent, Steam.Ugc.ItemType itemType = Steam.Ugc.ItemType.Standard)
	{
		return DirtyPools.Unpool(_Blueprint, parent).GetComponent<SteamWorkshopVotesView>()._SetData(result, itemType);
	}

	public static void CreateVoteViews(Steam.Ugc.Query.Result result, RectTransform upVoteContainer, RectTransform downVoteContainer, Steam.Ugc.ItemType itemType = Steam.Ugc.ItemType.Standard)
	{
		SteamWorkshopVoteView.Create(result, upVote: true, upVoteContainer, itemType);
		SteamWorkshopVoteView.Create(result, upVote: false, downVoteContainer, itemType);
	}

	private SteamWorkshopVotesView _SetData(Steam.Ugc.Query.Result result, Steam.Ugc.ItemType itemType)
	{
		_result = result;
		CreateVoteViews(_result, upVoteContainer, downVoteContainer, itemType);
		_Update();
		return this;
	}

	private void _Update()
	{
		if (this.IsActiveAndEnabled())
		{
			onLikeCountTextChange.Invoke(StringUtil.FormatLargeNumber(_result.likes));
			onDislikeCountTextChange.Invoke(StringUtil.FormatLargeNumber(_result.dislikes));
			onLikeRatioChange.Invoke(_result.hasBeenVotedOn ? ((float)_result.likes / (float)_result.totalVotes) : 0.5f);
		}
	}

	private void _OnVoteChange(PublishedFileId_t publishedFileId, Steam.Ugc.UserItemVoteResult previousVote, Steam.Ugc.UserItemVoteResult currentVote)
	{
		if (!(publishedFileId != _result))
		{
			bool? flag = previousVote.CurrentVote();
			if (flag == true)
			{
				_result.details.m_unVotesUp--;
			}
			else if (flag == false)
			{
				_result.details.m_unVotesDown--;
			}
			bool? flag2 = currentVote.CurrentVote();
			if (flag2 == true)
			{
				_result.details.m_unVotesUp++;
			}
			else if (flag2 == false)
			{
				_result.details.m_unVotesDown++;
			}
			_Update();
		}
	}

	private void OnEnable()
	{
		Steam.Ugc.OnItemVoteChange += _OnVoteChange;
	}

	private void OnDisable()
	{
		Steam.Ugc.OnItemVoteChange -= _OnVoteChange;
	}
}
