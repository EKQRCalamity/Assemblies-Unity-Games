using TMPro;
using UnityEngine;

public class AdventureTargetView : ATargetView
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/AdventureTargetView";

	[Header("Adventure")]
	public TextMeshProUGUI descriptionText;

	public StringEvent onNameChange;

	public StringEvent onDescriptionChange;

	public Texture2DEvent onImageChange;

	public RectEvent onImageUVChange;

	public MaterialEvent onCardFrontChange;

	public MaterialEvent onCardBackChange;

	public IAdventureCard adventureCard
	{
		get
		{
			return base.target as IAdventureCard;
		}
		set
		{
			base.target = value.adventureCard;
		}
	}

	public static ATargetView Create(ATarget value, Transform parent = null)
	{
		return Pools.Unpool(((IAdventureCard)value).blueprint, parent).GetComponent<ATargetView>().SetData(value);
	}

	protected virtual void _OnCardChange()
	{
		onNameChange?.InvokeLocalized(this, () => adventureCard?.name);
		onDescriptionChange?.InvokeLocalized(this, () => adventureCard?.description);
		if ((bool)adventureCard.image)
		{
			adventureCard.image.image.GetTexture2D(delegate(Texture2D texture)
			{
				this.InvokeIfAlive(onImageChange, texture);
			});
			onImageUVChange?.Invoke(adventureCard.image);
		}
		onCardBackChange?.Invoke(EnumUtil.GetResourceBlueprint(base.target.gameState.adventure?.data.cardBack ?? AdventureBackType.Seedling).GetComponent<Renderer>().sharedMaterial);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is IAdventureCard)
		{
			_OnCardChange();
		}
	}
}
