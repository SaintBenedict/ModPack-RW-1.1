using Verse;

namespace rjw
{
	public class config : Def
	{
		// TODO: Clean these.
		// Commented out old configs that are no longer in use (or have been converted into settings).

		// Feature Toggles
		//public bool comfort_prisoners_enabled;
		//public bool colonists_can_be_comfort_prisoners;
		//public bool cum_enabled;
		//public bool rape_me_sticky_enabled;
		//public bool sounds_enabled;
		//public bool stds_enabled;
		public bool bondage_gear_enabled;
		//public bool nymph_joiners_enabled;
		//public bool whore_beds_enabled;
		//public bool necro_enabled;
		//public bool random_rape_enabled;
		//public bool always_accept_whores;
		//public bool nymphs_always_JoinInBed;
		//public bool zoophis_always_rape;
		//public bool rapists_always_rape;
		//public bool pawns_always_do_fapping;
		//public bool pawns_always_rapeCP;
		//public bool whores_always_findjob;

		// STD config
		public bool std_show_roll_to_catch;

		public float std_min_severity_to_pitch;
		public float std_env_pitch_cleanliness_exaggeration;
		public float std_env_pitch_dirtiness_exaggeration;
		public float std_outdoor_cleanliness;

		// Age Config
		//public int sex_free_for_all_age;
		//public int sex_minimum_age;

		public float minor_pain_threshold;	        // 0.3
		public float significant_pain_threshold;	// 0.6
		public float extreme_pain_threshold;		// 0.95
		public float base_chance_to_hit_prisoner;	// 50
		public int min_ticks_between_hits;			// 500
		public int max_ticks_between_hits;			// 700

		public float max_nymph_fraction;
		public float opp_inf_initial_immunity;
		public float comfort_prisoner_rape_mtbh_mul;
		//public float whore_mtbh_mul;
		public float nymph_spawn_with_std_mul;
		//public float chance_to_rim;
		
		//public float fertility_endAge_male;
		//public float fertility_endAge_female_humanlike;
		//public float fertility_endAge_female_animal;
	}
}