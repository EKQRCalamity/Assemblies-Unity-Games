using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Steamworks;

public static class SteamEx
{
	public static async Task<Steam.CallResultData<T>> ResultAsync<T>(this SteamAPICall_t apiCall, Action process = null) where T : struct
	{
		Steam.CallResultData<T> output = default(Steam.CallResultData<T>);
		if (apiCall == SteamAPICall_t.Invalid)
		{
			if (Steam.DEBUG.LogError())
			{
				Log.Error("ResultAsync<" + typeof(T).Name + "> was passed an invalid SteamAPICall_t");
			}
			return default(Steam.CallResultData<T>);
		}
		await new AwaitCondition(delegate
		{
			if (SteamUtils.IsAPICallCompleted(apiCall, out output.failure))
			{
				return true;
			}
			if (process != null)
			{
				process();
			}
			return false;
		});
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
		Marshal.StructureToPtr(output.result, intPtr, fDeleteOld: false);
		SteamUtils.GetAPICallResult(apiCall, intPtr, Marshal.SizeOf<T>(), CallbackIdentities.GetCallbackIdentity(typeof(T)), out output.failure);
		output.result = Marshal.PtrToStructure<T>(intPtr);
		Marshal.DestroyStructure<T>(intPtr);
		Marshal.FreeHGlobal(intPtr);
		return output;
	}

	public static bool Success(this EResult result)
	{
		if (result != EResult.k_EResultOK)
		{
			return result == EResult.k_EResultAdministratorOK;
		}
		return true;
	}

	public static bool Failure(this EResult result)
	{
		return !result.Success();
	}

	public static Steam.Ugc.Query.Result GetResultAtIndex(this SteamUGCQueryCompleted_t queryCompleted, uint index, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false, uint startIndex = 0u)
	{
		Steam.Ugc.Query.Result result = default(Steam.Ugc.Query.Result);
		SteamUGC.GetQueryUGCResult(queryCompleted.m_handle, index, out result.details);
		if (returnMetaData && SteamUGC.GetQueryUGCMetadata(queryCompleted.m_handle, index, out var pchMetadata, 5000u) && !pchMetadata.IsNullOrEmpty())
		{
			result.metaData = Convert.FromBase64String(pchMetadata);
		}
		if (returnChildren)
		{
			result.children = new PublishedFileId_t[result.details.m_unNumChildren];
			SteamUGC.GetQueryUGCChildren(queryCompleted.m_handle, index, result.children, result.details.m_unNumChildren);
		}
		if (returnKeyValueTags)
		{
			int queryUGCNumKeyValueTags = (int)SteamUGC.GetQueryUGCNumKeyValueTags(queryCompleted.m_handle, index);
			result.keyValueTags = new Dictionary<string, string>(queryUGCNumKeyValueTags, StringComparer.OrdinalIgnoreCase);
			for (uint num = 0u; num < queryUGCNumKeyValueTags; num++)
			{
				if (SteamUGC.GetQueryUGCKeyValueTag(queryCompleted.m_handle, index, num, out var pchKey, 128u, out var pchValue, 128u))
				{
					result.keyValueTags[pchKey] = pchValue;
				}
				else if (Steam.DEBUG.LogError())
				{
					Log.Error("GetResultAtIndex failed to GetQueryUGCKeyValueTag");
				}
			}
		}
		if (returnAdditionalPreviews)
		{
			uint queryUGCNumAdditionalPreviews = SteamUGC.GetQueryUGCNumAdditionalPreviews(queryCompleted.m_handle, index);
			result.additionalPreviews = new Steam.Ugc.Query.Result.AdditionalPreview[queryUGCNumAdditionalPreviews];
			for (uint num2 = 0u; num2 < queryUGCNumAdditionalPreviews; num2++)
			{
				result.additionalPreviews[num2] = new Steam.Ugc.Query.Result.AdditionalPreview(queryCompleted.m_handle, index, num2);
			}
		}
		if (!SteamUGC.GetQueryUGCPreviewURL(queryCompleted.m_handle, index, out result.previewImageUrl, 2048u))
		{
			result.previewImageUrl = null;
		}
		result.index = startIndex + index;
		return result;
	}

	public static PoolKeepItemListHandle<Steam.Ugc.Query.Result> GetResults(this SteamUGCQueryCompleted_t queryCompleted, bool returnMetaData = false, bool returnChildren = false, bool returnKeyValueTags = false, bool returnAdditionalPreviews = false, uint startIndex = 0u)
	{
		PoolKeepItemListHandle<Steam.Ugc.Query.Result> poolKeepItemListHandle = Pools.UseKeepItemList<Steam.Ugc.Query.Result>();
		for (uint num = 0u; num < queryCompleted.m_unNumResultsReturned; num++)
		{
			poolKeepItemListHandle.Add(queryCompleted.GetResultAtIndex(num, returnMetaData, returnChildren, returnKeyValueTags, returnAdditionalPreviews, startIndex));
		}
		return poolKeepItemListHandle;
	}

	public static PoolKeepItemListHandle<Steam.Ugc.Query.Result> GetResults(this SteamUGCQueryCompleted_t queryCompleted, ref Steam.Ugc.QuerySettings settings)
	{
		PoolKeepItemListHandle<Steam.Ugc.Query.Result> results = queryCompleted.GetResults(settings.returnMetaData, settings.returnChildren, settings.returnKeyValueTags, settings.returnAdditionalPreviews, settings.startIndex);
		settings.startIndex += queryCompleted.m_unNumResultsReturned;
		return results;
	}

	public static void ReleaseHandle(ref SteamUGCQueryCompleted_t result)
	{
		if (result.m_eResult != 0 && !(result.m_handle == UGCQueryHandle_t.Invalid))
		{
			SteamUGC.ReleaseQueryUGCRequest(result.m_handle);
			result.m_handle = UGCQueryHandle_t.Invalid;
		}
	}

	public static bool IsValid(this PublishedFileId_t publishedFileId)
	{
		return publishedFileId != PublishedFileId_t.Invalid;
	}

	public static async Task<bool> SetChildrenAsync(this PublishedFileId_t publishedFileId, PublishedFileId_t[] children)
	{
		using Steam.Ugc.QuerySpecific querySpecific = Steam.Ugc.QuerySpecific.Create(publishedFileId, null, null, matchAnyTag: true, returnMetaData: false, returnChildren: true);
		PoolKeepItemListHandle<Steam.Ugc.Query.Result> poolKeepItemListHandle = await querySpecific.NextPageAsync();
		if (!poolKeepItemListHandle || poolKeepItemListHandle.Count == 0)
		{
			return false;
		}
		PoolKeepItemHashSetHandle<PublishedFileId_t> existingChildren = Pools.UseKeepItemHashSet(poolKeepItemListHandle.AsEnumerable().First().children.AsEnumerable());
		try
		{
			PoolKeepItemHashSetHandle<PublishedFileId_t> childrenToSet = Pools.UseKeepItemHashSet(children.AsEnumerable());
			try
			{
				if ((await (from child in existingChildren.AsEnumerable()
					where !childrenToSet.Contains(child)
					select SteamUGC.RemoveDependency(publishedFileId, child).ResultAsync<RemoveUGCDependencyResult_t>())).Any((Steam.CallResultData<RemoveUGCDependencyResult_t> result) => result.result.m_eResult.Failure()))
				{
					return false;
				}
				if ((await (from child in childrenToSet.AsEnumerable()
					where child.IsValid() && !existingChildren.Contains(child)
					select SteamUGC.AddDependency(publishedFileId, child).ResultAsync<AddUGCDependencyResult_t>())).Any((Steam.CallResultData<AddUGCDependencyResult_t> result) => result.result.m_eResult.Failure()))
				{
					return false;
				}
			}
			finally
			{
				if (childrenToSet != null)
				{
					((IDisposable)childrenToSet).Dispose();
				}
			}
		}
		finally
		{
			if (existingChildren != null)
			{
				((IDisposable)existingChildren).Dispose();
			}
		}
		return true;
	}

	public static async Task<TreeNode<PublishedFileId_t>> GetDependencyTreeAsync(this PublishedFileId_t publishedFileId, TreeNode<PublishedFileId_t> dependencyTreeNode = null)
	{
		dependencyTreeNode = dependencyTreeNode ?? new TreeNode<PublishedFileId_t>(publishedFileId);
		using (Steam.Ugc.QuerySpecific query = Steam.Ugc.QuerySpecific.Create(publishedFileId, null, null, matchAnyTag: true, returnMetaData: false, returnChildren: true))
		{
			PoolKeepItemListHandle<Steam.Ugc.Query.Result> poolKeepItemListHandle = await query.GetPageAsync(1);
			if (!poolKeepItemListHandle || poolKeepItemListHandle.Count == 0)
			{
				return null;
			}
			await (from dependency in poolKeepItemListHandle.AsEnumerable().First().children
				where !dependencyTreeNode.ContainsParent(dependency)
				select dependency.GetDependencyTreeAsync(dependencyTreeNode.AddChild(dependency)));
		}
		return dependencyTreeNode;
	}

	public static Steam.Ugc.UserItemVoteResult ResultType(this GetUserItemVoteResult_t result)
	{
		if (!result.m_eResult.Failure())
		{
			if (!result.m_bVotedUp)
			{
				if (!result.m_bVotedDown)
				{
					return Steam.Ugc.UserItemVoteResult.None;
				}
				return Steam.Ugc.UserItemVoteResult.DownVote;
			}
			return Steam.Ugc.UserItemVoteResult.UpVote;
		}
		return Steam.Ugc.UserItemVoteResult.Failure;
	}

	public static bool? CurrentVote(this Steam.Ugc.UserItemVoteResult result)
	{
		if ((int)result > 1)
		{
			return result == Steam.Ugc.UserItemVoteResult.UpVote;
		}
		return null;
	}

	public static EUGCQuery Type(this Steam.Ugc.Query.QueryType queryType)
	{
		switch (queryType)
		{
		case Steam.Ugc.Query.QueryType.Trending:
			return EUGCQuery.k_EUGCQuery_RankedByTrend;
		case Steam.Ugc.Query.QueryType.CreatedByFriends:
			return EUGCQuery.k_EUGCQuery_CreatedByFriendsRankedByPublicationDate;
		case Steam.Ugc.Query.QueryType.FriendFavorites:
			return EUGCQuery.k_EUGCQuery_FavoritedByFriendsRankedByPublicationDate;
		case Steam.Ugc.Query.QueryType.Recent:
			return EUGCQuery.k_EUGCQuery_RankedByPublicationDate;
		case Steam.Ugc.Query.QueryType.LikedCreators:
		case Steam.Ugc.Query.QueryType.GroupMembersOnly:
			return EUGCQuery.k_EUGCQuery_RankedByTrend;
		default:
			throw new ArgumentOutOfRangeException("queryType", queryType, null);
		}
	}
}
