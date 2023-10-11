public struct CanBeSelectedResult
{
	public readonly AbilityPreventedBy preventedBy;

	public CanBeSelectedResult(AbilityPreventedBy preventedBy)
	{
		this.preventedBy = preventedBy;
	}

	public CanBeSelectedResult Message()
	{
		if ((bool)this)
		{
			return this;
		}
		GameStateView.Instance?.LogError(preventedBy.LocalizeError(), GameStateView.Instance.state?.player?.audio.character.error[preventedBy]);
		return this;
	}

	public static implicit operator bool(CanBeSelectedResult result)
	{
		return result.preventedBy == AbilityPreventedBy.Nothing;
	}

	public static implicit operator AbilityPreventedBy(CanBeSelectedResult result)
	{
		return result.preventedBy;
	}

	public static implicit operator CanBeSelectedResult(AbilityPreventedBy? preventedBy)
	{
		return new CanBeSelectedResult(preventedBy.GetValueOrDefault());
	}
}
