public class GameStepPresentBonus : AGameStepPresentResult
{
	public BonusCard bonus => _card as BonusCard;

	protected override bool _shouldHaveOffset => bonus.isNew;

	protected override ProjectileMediaPack _clickMedia => bonus.rank.GetBonusMedia();

	public GameStepPresentBonus(BonusCard bonus)
		: base(bonus)
	{
	}
}
