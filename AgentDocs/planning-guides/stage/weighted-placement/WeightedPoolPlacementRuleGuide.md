# Weighted Pool Placement Rule v1 — Chapter 1 Material

Authority: `chapter1.weighted_pool_placement.v1`; coefficients `weighted-pool-coefficients.v1`.

- Logical encounters: 12 (`early4/mid4/late4`), each phase `primary3 + all1`.
- Generation mass: legacy45/new55 after eligibility, raw-weight normalized inside each generation cell.
- Prefix gate: at least four eligible candidates and two purposes, with deterministic backtracking before exposure.
- Event11 is child-only (`weight0`, top-level false); Event16 is `relic_p1` gated at weight60; Event34 follow-up is never a top-level row.
- Required-character, one-shot, exclusions, cooldown and purpose adjacency are fail-closed.
- Current Resources rule remains the legacy compatibility authority until Publish gate.

| # | Event ID | Gen | Band | Weight | Primary | Secondary | Gate |
|---:|---|---|---|---:|---|---|---|
| 1 | `event.act1.random_event.01.name_swallowing_well` | legacy | all | 100 | World | Battle | - |
| 2 | `event.act1.random_event.02.rain_seller` | legacy | mid | 85 | Gold | Battle | - |
| 3 | `event.act1.random_event.03.returning_straw_shoes` | legacy | early | 110 | Recovery | World | - |
| 4 | `event.act1.random_event.04.red_thread_mountain_goat` | legacy | all | 95 | Battle | Route | - |
| 5 | `event.act1.random_event.05.abandoned_miner_supper` | legacy | early | 100 | Recovery | World | - |
| 6 | `event.act1.random_event.06.shrine_eaves_empty_perch` | legacy | early | 100 | Recovery | World | - |
| 7 | `event.act1.random_event.07.shadow_selling_child` | legacy | all | 80 | World | Gold | - |
| 8 | `event.act1.random_event.08.spring_beneath_stone_grave` | legacy | early | 90 | Recovery | World | - |
| 9 | `event.act1.random_event.09.black_cloth_herbalist` | legacy | mid | 85 | Battle | World | - |
| 10 | `event.act1.random_event.10.silent_jangseung` | legacy | late | 75 | Route | World | - |
| 11 | `event.act1.random_event.11.dry_waterwheel` | legacy | late | 0 | World | Route | - |
| 12 | `event.act1.random_event.12.paper_flower_grave` | legacy | all | 100 | World | Recovery | - |
| 13 | `event.act1.random_event.13.empty_bride_palanquin` | legacy | all | 85 | Battle | World | - |
| 14 | `event.act1.random_event.14.stone_laying_hen` | legacy | mid | 80 | Gold | Relic | - |
| 15 | `event.act1.random_event.15.ash_eating_jar` | legacy | late | 65 | World | Battle | - |
| 16 | `event.act1.random_event.16.crying_bell_smithy` | legacy | mid | 60 | Relic | Growth | relic_p1 |
| 17 | `event.act1.random_event.17.three_bowls_on_mountain_path` | legacy | early | 110 | Recovery | World | - |
| 18 | `event.act1.random_event.18.reverse_flowing_stream` | legacy | late | 75 | World | Route | - |
| 19 | `event.act1.random_event.19.faceless_woodcarver` | legacy | all | 75 | World | Character | - |
| 20 | `event.act1.random_event.20.fog_sewing_old_woman` | legacy | all | 90 | Route | World | - |
| 21 | `event.act1.random_event.21.breath_between_water_drops` | new | early | 110 | Growth | World | - |
| 22 | `event.act1.random_event.22.sleeping_hawk_watch` | new | mid | 75 | Growth | Recovery | - |
| 23 | `event.act1.random_event.23.temple_hundred_eight_steps` | new | mid | 75 | Growth | Recovery | - |
| 24 | `event.act1.random_event.24.herb_scent_empty_barracks` | new | early | 105 | Recovery | World | - |
| 25 | `event.act1.random_event.25.hot_spring_beneath_ice` | new | mid | 70 | Recovery | Relic | - |
| 26 | `event.act1.random_event.26.sleepless_waystation` | new | early | 95 | Recovery | Route | - |
| 27 | `event.act1.random_event.27.paper_armor_bandits` | new | all | 85 | Battle | Gold | - |
| 28 | `event.act1.random_event.28.rockfall_scouts` | new | late | 60 | Battle | Route | - |
| 29 | `event.act1.random_event.29.chain_bridge_tollkeepers` | new | mid | 75 | Battle | Gold | - |
| 30 | `event.act1.random_event.30.night_beacon_intruders` | new | all | 80 | Battle | World | - |
| 31 | `event.act1.random_event.31.wounded_mountain_tiger_domain` | new | mid | 65 | Battle | Relic | - |
| 32 | `event.act1.random_event.32.hidden_ledger_salt_cart` | new | mid | 80 | Gold | World | - |
| 33 | `event.act1.random_event.33.false_mountain_rite_offering_box` | new | mid | 70 | Gold | Battle | - |
| 34 | `event.act1.random_event.34.half_vein_map` | new | late | 55 | Gold | Route | - |
| 35 | `event.act1.random_event.35.ownerless_wage_sack` | new | mid | 75 | Gold | Recovery | - |
| 36 | `event.act1.random_event.36.cracked_bronze_mirror` | new | late | 55 | Relic | World | - |
| 37 | `event.act1.random_event.37.nameless_long_sword_in_rain` | new | mid | 60 | Relic | Battle | - |
| 38 | `event.act1.random_event.38.three_cups_of_moonlight` | new | mid | 65 | Relic | Recovery | - |
| 39 | `event.act1.random_event.39.self_knotting_rope` | new | late | 55 | Relic | Route | - |
| 40 | `event.act1.random_event.40.jihan_empty_medicine_folio` | new | mid | 70 | Character | Recovery | - |
| 41 | `event.act1.random_event.41.yujin_broken_arrow_fletching` | new | mid | 65 | Character | Battle | - |
| 42 | `event.act1.random_event.42.twice_ringing_mountain_echo` | new | late | 60 | Route | World | - |
| 43 | `event.act1.random_event.43.reverse_growing_moss_marker` | new | late | 60 | Route | Relic | - |
| 44 | `event.act1.random_event.44.buried_tax_stele` | new | late | 60 | World | Gold | - |
| 45 | `event.act1.random_event.45.false_wildfire_boundary_stones` | new | late | 55 | World | Route | - |
| 46 | `event.act1.random_event.46.funeral_without_black_cloth` | new | all | 80 | World | Battle | - |
