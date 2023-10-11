using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[UICategorySort(CategorySortType.Alphabetical)]
[ResourceEnum("Gameplay/Ability/Media/Particles/AbilityTileSFX/", true)]
public enum AbilityTileSFXType : ushort
{
	[UICategory("Energy")]
	Darkness,
	[UICategory("Misc Effects")]
	DiscoBall,
	[UICategory("Misc Effects")]
	DiceRainD10D12,
	[UICategory("Misc Effects")]
	DiceRainD6,
	[UICategory("Beast")]
	BiteWolf,
	[UICategory("Tech")]
	BlackHole,
	[UICategory("Magic Effects")]
	BookDownAndOpenPositive,
	[UICategory("Medieval")]
	BootStomp,
	[UICategory("Magic Effects")]
	BookDownAndOpenNegative,
	[UICategory("Beast")]
	BiteNibble,
	[UICategory("Beast")]
	BiteShark,
	[UICategory("Medieval")]
	AxesDouble,
	[UICategory("Tech")]
	BinaryUp,
	[UICategory("Elemental")]
	Blaze,
	[UICategory("Elemental")]
	Blizzard,
	[UICategory("Medieval")]
	BootsMarch,
	[UICategory("Object Drops")]
	BoulderDrop,
	[UICategory("Ranged")]
	BowDraw,
	[UICategory("Ranged")]
	BowFire,
	[UICategory("Elemental")]
	AirQuick,
	[UICategory("Misc Effects")]
	Bubbles,
	[UICategory("Buff Debuff Misc")]
	BuffSingleArrowUpWithCircles,
	[UICategory("Ranged")]
	BulletRain,
	[UICategory("Ranged")]
	BullseyeBillboarded,
	[UICategory("Ranged")]
	BullseyeDown,
	[UICategory("Object Drops")]
	CageDrop,
	[UICategory("Traps")]
	Caltrops,
	[UICategory("Defensive")]
	ArmorChestplateDeflectsArrows,
	[UICategory("Tech")]
	CameraFocus,
	[UICategory("Tech")]
	CameraPaparazzi,
	[UICategory("Magic Effects")]
	CandlesFloating,
	[UICategory("Magic Effects")]
	CandlesFloatingBlue,
	[UICategory("Misc Effects")]
	CardSuitFlashColorful,
	[UICategory("Misc Effects")]
	CardSuitFlashRB,
	[UICategory("Misc Effects")]
	CardSuitsUpColorful,
	[UICategory("Misc Effects")]
	CardSuitsUpRB,
	[UICategory("Physical")]
	Choke,
	[UICategory("Beast")]
	Claw3,
	[UICategory("Beast")]
	Claw4,
	[UICategory("Comical")]
	Coffee,
	[UICategory("Tech")]
	CubesColors,
	[UICategory("Tech")]
	CubesTech,
	[UICategory("Energy")]
	DarkTornado,
	[UICategory("Explosions")]
	ExplosionFireball,
	[UICategory("Physical")]
	Backstab,
	[UICategory("Beast")]
	BiteMonster,
	[UICategory("Tech")]
	Bacteria,
	[UICategory("Defensive")]
	ArmorUpChestplate,
	[UICategory("Magic Effects")]
	ArcaneTrisDarkUp,
	[UICategory("Magic Effects")]
	ArcaneDiamondsUp,
	[UICategory("Magic Effects")]
	ArcaneCirclesOutAndUp,
	[UICategory("Ranged")]
	ArrowRain,
	[UICategory("Magic Effects")]
	ArcaneDiamondsOutAndUp,
	[UICategory("Beast")]
	BeesUp,
	[UICategory("Beast")]
	BeesSwarm,
	[UICategory("Object Drops")]
	AnvilDrop,
	[UICategory("Magic Effects")]
	ArcaneDiamondsBurstSphere,
	[UICategory("Magic Effects")]
	ArcaneCircleUp,
	[UICategory("Explosions")]
	ExplosionsTriple,
	[UICategory("Magic Effects")]
	FairiesUp,
	[UICategory("Elemental")]
	FireCircle,
	[UICategory("Comical")]
	GasCartoony,
	[UICategory("Tech")]
	GraphBarsModAnimated,
	[UICategory("Tech")]
	Fireworks,
	[UICategory("Elemental")]
	Hail,
	[UICategory("Buff Debuff Misc")]
	FalconUp,
	[UICategory("Elemental")]
	FirePixel,
	[UICategory("Explosions")]
	ExplosionsMulti,
	[UICategory("Comical")]
	FistSidewaysWords,
	[UICategory("Beast")]
	EyesSpooky,
	[UICategory("Tech")]
	DNAUp,
	[UICategory("Tech")]
	GraphBarsModAnimatedBillboarded,
	[UICategory("Energy")]
	GravCrush,
	[UICategory("Comical")]
	HammerBonk,
	[UICategory("Energy")]
	GravLevitate,
	[UICategory("Healing")]
	HealBasicTech,
	[UICategory("Energy")]
	Eclipse,
	[UICategory("Explosions")]
	ExplosionForce,
	[UICategory("Elemental")]
	FlameColumn,
	[UICategory("Explosions")]
	ExplosionStandard,
	[UICategory("Physical")]
	FistDown,
	[UICategory("Comical")]
	Gas,
	[UICategory("Comical")]
	DonutSnare,
	[UICategory("Comical")]
	DonutDunk,
	[UICategory("Explosions")]
	ExplosionDistort,
	[UICategory("Tech")]
	DrillOutOfGround,
	[UICategory("Explosions")]
	ExplosionAtomic,
	[UICategory("Tech")]
	DrillInAir,
	[UICategory("Comical")]
	DonutShake,
	[UICategory("Explosions")]
	ExplosionPixel,
	[UICategory("Elemental")]
	ElectricBurst,
	[UICategory("Misc Effects")]
	HeartBurst,
	[UICategory("Beast")]
	BiteBloodDrinker,
	[UICategory("Misc Effects")]
	Heartbreak,
	[UICategory("Misc Effects")]
	HeartsAndKiss,
	[UICategory("Medieval")]
	HelmetMedieval,
	[UICategory("Medieval")]
	HelmetMedievalDoubleCollide,
	[UICategory("Medieval")]
	HelmetMedievalGlare,
	[UICategory("Object Drops")]
	IcebergDROP,
	[UICategory("Elemental")]
	IcebergUP,
	[UICategory("Healing")]
	HealRingsAndCrosses,
	[UICategory("Healing")]
	HealStarDown,
	[UICategory("Healing")]
	HealTwinkleDownStars,
	[UICategory("Elemental")]
	IcicleBurst,
	[UICategory("Elemental")]
	IciclesDown,
	[UICategory("Elemental")]
	IciclesGround,
	[UICategory("Physical")]
	TwoBladeStrokes,
	[UICategory("Elemental")]
	IcicleSmash,
	[UICategory("Buff Debuff Misc")]
	IntBrain,
	[UICategory("Tech")]
	Invader,
	[UICategory("Japanese")]
	JapaneseBellShinto,
	[UICategory("Japanese")]
	JapaneseDenDenDaiko,
	[UICategory("Japanese")]
	JapaneseDragonColumn,
	[UICategory("Japanese")]
	JapaneseDragonColumnUp,
	[UICategory("Japanese")]
	JapaneseFanFlutter,
	[UICategory("Japanese")]
	JapaneseKatanaGleam,
	[UICategory("Japanese")]
	JapaneseKatanaTsuba,
	[UICategory("Japanese")]
	JapaneseMaskKabuki,
	[UICategory("Japanese")]
	JapaneseSakuraWind,
	[UICategory("Japanese")]
	JapaneseShurikenSpinUP,
	[UICategory("Japanese")]
	JapaneseTalisman1,
	[UICategory("Japanese")]
	JapaneseTalisman2,
	[UICategory("Japanese")]
	JapaneseWindChimes,
	[UICategory("Japanese")]
	JapaneseYinYang,
	[UICategory("Japanese")]
	JapaneseYinYangCastCircle,
	[UICategory("Ranged")]
	LaserWar,
	[UICategory("Elemental")]
	LavaBurst,
	[UICategory("Elemental")]
	LavaGroundSmall,
	[UICategory("Elemental")]
	LightningBasic,
	[UICategory("Elemental")]
	LightningCartoonyBlue,
	[UICategory("Elemental")]
	LightningCartoonyYellow,
	[UICategory("Elemental")]
	LightningOrb,
	[UICategory("Elemental")]
	LightningPixels,
	[UICategory("Tech")]
	MatrixCode,
	[UICategory("Elemental")]
	Meteor,
	[UICategory("Ranged")]
	MissileSalvo,
	[UICategory("Ranged")]
	MissileStrike,
	[UICategory("Tech")]
	Molecules,
	[UICategory("Energy")]
	MoonBeam,
	[UICategory("Beast")]
	DodgeFeathers,
	[UICategory("Object Drops")]
	GelCubeDrop,
	[UICategory("Object Drops")]
	CheeseBlockDrop,
	[UICategory("Comical")]
	GelCubePopup,
	[UICategory("Ranged")]
	ArrowStrike,
	[UICategory("Beast")]
	DodgeCat1,
	[UICategory("Tech")]
	Paradrop,
	[UICategory("Comical")]
	PartyBall,
	[UICategory("Misc Effects")]
	PerformMusicNotes,
	[UICategory("Physical")]
	PoisonUpGreen,
	[UICategory("Physical")]
	PoisonUpPurple,
	[UICategory("Buff Debuff Misc")]
	PowerUpAuraDragonStyle,
	[UICategory("Energy")]
	Prism,
	[UICategory("Physical")]
	Pummel,
	[UICategory("Energy")]
	PyramidLight,
	[UICategory("Elemental")]
	Quake,
	[UICategory("Elemental")]
	Rain,
	[UICategory("Comical")]
	RainbowRainCartoony,
	[UICategory("Tech")]
	RepairWrenchAndGears,
	[UICategory("Tech")]
	ReticleSpinner3Piece,
	[UICategory("Ranged")]
	RevolverFireSingleSpin,
	[UICategory("Ranged")]
	RevolverReload,
	[UICategory("Ranged")]
	RevolverReloadSpedUp,
	[UICategory("Ranged")]
	RevolversDown2,
	[UICategory("Healing")]
	Rez,
	[UICategory("Elemental")]
	RockBurstGround,
	[UICategory("Elemental")]
	Rockslide,
	[UICategory("Magic Effects")]
	RuneBurstSimpleColorful,
	[UICategory("Magic Effects")]
	RuneBurstSimpleDark,
	[UICategory("Magic Effects")]
	RunesCircle4Up,
	[UICategory("Magic Effects")]
	RunesCircleAndCylinder,
	[UICategory("Magic Effects")]
	RunesCylinderBurstRed,
	[UICategory("Magic Effects")]
	RunesFlash4Up,
	[UICategory("Magic Effects")]
	RunesFlash8Up,
	[UICategory("Magic Effects")]
	RunesSphere,
	[UICategory("Tech")]
	SatelliteCannon,
	[UICategory("Traps")]
	SawCircular,
	[UICategory("Tech")]
	Scanner,
	[UICategory("Defensive")]
	ShieldForce,
	[UICategory("Defensive")]
	ShieldHoneycomb,
	[UICategory("Defensive")]
	ShieldPhysical3,
	[UICategory("Defensive")]
	ShieldPhysicalForward,
	[UICategory("Defensive")]
	ShieldStone,
	[UICategory("Buff Debuff Misc")]
	ShieldBreakWithAxe,
	[UICategory("Defensive")]
	ShieldsPhysicalSpinUP,
	[UICategory("Ranged")]
	ShurikenStorm,
	[UICategory("Energy")]
	SkullCackle,
	[UICategory("Energy")]
	SkullDarkSmokeVomit,
	[UICategory("Energy")]
	SkullUpDark,
	[UICategory("Comical")]
	SleepSheep,
	[UICategory("Physical")]
	SlimeDrops,
	[UICategory("Japanese")]
	SmokeBomb,
	[UICategory("Elemental")]
	SnareVines,
	[UICategory("Elemental")]
	Snow,
	[UICategory("Elemental")]
	SnowBlast,
	[UICategory("Energy")]
	SolarSystem,
	[UICategory("Magic Effects")]
	SparkleCylinderBurst,
	[UICategory("Medieval")]
	SpeedUPWingedSandal,
	[UICategory("Traps")]
	SpikeTrap,
	[UICategory("Elemental")]
	StalactiteDrop,
	[UICategory("Elemental")]
	StalagmiteGrow,
	[UICategory("Tech")]
	StarshipBombingRun,
	[UICategory("Buff Debuff Misc")]
	StarsUpBurst,
	[UICategory("Elemental")]
	Storm,
	[UICategory("Buff Debuff Misc")]
	StrengthBiceps,
	[UICategory("Physical")]
	StrikeArc,
	[UICategory("Physical")]
	StrikeBlunt,
	[UICategory("Physical")]
	StrikeChop,
	[UICategory("Physical")]
	StrikePierce,
	[UICategory("Physical")]
	StrikePierceBillboarded,
	[UICategory("Physical")]
	StrikeShort,
	[UICategory("Physical")]
	StrikeSlash,
	[UICategory("Physical")]
	StrikeSwish,
	[UICategory("Medieval")]
	SwordAndShield,
	[UICategory("Medieval")]
	SwordCircleSpin,
	[UICategory("Medieval")]
	SwordUpSlashUnFinished,
	[UICategory("Medieval")]
	SwordsCrossBlades,
	[UICategory("Medieval")]
	SwordsCrossBladesBillboarded,
	[UICategory("Medieval")]
	SwordsFlyDown,
	[UICategory("Medieval")]
	SwordSingleFlashUp,
	[UICategory("Beast")]
	TentacleAttack,
	[UICategory("Buff Debuff Misc")]
	TimeHaste,
	[UICategory("Buff Debuff Misc")]
	TimeHasteReposition,
	[UICategory("Buff Debuff Misc")]
	TimeSlow,
	[UICategory("Buff Debuff Misc")]
	TimeSlowReposition,
	[UICategory("Object Drops")]
	TonWeightDrop,
	[UICategory("Elemental")]
	Tornado,
	[UICategory("Object Drops")]
	TotemDrop,
	[UICategory("Traps")]
	TrapBear,
	[UICategory("Traps")]
	TrapMouse,
	[UICategory("Elemental")]
	TreeGrowth,
	[UICategory("Tech")]
	UFOTractor,
	[UICategory("Comical")]
	UnicornRainbows,
	[UICategory("Energy")]
	VoodooDoll,
	[UICategory("Elemental")]
	WaterDrown,
	[UICategory("Elemental")]
	WaterJetUp,
	[UICategory("Elemental")]
	WindBlastLeavesBottomUp,
	[UICategory("Elemental")]
	WindBlastLeavesSideways,
	[UICategory("Beast")]
	WingsUp,
	[UICategory("Energy")]
	HolyBoltDown,
	[UICategory("Comical")]
	LickSlapAndSlide,
	[UICategory("Physical")]
	StrikeSlashX,
	[UICategory("Comical")]
	Lick,
	[UICategory("Buff Debuff Misc")]
	GobbletPour,
	[UICategory("Physical")]
	StrikeShieldBash,
	[UICategory("Physical")]
	StrikeSlap,
	[UICategory("Beast")]
	BiteWolfDouble,
	[UICategory("Object Drops")]
	TrophyDrop,
	[UICategory("Healing")]
	HealthPotion,
	[UICategory("Beast")]
	BiteHumanoid,
	[UICategory("Magic Effects")]
	LightSpiralDown,
	[UICategory("Healing")]
	HealMagical,
	[UICategory("Healing")]
	HealFromRE,
	[UICategory("Healing")]
	HealCrossesUpTech,
	[UICategory("Physical")]
	StrikeSlashBig,
	[UICategory("Magic Effects")]
	SummonBones,
	[UICategory("Physical")]
	BoneSwing,
	[UICategory("Physical")]
	BoneShrapnel,
	[UICategory("Magic Effects")]
	DarkRunes8SectionsBillboard,
	[UICategory("Magic Effects")]
	DoomPendulum,
	[UICategory("Magic Effects")]
	StarBlast,
	[UICategory("Magic Effects")]
	RingSphere,
	[UICategory("Energy")]
	ChaosColorRaysUp,
	[UICategory("Physical")]
	StrikePierceDouble,
	[UICategory("Traps")]
	TrapMouseSet,
	[UICategory("Physical")]
	StrikeCircularVerticalOLD,
	[UICategory("Physical")]
	StrikeCircularVertical,
	[UICategory("Magic Effects")]
	RingsDown,
	[UICategory("Comical")]
	GooDa,
	[UICategory("Comical")]
	SayCheese,
	[UICategory("Comical")]
	TentacleCheeseAttack,
	[UICategory("Elemental")]
	FrostboltHit,
	[UICategory("Elemental")]
	FireboltHit,
	[UICategory("Traps")]
	TrapBearSet,
	[UICategory("Elemental")]
	FireComponentGround1,
	[UICategory("Defensive")]
	CrystalShield,
	[UICategory("Comical")]
	FriendsForever,
	[UICategory("Tech")]
	ClawGrabSide,
	[UICategory("Ranged")]
	BullseyeDownArrow,
	[UICategory("Buff Debuff Misc")]
	IronMaiden,
	[UICategory("Magic Effects")]
	BookDownLightReading,
	[UICategory("Tech")]
	Math,
	[UICategory("Physical")]
	StrikeTripleLash,
	[UICategory("Physical")]
	LovingLash,
	[UICategory("Defensive")]
	ArmorDown,
	[UICategory("Beast")]
	EggHatchDragon,
	[UICategory("Beast")]
	NibbleHot,
	[UICategory("Buff Debuff Misc")]
	DiceRainCrit,
	[UICategory("Physical")]
	StrikeSlapImpact,
	[UICategory("Buff Debuff Misc")]
	RedFlag,
	[UICategory("Buff Debuff Misc")]
	Snare,
	[UICategory("Misc Effects")]
	NONE,
	[UICategory("Ranged")]
	FireArrow,
	[UICategory("Elemental")]
	VineSpiralUp,
	[UICategory("Elemental")]
	VenusFlyTrap,
	[UICategory("Elemental")]
	FlowerRing,
	[UICategory("Buff Debuff Misc")]
	WildGrowth,
	[UICategory("Physical")]
	BonePopBack,
	[UICategory("Beast")]
	WingsUpFlight,
	[UICategory("Beast")]
	HorseStampede,
	[UICategory("Beast")]
	SnakeBite,
	[UICategory("Elemental")]
	SingleDrop,
	[UICategory("Comical")]
	PinkMist,
	[UICategory("Comical")]
	RainbowInjection,
	[UICategory("Components")]
	ExplosionsMicroComponentColumn,
	[UICategory("Components")]
	LightComponentGround,
	[UICategory("Components")]
	ForceComponentBurst,
	[UICategory("Components")]
	CirclesUpComponentColumn,
	[UICategory("Components")]
	DarkComponentBurst,
	[UICategory("Components")]
	ElectricityComponentColumn,
	[UICategory("Components")]
	WaterComponentBurst,
	[UICategory("Components")]
	HealMagicComponentColumn,
	[UICategory("Components")]
	ArrowRainComponentColumn,
	[UICategory("Components")]
	FireComponentColumn,
	[UICategory("Components")]
	IceComponentGround,
	[UICategory("Components")]
	EarthComponentBurst,
	[UICategory("Components")]
	PoisonComponentGround,
	[UICategory("Components")]
	LaserComponentColumn,
	[UICategory("Components")]
	LightComponentColumn,
	[UICategory("Components")]
	IceComponentBurst,
	[UICategory("Components")]
	EarthComponentGround,
	[UICategory("Components")]
	DarkComponentGround,
	[UICategory("Components")]
	HealMagicComponentBurst,
	[UICategory("Components")]
	IceComponentColumn,
	[UICategory("Components")]
	WaterComponentColumn,
	[UICategory("Components")]
	SpiritComponentBurst,
	[UICategory("Components")]
	SpiritComponentColumn,
	[UICategory("Components")]
	EarthComponentColumn,
	[UICategory("Components")]
	StreaksUpComponentColumn,
	[UICategory("Components")]
	CrossesComponentBurst,
	[UICategory("Components")]
	PoisonComponentBurst,
	[UICategory("Components")]
	ElectricityComponentBurst,
	[UICategory("Components")]
	FireComponentBurst,
	[UICategory("Components")]
	LightComponentBurst,
	[UICategory("Components")]
	SpiritComponentGround,
	[UICategory("Components")]
	ArrowSingleUpComponentColumn,
	[UICategory("Components")]
	WaterComponentGround,
	[UICategory("Components")]
	StarsSmallTwinklingComponenetColumn,
	[UICategory("Components")]
	ElectricityComponentGround,
	[UICategory("Components")]
	ForceComponentColumn,
	[UICategory("Components")]
	FireComponentGround,
	[UICategory("Components")]
	ArrowsDownComponentColumn,
	[UICategory("Components")]
	CrossesComponenetColumn,
	[UICategory("Components")]
	HeartsComponentBurst,
	[UICategory("Components")]
	ArrowsUpComponentColumn,
	[UICategory("Components")]
	DarkComponentColumn,
	[UICategory("Components")]
	RainbowBeamComponentColumn,
	[UICategory("Components")]
	PoisonComponentColumn,
	[UICategory("Components")]
	BulletRainComponentColumn,
	[UICategory("Components")]
	StarsComponentBurst,
	[UICategory("Elemental")]
	WaterCubeUp,
	[UICategory("Elemental")]
	WaterBallDown,
	[UICategory("Magic Effects")]
	SpellSiphonReceive,
	[UICategory("Energy")]
	HypnosisSpiral,
	[UICategory("Magic Effects")]
	SpellSiphon,
	[UICategory("Traps")]
	Vault,
	[UICategory("Buff Debuff Misc")]
	Cajole,
	[UICategory("Beast")]
	PoisonTail,
	[UICategory("Traps")]
	VaultDown,
	[UICategory("Beast")]
	HornsBullTip,
	[UICategory("Beast")]
	HornsBullForward,
	[UICategory("Buff Debuff Misc")]
	GOMAD,
	[UICategory("Buff Debuff Misc")]
	PumpWeights,
	[UICategory("Comical")]
	BubbleWings,
	[UICategory("Magic Effects")]
	BookCloseAndTorch,
	[UICategory("Magic Effects")]
	BookCloseAndIce,
	[UICategory("Magic Effects")]
	BookCloseAndElectric,
	[UICategory("Magic Effects")]
	BookCloseAndNature,
	[UICategory("Ranged")]
	Boomerang,
	[UICategory("Elemental")]
	BlazeBlue,
	[UICategory("Elemental")]
	BlueFlameOut,
	[UICategory("Ranged")]
	SpearStrikeDown,
	[UICategory("Ranged")]
	SpearToss,
	[UICategory("Ranged")]
	BoomerangFromBehind,
	[UICategory("Buff Debuff Misc")]
	BubblesAndGlitterSpiral,
	[UICategory("Defensive")]
	HolyShield,
	[UICategory("Comical")]
	Fizz,
	[UICategory("Comical")]
	WhirlyUp,
	[UICategory("Object Drops")]
	BarrelDrop,
	[UICategory("Energy")]
	SoulWell,
	[UICategory("Buff Debuff Misc")]
	VitruvianSplit,
	[UICategory("Energy")]
	HungeringDarkness,
	[UICategory("Energy")]
	GraspingDead,
	[UICategory("Buff Debuff Misc")]
	DarkBrambles,
	[UICategory("Energy")]
	GraveRise,
	[UICategory("Buff Debuff Misc")]
	DarkPactRunes,
	[UICategory("Energy")]
	NightTerror,
	[UICategory("Tech")]
	RoboFlail,
	[UICategory("Tech")]
	TetherTieUp,
	[UICategory("Tech")]
	BeaconBeam,
	[UICategory("Tech")]
	RoboClamp,
	[UICategory("Comical")]
	Angry,
	[UICategory("Comical")]
	ShockAndQuestion,
	[UICategory("Tech")]
	KineticDrive,
	[UICategory("Misc Effects")]
	DustOut,
	[UICategory("Physical")]
	ToxicUp,
	[UICategory("Physical")]
	ToxicBurst,
	[UICategory("Physical")]
	ToxinAlluringUp,
	[UICategory("Comical")]
	StrawSip,
	[UICategory("Comical")]
	SnipNSip,
	[UICategory("Physical")]
	SnipNSnap,
	[UICategory("Physical")]
	Snip,
	[UICategory("Buff Debuff Misc")]
	EnergyOrbsReceive,
	[UICategory("Physical")]
	StrikePunchMetal,
	[UICategory("Defensive")]
	ScrapMetalShield,
	[UICategory("Healing")]
	BandageKitty,
	[UICategory("Tech")]
	HammerBuild,
	[UICategory("Magic Effects")]
	SummonMagic,
	[UICategory("Buff Debuff Misc")]
	CorkDagger,
	[UICategory("Energy")]
	Radiation,
	[UICategory("Buff Debuff Misc")]
	RedDrop,
	[UICategory("Components")]
	LaserStrikeComponentBurst,
	[UICategory("Comical")]
	Whisking,
	[UICategory("Ranged")]
	LockNLoad,
	[UICategory("Comical")]
	BellyLove,
	[UICategory("Misc Effects")]
	Veto,
	[UICategory("Comical")]
	Trophy,
	[UICategory("Tech")]
	PowerRingsUp,
	[UICategory("Tech")]
	Magnet,
	[UICategory("Tech")]
	CrateTech,
	[UICategory("Buff Debuff Misc")]
	PowerUpTechWeapons,
	[UICategory("Buff Debuff Misc")]
	PowerUpTechEnergy,
	[UICategory("Buff Debuff Misc")]
	PowerUpTechDefense,
	[UICategory("Tech")]
	SpawnBeam,
	[UICategory("Buff Debuff Misc")]
	WildGrowthVines,
	[UICategory("Healing")]
	HealBees,
	[UICategory("Physical")]
	StrikeBranch,
	[UICategory("Physical")]
	StrikeSting,
	[UICategory("Magic Effects")]
	SeedOfLife,
	[UICategory("Magic Effects")]
	TransformNature,
	[UICategory("Tech")]
	FlareGreen,
	[UICategory("Explosions")]
	ExplosionsShotgun,
	[UICategory("Buff Debuff Misc")]
	DownArrowBig,
	[UICategory("Elemental")]
	Sparks3,
	[UICategory("Comical")]
	BellyBopImpact,
	[UICategory("Comical")]
	OilSlip,
	[UICategory("Elemental")]
	RocksMunch,
	[UICategory("Buff Debuff Misc")]
	RedWhiteAndBlueUp,
	[UICategory("Buff Debuff Misc")]
	WordsBurstGovt,
	[UICategory("Misc Effects")]
	GoldCoinsUp,
	[UICategory("Elemental")]
	AsteroidStrike,
	[UICategory("Buff Debuff Misc")]
	GammaFlash,
	[UICategory("Energy")]
	AnchorBurst,
	[UICategory("Buff Debuff Misc")]
	NeuroHUD,
	[UICategory("Tech")]
	TeleportTech,
	[UICategory("Buff Debuff Misc")]
	TimeDouble,
	[UICategory("Tech")]
	TeleportTechBlue,
	[UICategory("Tech")]
	AmmoBagExplosion,
	[UICategory("Buff Debuff Misc")]
	PermanenceLights,
	[UICategory("Ranged")]
	RevolverSpinBarrel,
	[UICategory("Tech")]
	Hologram,
	[UICategory("Japanese")]
	KatanaSlashBlue,
	[UICategory("Japanese")]
	KatanaWaterSlash,
	[UICategory("Japanese")]
	KatanaSlashRed,
	[UICategory("Japanese")]
	KanaboStrike,
	[UICategory("Japanese")]
	ShimenawaRingUp,
	[UICategory("Beast")]
	BiteSilverFang,
	[UICategory("Physical")]
	StrikePrimeCut,
	[UICategory("Japanese")]
	BambooGrowthSummon,
	[UICategory("Japanese")]
	BambooGrowthBasic,
	[UICategory("Japanese")]
	KatanaSlashSakura,
	[UICategory("Comical")]
	Cute,
	[UICategory("Japanese")]
	BambooQuake,
	[UICategory("Buff Debuff Misc")]
	ChickenCrazy,
	[UICategory("Beast")]
	EggHatchPhoenix,
	[UICategory("Elemental")]
	FoxFire,
	[UICategory("Elemental")]
	CrossWind,
	[UICategory("Medieval")]
	SwordEnchantSpirit,
	[UICategory("Magic Effects")]
	MysticFlamesBlue,
	[UICategory("Misc Effects")]
	MusicalBlast,
	[UICategory("Misc Effects")]
	MusicUpDissonant,
	[UICategory("Japanese")]
	JapaneseMusicUpDark,
	[UICategory("Physical")]
	StrikePunch,
	[UICategory("Physical")]
	StrikeKarateChop,
	[UICategory("Japanese")]
	BambooSliced,
	[UICategory("Japanese")]
	ToriiSummon,
	[UICategory("Japanese")]
	SakuraPurity,
	[UICategory("Japanese")]
	KatanaSlashFlame,
	[UICategory("Energy")]
	RadiantRays,
	[UICategory("Japanese")]
	FireKatanaDown,
	[UICategory("Japanese")]
	SuzuPurify,
	[UICategory("Japanese")]
	OmikujiPapersPositive,
	[UICategory("Physical")]
	Mug,
	[UICategory("Japanese")]
	NunchakuStrikeSingle,
	[UICategory("Japanese")]
	NunchakuStrikeDouble,
	[UICategory("Medieval")]
	WoodenSwordUp,
	[UICategory("Medieval")]
	SplinteringBlow,
	[UICategory("Japanese")]
	CurseKyou,
	[UICategory("Japanese")]
	BlessingKichi,
	[UICategory("Japanese")]
	OmikujiPapersNegative,
	[UICategory("Comical")]
	FishSlap,
	[UICategory("Elemental")]
	Splinters,
	[UICategory("Physical")]
	StrikeSpikedBallHit,
	[UICategory("Japanese")]
	KatanaDown,
	[UICategory("Elemental")]
	WindSwirl,
	[UICategory("Japanese")]
	JapaneseSummonSquares,
	[UICategory("Japanese")]
	JapaneseElementalFire,
	[UICategory("Japanese")]
	JapaneseElementalWater,
	[UICategory("Japanese")]
	JapaneseElementalEarth,
	[UICategory("Japanese")]
	JapaneseElementalAir,
	[UICategory("Japanese")]
	GeomancerFourElement,
	[UICategory("Tech")]
	SparkSpitter,
	[UICategory("Medieval")]
	HolyHammerStrike,
	[UICategory("Energy")]
	RaysHoly,
	[UICategory("Medieval")]
	HolySlash,
	[UICategory("Medieval")]
	ShadowFrostSlash,
	[UICategory("Comical")]
	Popcorn,
	[UICategory("Misc Effects")]
	DiamondUp,
	[UICategory("Elemental")]
	GroundBreak,
	[UICategory("Beast")]
	Spiderwebs,
	[UICategory("Comical")]
	Cryptocurrency,
	[UICategory("Energy")]
	GhostsUp,
	[UICategory("Comical")]
	Maracas,
	[UICategory("Comical")]
	Sombrero,
	[UICategory("Comical")]
	TrumpetTapatio,
	[UICategory("Comical")]
	EyeGouge,
	[UICategory("Medieval")]
	CleaverBlackSlash,
	[UICategory("Energy")]
	ThrilledToDeath,
	[UICategory("Comical")]
	BiteThrill,
	[UICategory("Comical")]
	WormRain,
	[UICategory("Beast")]
	WormNibbles,
	[UICategory("Tech")]
	Chainsaw,
	[UICategory("Medieval")]
	CleaverSlash,
	[UICategory("Magic Effects")]
	CauldronBoil,
	[UICategory("Object Drops")]
	CauldronDrop,
	[UICategory("Comical")]
	CandyCornRain,
	[UICategory("Comical")]
	BiteCandyCorn,
	[UICategory("Magic Effects")]
	TotemDropAir,
	[UICategory("Magic Effects")]
	TotemDropIce,
	[UICategory("Magic Effects")]
	TotemDropGuardian,
	[UICategory("Magic Effects")]
	TotemDropWar,
	[UICategory("Buff Debuff Misc")]
	WolfSpirit,
	[UICategory("Energy")]
	ColorRaysRoyalFlush,
	[UICategory("Misc Effects")]
	CardBluff,
	[UICategory("Comical")]
	FlanUp,
	[UICategory("Object Drops")]
	FlanDrop,
	[UICategory("Comical")]
	ChipsBurst,
	[UICategory("Misc Effects")]
	TreeChop,
	[UICategory("Comical")]
	Sauced,
	[UICategory("Comical")]
	ChocolateCake,
	[UICategory("Object Drops")]
	PotatoMash,
	[UICategory("Comical")]
	TaterSkin,
	[UICategory("Beast")]
	BatBiteBurst,
	[UICategory("Beast")]
	BiteVampire,
	[UICategory("Magic Effects")]
	BookCloseLight,
	[UICategory("Magic Effects")]
	BoneStorm,
	[UICategory("Elemental")]
	BoneFrost,
	[UICategory("Comical")]
	JackolanternHead,
	[UICategory("Comical")]
	JackolanternFace,
	[UICategory("Comical")]
	ChocolateCakeGobble,
	[UICategory("Comical")]
	PumpkinUp,
	[UICategory("Comical")]
	BroomSweep,
	[UICategory("Magic Effects")]
	Glitter,
	[UICategory("Magic Effects")]
	DarkBubbles,
	[UICategory("Tech")]
	MovieBuff,
	[UICategory("Energy")]
	DiscoHaunt,
	[UICategory("Buff Debuff Misc")]
	TourOfDuty,
	[UICategory("Tech")]
	LocomotiveForward,
	[UICategory("Tech")]
	WindUpKey,
	[UICategory("Tech")]
	SteamPuff,
	[UICategory("Tech")]
	SteampunkStampGear,
	[UICategory("Tech")]
	Guillotine,
	[UICategory("Object Drops")]
	TrunkSlam,
	[UICategory("Tech")]
	SteampunkSpears,
	[UICategory("Tech")]
	SteampunkBlades,
	[UICategory("Tech")]
	SteampunkHammer,
	[UICategory("Explosions")]
	BombDrop,
	[UICategory("Tech")]
	ZeppelinBombingRun,
	[UICategory("Tech")]
	SteampunkPlow,
	[UICategory("Tech")]
	PistonPunch,
	[UICategory("Tech")]
	SteampunkPlowDown,
	[UICategory("Tech")]
	WindUpKeyDown,
	[UICategory("Tech")]
	SteampunkOpenCrate,
	[UICategory("Tech")]
	AlarmBell,
	[UICategory("Tech")]
	MagnifiedBeam,
	[UICategory("Tech")]
	Magnify,
	[UICategory("Traps")]
	ManholeBurst,
	[UICategory("Tech")]
	WrenchSteampunk,
	[UICategory("Tech")]
	SteamPipeExhaust,
	[UICategory("Physical")]
	WrenchStrike,
	[UICategory("Tech")]
	JawMetalSnap,
	[UICategory("Tech")]
	SleepingGas,
	[UICategory("Comical")]
	GumballsHose,
	[UICategory("Tech")]
	LightBulbOn,
	[UICategory("Tech")]
	LightBulbFlash,
	[UICategory("Comical")]
	GumballsGround,
	[UICategory("Energy")]
	LightFlares,
	[UICategory("Tech")]
	Winch,
	[UICategory("Tech")]
	ZeppelinUp,
	[UICategory("Object Drops")]
	CardboardBoxDown,
	[UICategory("Misc Effects")]
	SpotlightsExpose,
	[UICategory("Misc Effects")]
	GoldCoinsDown,
	[UICategory("Misc Effects")]
	CardsUpSpiral,
	[UICategory("Comical")]
	LuckyHorseshoe,
	[UICategory("Comical")]
	UnluckyHorseshoe,
	[UICategory("Magic Effects")]
	PendulumHypnosis,
	[UICategory("Misc Effects")]
	ScissorsCut,
	[UICategory("Object Drops")]
	PneumaticTube,
	[UICategory("Comical")]
	Seasoning,
	[UICategory("Comical")]
	Gift,
	[UICategory("Energy")]
	RiftFracture,
	[UICategory("Physical")]
	StrikePoison,
	[UICategory("Ranged")]
	ShotgunShellsUp,
	[UICategory("Medieval")]
	DarkWaterSlash,
	[UICategory("Comical")]
	FastFoodBurger,
	[UICategory("Comical")]
	DiscoFloor,
	[UICategory("Buff Debuff Misc")]
	DJBuffDefense,
	[UICategory("Buff Debuff Misc")]
	DJBuffOffense,
	[UICategory("Misc Effects")]
	LightsRave,
	[UICategory("Comical")]
	BiteChest,
	[UICategory("Medieval")]
	ChestOpen,
	[UICategory("Misc Effects")]
	LightsDance,
	[UICategory("Comical")]
	PizzaPositive,
	[UICategory("Physical")]
	CinderBlockSmash,
	[UICategory("Misc Effects")]
	GraffitiSkull,
	[UICategory("Misc Effects")]
	CoinsImpactLooseChange,
	[UICategory("Misc Effects")]
	SpeakerBoom,
	[UICategory("Comical")]
	SkateboardWhack,
	[UICategory("Physical")]
	ChainHit,
	[UICategory("Tech")]
	WreckingBall,
	[UICategory("Tech")]
	ShipMercuryPew,
	[UICategory("Comical")]
	CookieUp,
	[UICategory("Tech")]
	TireFire,
	[UICategory("Elemental")]
	FourFlames,
	[UICategory("Misc Effects")]
	HotBuns,
	[UICategory("Tech")]
	TireSpringUp,
	[UICategory("Tech")]
	TiresUp,
	[UICategory("Defensive")]
	KevlarUp,
	[UICategory("Buff Debuff Misc")]
	TrafficLightGreen,
	[UICategory("Buff Debuff Misc")]
	TrafficLightRed,
	[UICategory("Buff Debuff Misc")]
	TrafficLightYellow,
	[UICategory("Buff Debuff Misc")]
	ArrowBlinker,
	[UICategory("Defensive")]
	Fence,
	[UICategory("Defensive")]
	ShieldHighway,
	[UICategory("Buff Debuff Misc")]
	TrafficSignNoParking,
	[UICategory("Buff Debuff Misc")]
	TrafficSignYield,
	[UICategory("Buff Debuff Misc")]
	TrafficSignStop,
	[UICategory("Buff Debuff Misc")]
	TicketsReceipts,
	[UICategory("Misc Effects")]
	PizzaCutter,
	[UICategory("Buff Debuff Misc")]
	MedicineCapsulesUp,
	[UICategory("Misc Effects")]
	RecordSlowDown,
	[UICategory("Physical")]
	BaseballBatHit,
	[UICategory("Physical")]
	BowlingPinsStrike,
	[UICategory("Buff Debuff Misc")]
	Sunglasses,
	[UICategory("Physical")]
	HockeySlapShot,
	[UICategory("Comical")]
	ClownHammer,
	[UICategory("Misc Effects")]
	GoldStacks,
	[UICategory("Object Drops")]
	ChessKnightDrop,
	[UICategory("Object Drops")]
	ZDrop,
	[UICategory("Comical")]
	LikeSubComment,
	[UICategory("Tech")]
	GhostShipMirage,
	[UICategory("Physical")]
	BillyClubBonk,
	[UICategory("Misc Effects")]
	AppleBurst,
	[UICategory("Misc Effects")]
	Mirage,
	[UICategory("Elemental")]
	HeatWave,
	[UICategory("Comical")]
	Slip,
	[UICategory("Medieval")]
	ScytheSlash,
	[UICategory("Physical")]
	RedSlice,
	[UICategory("Object Drops")]
	GummyBearDrop,
	[UICategory("Medieval")]
	WildSwing,
	[UICategory("Buff Debuff Misc")]
	SwordEnchantFrost,
	[UICategory("Elemental")]
	RockSmash,
	[UICategory("Buff Debuff Misc")]
	RockThorns,
	[UICategory("Physical")]
	ImpactBash,
	[UICategory("Beast")]
	DeerHeadAttack,
	[UICategory("Beast")]
	ClawDouble,
	[UICategory("Medieval")]
	BigSwordSlash,
	[UICategory("Tech")]
	HammerAndAnvilStrike,
	[UICategory("Misc Effects")]
	SafeClose,
	[UICategory("Object Drops")]
	SafeDrop,
	[UICategory("Comical")]
	TentacleGooAttack,
	[UICategory("Magic Effects")]
	SweetwaterAmuletCast,
	[UICategory("Medieval")]
	FireSlashUp,
	[UICategory("Elemental")]
	MushroomPopsUp,
	[UICategory("Traps")]
	BookTrap,
	[UICategory("Physical")]
	ThreeImpacts,
	[UICategory("Tech")]
	MaceTechStrike,
	[UICategory("Tech")]
	MaceTechStrikeSpiked,
	[UICategory("Tech")]
	GalvanicPiston,
	[UICategory("Misc Effects")]
	BoneDig,
	[UICategory("Medieval")]
	FireSlashArc,
	[UICategory("Comical")]
	IceCreamCone,
	[UICategory("Comical")]
	JackolanternDeathWard,
	[UICategory("Tech")]
	ArcadeControls
}
