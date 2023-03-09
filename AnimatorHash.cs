using UnityEngine;

public static class AnimatorHash
{
	public static class level_oldman_man
	{
		public static class Layer
		{
			public static readonly int BaseLayer;

			public static readonly int Beard = 1;

			public static readonly int Eyes = 2;

			public static readonly int GnomeA = 3;

			public static readonly int Cauldron = 4;

			public static readonly int GnomeB = 5;
		}

		public static class ShortHash
		{
			public static readonly int Idle_Part_1 = Animator.StringToHash("Idle_Part_1");

			public static readonly int Idle_Part_2 = Animator.StringToHash("Idle_Part_2");

			public static readonly int Spit_Transition_A = Animator.StringToHash("Spit_Transition_A");

			public static readonly int Spit_Loop = Animator.StringToHash("Spit_Loop");

			public static readonly int Spit_Transition_B = Animator.StringToHash("Spit_Transition_B");

			public static readonly int Spit_Intro_Continued = Animator.StringToHash("Spit_Intro_Continued");

			public static readonly int Spit_Outro = Animator.StringToHash("Spit_Outro");

			public static readonly int Phase_Trans = Animator.StringToHash("Phase_Trans");

			public static readonly int Phase_Trans_Cont = Animator.StringToHash("Phase_Trans_Cont");

			public static readonly int Beard_Boil = Animator.StringToHash("Beard_Boil");

			public static readonly int Blank = Animator.StringToHash("Blank");

			public static readonly int Loop = Animator.StringToHash("Loop");
		}

		public static class FullHash
		{
			public static readonly int BaseLayer_Idle_Part_1 = Animator.StringToHash("Base Layer.Idle_Part_1");

			public static readonly int BaseLayer_Idle_Part_2 = Animator.StringToHash("Base Layer.Idle_Part_2");

			public static readonly int BaseLayer_Spit_Transition_A = Animator.StringToHash("Base Layer.Spit_Transition_A");

			public static readonly int BaseLayer_Spit_Loop = Animator.StringToHash("Base Layer.Spit_Loop");

			public static readonly int BaseLayer_Spit_Transition_B = Animator.StringToHash("Base Layer.Spit_Transition_B");

			public static readonly int BaseLayer_Spit_Intro_Continued = Animator.StringToHash("Base Layer.Spit_Intro_Continued");

			public static readonly int BaseLayer_Spit_Outro = Animator.StringToHash("Base Layer.Spit_Outro");

			public static readonly int BaseLayer_Phase_Trans = Animator.StringToHash("Base Layer.Phase_Trans");

			public static readonly int BaseLayer_Phase_Trans_Cont = Animator.StringToHash("Base Layer.Phase_Trans_Cont");

			public static readonly int Beard_Beard_Boil = Animator.StringToHash("Beard.Beard_Boil");

			public static readonly int Eyes_Blank = Animator.StringToHash("Eyes.Blank");

			public static readonly int Eyes_Spit_Loop = Animator.StringToHash("Eyes.Spit_Loop");

			public static readonly int GnomeA_Loop = Animator.StringToHash("GnomeA.Loop");

			public static readonly int Cauldron_Loop = Animator.StringToHash("Cauldron.Loop");

			public static readonly int GnomeB_Loop = Animator.StringToHash("GnomeB.Loop");
		}

		public static class Parameter
		{
			public static readonly int IsSpitAttack = Animator.StringToHash("IsSpitAttack");

			public static readonly int IsSpitAttackEyeLoop = Animator.StringToHash("IsSpitAttackEyeLoop");

			public static readonly int Phase2 = Animator.StringToHash("Phase2");
		}
	}
}
