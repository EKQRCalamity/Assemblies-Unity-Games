using System.Collections.Generic;
using System.Threading.Tasks;

public static class IDataContentExtensions
{
	public static string PrepareDataAndGetSaveErrorMessage(this IDataContent content)
	{
		content.PrepareDataForSave();
		return content.GetSaveErrorMessage();
	}

	public static IEnumerable<KeyValuePair<string, string>> GetAdditionalWorkshopTags(this IDataContent data)
	{
		yield break;
	}

	public static ContentVisibility? ForcedVisibility(this IDataContent data)
	{
		return null;
	}

	public static async Task<PoolKeepItemListHandle<string>> GenerateAdditionalPreviewsAsync(this IDataContent data, ContentRef contentRef)
	{
		return Pools.UseKeepItemList<string>();
	}

	public static IEnumerable<Couple<string, ContentRef>> GetQuickDependencies(this IDataContent data)
	{
		yield break;
	}

	public static string GetWorkshopTrailerVideoId(this IDataContent data)
	{
		return "";
	}
}
