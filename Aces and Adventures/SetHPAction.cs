using ProtoBuf;

[ProtoContract]
[UIField("Set HP", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant", order = 3u)]
public class SetHPAction : ACombatantAction
{
	[ProtoContract(EnumPassthru = true)]
	public enum SetHealthType
	{
		DoNotSet,
		Set,
		Increase,
		Decrease
	}

	[ProtoMember(1)]
	[UIField("Set HP To", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _setHP;

	[ProtoMember(2)]
	[UIField(validateOnChange = true)]
	[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 999f)]
	private SetHealthType _setHealth;

	[ProtoMember(3)]
	[UIField]
	[UIHideIf("_hideAllowHealingOverMaxHP")]
	[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 0f, minWidth = 340f)]
	private bool _allowHealingOverMaxHP;

	[ProtoMember(4)]
	[UIField(tooltip = "Skip Heal and Damage logic and set HP value directly.")]
	[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 0f, minWidth = 300f)]
	private bool _directlySetHP;

	private bool _hideAllowHealingOverMaxHP
	{
		get
		{
			if (_setHealth != SetHealthType.Set)
			{
				return _setHealth == SetHealthType.Increase;
			}
			return true;
		}
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context))
		{
			return _setHP.GetValue(context) != (int)context.GetTarget<ACombatant>().HP;
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		int value = _setHP.GetValue(context);
		if (value > (int)combatant.HP)
		{
			if (_setHealth == SetHealthType.Set || (_setHealth == SetHealthType.Increase && value > (int)combatant.stats.health))
			{
				combatant.stats.health.value = value;
			}
			if (_directlySetHP)
			{
				combatant.HP.value = value;
			}
			else
			{
				HealAction.Heal(this, context, value - (int)combatant.HP, _allowHealingOverMaxHP);
			}
		}
		else if (value < (int)combatant.HP)
		{
			if (_directlySetHP)
			{
				combatant.HP.value = value;
			}
			else
			{
				DamageAction.Damage(this, context, (int)combatant.HP - value, true);
			}
			if (_setHealth == SetHealthType.Set || _setHealth == SetHealthType.Decrease)
			{
				combatant.stats.health.value = value;
			}
		}
	}

	protected override string _ToStringUnique()
	{
		return string.Format("Set HP{0} to {1}{2} for", (_setHealth != 0) ? (" (& " + EnumUtil.FriendlyName(_setHealth, uppercase: false) + " Health)").SizeIfNotEmpty() : "", _setHP, (_allowHealingOverMaxHP && !_hideAllowHealingOverMaxHP) ? " (allow over-heal)".SizeIfNotEmpty() : "");
	}
}
