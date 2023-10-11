using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureDeckView : ADeckView
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/Decks/AdventureDeckView";

	public static readonly ResourceBlueprint<GameObject> StickerBlueprint = "GameState/Decks/AdventureDeckSticker";

	private static Dictionary<AdventureDeckSticker, Sprite> _StickerSprites;

	public RectTransform bestCompletionClassContainer;

	public RectTransform bestCompletionRankContainer;

	public RectTransform classCompletionContainer;

	public RectTransform bestCompletionRankEdgeContainer;

	public RectTransform tooltipCreator;

	public GameObject additionalTooltipCreator;

	public static Dictionary<AdventureDeckSticker, Sprite> StickerSprites => _StickerSprites ?? (_StickerSprites = ReflectionUtil.CreateEnumResourceMap<AdventureDeckSticker, Sprite>("GameState/Adventure/AdventureDeckSticker"));

	public AdventureDeck adventureDeck
	{
		get
		{
			return base.target as AdventureDeck;
		}
		set
		{
			base.target = value;
		}
	}

	private void _AddSticker(AdventureDeckSticker stickerType, RectTransform container)
	{
		if ((bool)container)
		{
			Object.Instantiate(StickerBlueprint.value, container, worldPositionStays: false).GetComponentInChildren<Image>().sprite = StickerSprites[stickerType];
		}
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		base._OnTargetChange(oldTarget, newTarget);
		if (adventureDeck == null)
		{
			return;
		}
		onMaterialChange?.Invoke(EnumUtil.GetResourceBlueprint(adventureDeck.adventureDataRef.data.deck).GetComponent<Renderer>().sharedMaterial);
		AdventureCompletion completion = ProfileManager.progress.games.read.GetCompletion(base.target.gameState.game, adventureDeck.adventureDataRef);
		if (completion == null)
		{
			return;
		}
		KeyValuePair<PlayerClass, AdventureCompletionRank>? bestCompletionRank = completion.GetBestCompletionRank(adventureDeck.adventureDataRef.data);
		if (!bestCompletionRank.HasValue)
		{
			return;
		}
		KeyValuePair<PlayerClass, AdventureCompletionRank> valueOrDefault = bestCompletionRank.GetValueOrDefault();
		_AddSticker(valueOrDefault.Key.ToAdventureDeckSticker(), bestCompletionClassContainer);
		_AddSticker(valueOrDefault.Value.ToAdventureDeckSticker(), bestCompletionRankContainer);
		_AddSticker(valueOrDefault.Value.ToAdventureDeckSticker(), bestCompletionRankEdgeContainer);
		foreach (PlayerClass completedWithClass in completion.GetCompletedWithClasses())
		{
			if (completedWithClass != valueOrDefault.Key)
			{
				_AddSticker(completedWithClass.ToAdventureDeckSticker(), classCompletionContainer);
			}
		}
	}
}
