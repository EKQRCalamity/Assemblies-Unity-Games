using System;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.FrameworkCore.Attributes;
using Framework.FrameworkCore.Attributes.Logic;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[Serializable]
public class EntityStats : PersistentInterface
{
	public enum StatsTypes
	{
		AttackSpeed,
		Agility,
		Defense,
		DashCooldown,
		Fervour,
		Life,
		DashRide,
		Resistance,
		Strength,
		Speed,
		FervourStrength,
		Flask,
		BeadSlots,
		CriticalChance,
		CriticalMultiplier,
		DamageMultiplier,
		FlaskHealth,
		Purge,
		MeaCulpa,
		PurgeStrength,
		ParryWindow,
		FireDmgReduction,
		ToxicDmgReduction,
		MagicDmgReduction,
		LightningDmgReduction,
		RangedStrength,
		PrayerDurationAddition,
		PrayerStrengthMultiplier,
		PrayerCostAddition,
		ContactDmgReduction,
		ActiveRiposteWindow,
		AirImpulses,
		NormalDmgReduction
	}

	public class StatsPersistenceData : PersistentManager.PersistentData
	{
		public Dictionary<StatsTypes, float> currentValues = new Dictionary<StatsTypes, float>();

		public Dictionary<StatsTypes, float> permanetBonus = new Dictionary<StatsTypes, float>();

		public StatsPersistenceData()
			: base("ID_STATS")
		{
		}
	}

	[SerializeField]
	private float AttackSpeedBase = 1f;

	[SerializeField]
	private float AttackSpeedMaximum = 1.5f;

	[SerializeField]
	private float AttackSpeedUpgrade = 1f;

	[SerializeField]
	public float LifeBase = 100f;

	[SerializeField]
	public float LifeMaximun = 500f;

	[SerializeField]
	public float LifeUpgrade = 20f;

	[SerializeField]
	private float FervorBase = 40f;

	[SerializeField]
	private float FervorMaxValue = 120f;

	[SerializeField]
	private float FervorUpgrade = 20f;

	[SerializeField]
	private float FervourStrengthBase = 2f;

	[SerializeField]
	private float FervourStrengthUpgrade = 1f;

	[SerializeField]
	private float RangedStrengthBase = 1f;

	[SerializeField]
	private float RangedStrengthUpgrade = 1f;

	[SerializeField]
	private float AgilityBase = 10f;

	[SerializeField]
	private float AgilityUpgrade = 1f;

	[SerializeField]
	private float DefenseBase;

	[SerializeField]
	private float DefenseUpgrade = 1f;

	[SerializeField]
	private float ResistanceBase = 10f;

	[SerializeField]
	private float ResistanceUpgrade = 1f;

	[SerializeField]
	public float StrengthBase = 10f;

	[SerializeField]
	public float StrengthUpgrade = 1f;

	[SerializeField]
	private float SpeedBase = 10f;

	[SerializeField]
	private float SpeedUpgrade = 1f;

	[SerializeField]
	private float FlaskBase = 2f;

	[SerializeField]
	private float FlaskMaximun = 10f;

	[SerializeField]
	private float FlaskUpgrade = 1f;

	[SerializeField]
	private float FlaskHealthBase = 20f;

	[SerializeField]
	public float FlaskHealthUpgrade = 10f;

	[SerializeField]
	private float BeadSlotsBase = 2f;

	[SerializeField]
	private float BeadSlotsUpgrade = 1f;

	[SerializeField]
	private float CriticalChanceBase;

	[SerializeField]
	private float CriticalChanceUpgrade = 1f;

	[SerializeField]
	private float CriticalMultiplierBase = 2f;

	[SerializeField]
	private float CriticalMultiplierUpgrade = 1f;

	[SerializeField]
	private float DamageMultiplierBase = 1f;

	[SerializeField]
	private float DamageMultiplierUpgrade = 1f;

	[SerializeField]
	private float InitialPurge;

	[SerializeField]
	private float PurgeUpgrade = 1f;

	[SerializeField]
	private float InitialMeaCulpa;

	[SerializeField]
	private float MeaCulpaUpgrade = 1f;

	[SerializeField]
	private float PurgeStrengthBase = 1f;

	[SerializeField]
	private float PurgeStrengthUpgrade = 1f;

	[SerializeField]
	private float ParryWindowBase = 0.5f;

	[SerializeField]
	private float ParryWindowUpgrade = 1f;

	[SerializeField]
	private float RiposteWindowBase = 0.15f;

	[SerializeField]
	private float RiposteWindowUpgrade = 1f;

	[SerializeField]
	private float DashCooldownBase = 1f;

	[SerializeField]
	private float DashCooldownUpgrade = 1f;

	[SerializeField]
	private float DashRideBase = 0.35f;

	[SerializeField]
	private float DashRideUpgrade = 1f;

	[SerializeField]
	private float FireDmgReductionBase;

	[SerializeField]
	private float FireDmgReductionUpgrade = 0.25f;

	[SerializeField]
	private float ToxicDmgReductionBase;

	[SerializeField]
	private float ToxicDmgReductionUpgrade = 0.25f;

	[SerializeField]
	private float MagicDmgReductionBase;

	[SerializeField]
	private float MagicDmgReductionUpgrade = 0.25f;

	[SerializeField]
	private float LightningDmgReductionBase;

	[SerializeField]
	private float LightningDmgReductionUpgrade = 0.25f;

	[SerializeField]
	private float ContactDmgReductionBase;

	[SerializeField]
	private float ContactDmgReductionUpgrade = 0.4f;

	[SerializeField]
	private float NormalDmgReductionBase;

	[SerializeField]
	private float NormalDmgReductionUpgrade = 0.4f;

	[SerializeField]
	private float AirImpulsesBase = 2f;

	[SerializeField]
	private float AirImpulsesUpgrade = 1f;

	private const string PERSITENT_ID = "ID_STATS";

	public AttackSpeed AttackSpeed { get; set; }

	public Agility Agility { get; set; }

	public Defense Defense { get; set; }

	public Fervour Fervour { get; set; }

	public FervourStrength FervourStrength { get; set; }

	public Life Life { get; set; }

	public Resistance Resistance { get; set; }

	public Strength Strength { get; set; }

	public RangedStrength RangedStrength { get; set; }

	public Speed Speed { get; set; }

	public Flask Flask { get; set; }

	public FlaskHealth FlaskHealth { get; set; }

	public BeadSlots BeadSlots { get; set; }

	public CriticalChance CriticalChance { get; set; }

	public CriticalMultiplier CriticalMultiplier { get; set; }

	public DamageMultiplier DamageMultiplier { get; set; }

	public Purge Purge { get; set; }

	public MeaCulpa MeaCulpa { get; set; }

	public PurgeStrength PurgeStrength { get; set; }

	public ParryWindow ParryWindow { get; set; }

	public ActiveRiposteWindow ActiveRiposteWindow { get; set; }

	public DashCooldown DashCooldown { get; set; }

	public DashRide DashRide { get; set; }

	public FireDmgReduction FireDmgReduction { get; set; }

	public ToxicDmgReduction ToxicDmgReduction { get; set; }

	public MagicDmgReduction MagicDmgReduction { get; set; }

	public LightningDmgReduction LightningDmgReduction { get; set; }

	public ContactDmgReduction ContactDmgReduction { get; set; }

	public NormalDmgReduction NormalDmgReduction { get; set; }

	public Framework.FrameworkCore.Attributes.Logic.Attribute PrayerDurationAddition { get; set; }

	public Framework.FrameworkCore.Attributes.Logic.Attribute PrayerStrengthMultiplier { get; set; }

	public Framework.FrameworkCore.Attributes.Logic.Attribute PrayerCostAddition { get; set; }

	public Framework.FrameworkCore.Attributes.Logic.Attribute AirImpulses { get; set; }

	public void Initialize()
	{
		AttackSpeed = new AttackSpeed(AttackSpeedBase, AttackSpeedUpgrade, AttackSpeedMaximum, 1f);
		Life = new Life(LifeBase, LifeUpgrade, LifeMaximun, 1f);
		Fervour = new Fervour(FervorBase, FervorUpgrade, FervorMaxValue, 1f);
		FervourStrength = new FervourStrength(FervourStrengthBase, FervourStrengthUpgrade, 1f);
		Agility = new Agility(AgilityBase, AgilityUpgrade, 1f);
		Defense = new Defense(DefenseBase, DefenseUpgrade, 1f);
		Resistance = new Resistance(ResistanceBase, ResistanceUpgrade, 1f);
		Strength = new Strength(StrengthBase, StrengthUpgrade, 1f);
		Speed = new Speed(SpeedBase, SpeedUpgrade, 1f);
		Flask = new Flask(FlaskBase, FlaskUpgrade, FlaskMaximun, 1f);
		BeadSlots = new BeadSlots(BeadSlotsBase, BeadSlotsUpgrade, 1f);
		CriticalChance = new CriticalChance(CriticalChanceBase, CriticalChanceUpgrade, 1f);
		CriticalMultiplier = new CriticalMultiplier(CriticalMultiplierBase, CriticalMultiplierUpgrade, 1f);
		DamageMultiplier = new DamageMultiplier(DamageMultiplierBase, DamageMultiplierUpgrade, 1f);
		FlaskHealth = new FlaskHealth(FlaskHealthBase, FlaskHealthUpgrade, 1f);
		Purge = new Purge(InitialPurge, PurgeUpgrade, 1f);
		MeaCulpa = new MeaCulpa(InitialMeaCulpa, MeaCulpaUpgrade, 1f);
		PurgeStrength = new PurgeStrength(PurgeStrengthBase, PurgeStrengthUpgrade, 1f);
		ParryWindow = new ParryWindow(ParryWindowBase, ParryWindowUpgrade, 1f);
		ActiveRiposteWindow = new ActiveRiposteWindow(RiposteWindowBase, RiposteWindowUpgrade, 1f);
		DashCooldown = new DashCooldown(DashCooldownBase, DashCooldownUpgrade, 1f);
		DashRide = new DashRide(DashRideBase, DashRideUpgrade, 1f);
		FireDmgReduction = new FireDmgReduction(FireDmgReductionBase, FireDmgReductionUpgrade, 1f);
		ToxicDmgReduction = new ToxicDmgReduction(ToxicDmgReductionBase, ToxicDmgReductionUpgrade, 1f);
		MagicDmgReduction = new MagicDmgReduction(MagicDmgReductionBase, MagicDmgReductionUpgrade, 1f);
		LightningDmgReduction = new LightningDmgReduction(LightningDmgReductionBase, LightningDmgReductionUpgrade, 1f);
		ContactDmgReduction = new ContactDmgReduction(ContactDmgReductionBase, ContactDmgReductionUpgrade, 1f);
		NormalDmgReduction = new NormalDmgReduction(NormalDmgReductionBase, NormalDmgReductionUpgrade, 1f);
		RangedStrength = new RangedStrength(RangedStrengthBase, RangedStrengthUpgrade, 1f);
		PrayerDurationAddition = new Framework.FrameworkCore.Attributes.Logic.Attribute(0f, 1f);
		PrayerStrengthMultiplier = new Framework.FrameworkCore.Attributes.Logic.Attribute(1f, 1f);
		PrayerCostAddition = new Framework.FrameworkCore.Attributes.Logic.Attribute(0f, 1f);
		AirImpulses = new AirImpulses(AirImpulsesBase, AirImpulsesUpgrade, 1f);
	}

	public Framework.FrameworkCore.Attributes.Logic.Attribute GetByType(StatsTypes nameType)
	{
		Framework.FrameworkCore.Attributes.Logic.Attribute result = null;
		switch (nameType)
		{
		case StatsTypes.AttackSpeed:
			result = AttackSpeed;
			break;
		case StatsTypes.Agility:
			result = Agility;
			break;
		case StatsTypes.Defense:
			result = Defense;
			break;
		case StatsTypes.Fervour:
			result = Fervour;
			break;
		case StatsTypes.FervourStrength:
			result = FervourStrength;
			break;
		case StatsTypes.Life:
			result = Life;
			break;
		case StatsTypes.DashCooldown:
			result = DashCooldown;
			break;
		case StatsTypes.DashRide:
			result = DashRide;
			break;
		case StatsTypes.Resistance:
			result = Resistance;
			break;
		case StatsTypes.Speed:
			result = Speed;
			break;
		case StatsTypes.Strength:
			result = Strength;
			break;
		case StatsTypes.Flask:
			result = Flask;
			break;
		case StatsTypes.BeadSlots:
			result = BeadSlots;
			break;
		case StatsTypes.CriticalChance:
			result = CriticalChance;
			break;
		case StatsTypes.CriticalMultiplier:
			result = CriticalMultiplier;
			break;
		case StatsTypes.DamageMultiplier:
			result = DamageMultiplier;
			break;
		case StatsTypes.FlaskHealth:
			result = FlaskHealth;
			break;
		case StatsTypes.Purge:
			result = Purge;
			break;
		case StatsTypes.MeaCulpa:
			result = MeaCulpa;
			break;
		case StatsTypes.PurgeStrength:
			result = PurgeStrength;
			break;
		case StatsTypes.ParryWindow:
			result = ParryWindow;
			break;
		case StatsTypes.ActiveRiposteWindow:
			result = ActiveRiposteWindow;
			break;
		case StatsTypes.FireDmgReduction:
			result = FireDmgReduction;
			break;
		case StatsTypes.ToxicDmgReduction:
			result = ToxicDmgReduction;
			break;
		case StatsTypes.MagicDmgReduction:
			result = MagicDmgReduction;
			break;
		case StatsTypes.LightningDmgReduction:
			result = LightningDmgReduction;
			break;
		case StatsTypes.ContactDmgReduction:
			result = ContactDmgReduction;
			break;
		case StatsTypes.NormalDmgReduction:
			result = NormalDmgReduction;
			break;
		case StatsTypes.RangedStrength:
			result = RangedStrength;
			break;
		case StatsTypes.PrayerDurationAddition:
			result = PrayerDurationAddition;
			break;
		case StatsTypes.PrayerStrengthMultiplier:
			result = PrayerStrengthMultiplier;
			break;
		case StatsTypes.PrayerCostAddition:
			result = PrayerCostAddition;
			break;
		case StatsTypes.AirImpulses:
			result = AirImpulses;
			break;
		}
		return result;
	}

	public void ResetAllBonus()
	{
		foreach (StatsTypes value in Enum.GetValues(typeof(StatsTypes)))
		{
			Framework.FrameworkCore.Attributes.Logic.Attribute byType = GetByType(value);
			byType.ResetBonus();
		}
	}

	public string GetPersistenID()
	{
		return "ID_STATS";
	}

	public int GetOrder()
	{
		return 0;
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		StatsPersistenceData statsPersistenceData = new StatsPersistenceData();
		foreach (StatsTypes value in Enum.GetValues(typeof(StatsTypes)))
		{
			Framework.FrameworkCore.Attributes.Logic.Attribute byType = GetByType(value);
			statsPersistenceData.permanetBonus[value] = byType.PermanetBonus;
			if (byType.IsVariable())
			{
				VariableAttribute variableAttribute = (VariableAttribute)byType;
				statsPersistenceData.currentValues[value] = variableAttribute.Current;
			}
		}
		return statsPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		StatsPersistenceData statsPersistenceData = (StatsPersistenceData)data;
		foreach (KeyValuePair<StatsTypes, float> permanetBonu in statsPersistenceData.permanetBonus)
		{
			Framework.FrameworkCore.Attributes.Logic.Attribute byType = GetByType(permanetBonu.Key);
			byType.SetPermanentBonus(permanetBonu.Value);
		}
		foreach (KeyValuePair<StatsTypes, float> currentValue in statsPersistenceData.currentValues)
		{
			Framework.FrameworkCore.Attributes.Logic.Attribute byType2 = GetByType(currentValue.Key);
			if (byType2.IsVariable())
			{
				VariableAttribute variableAttribute = (VariableAttribute)byType2;
				variableAttribute.Current = currentValue.Value;
			}
		}
		if (isloading)
		{
			Fervour.Current = 0f;
			Life.SetToCurrentMax();
			Flask.SetToCurrentMax();
		}
	}

	public void ResetPersistence()
	{
		foreach (StatsTypes value in Enum.GetValues(typeof(StatsTypes)))
		{
			Framework.FrameworkCore.Attributes.Logic.Attribute byType = GetByType(value);
			byType.SetPermanentBonus(0f);
			if (byType.IsVariable())
			{
				VariableAttribute variableAttribute = (VariableAttribute)byType;
				variableAttribute.Current = 0f;
			}
		}
		Life.SetToCurrentMax();
		Flask.SetToCurrentMax();
	}
}
