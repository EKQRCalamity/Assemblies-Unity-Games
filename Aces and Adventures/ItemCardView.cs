using UnityEngine;

[RequireComponent(typeof(AbilityCardView))]
public class ItemCardView : MonoBehaviour
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/ItemCardView";

	public StringEvent onPurchaseCostChange;

	public BoolEvent onHasPurchaseCostChange;

	public StringEvent onAdventureIndexChange;

	private AbilityCardView _abilityView;

	private ItemCard _item;

	public AbilityCardView abilityView => this.CacheComponent(ref _abilityView);

	public void SetData(ItemCard item)
	{
		_item = item;
		abilityView.onCardBackChange?.Invoke(EnumUtil.GetResourceBlueprint(item.gameState.adventure.data.cardBack).GetComponent<Renderer>().sharedMaterial);
		onPurchaseCostChange?.InvokeLocalized(this, () => _item?.purchaseCostDescription ?? "");
		onHasPurchaseCostChange?.Invoke(_item.purchaseCostDescription.HasVisibleCharacter());
	}
}
