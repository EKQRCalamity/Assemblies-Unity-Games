public abstract class IdDeckBase
{
	public delegate void DeckTransferEvent(ATarget card, IdDeckBase newDeck);

	public const float DRAW_WAIT = 0.1f;

	public static event DeckTransferEvent OnDeckTransfer;

	protected static void _SignalDeckTransfer(ATarget card, IdDeckBase newDeck)
	{
		IdDeckBase.OnDeckTransfer?.Invoke(card, newDeck);
	}
}
