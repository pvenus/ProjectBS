# Chapter 1 Random Event Portfolio 48 Guide

## 1. Authority and status

- Canonical path: `AgentDocs/planning-guides/event/Chapter1RandomEventPortfolio48Guide.md`
- Design owner and approver: 벼리 — 게임 기획 디렉터
- Scope owner and gate approver: 이음 — 프로젝트 프로듀서
- Technical owner: 한결 — 프로젝트 개발 리드
- Visual owner: 화감 — 프로젝트 아트 디렉터
- Status: **G0 portfolio design authority; exact1 awaiting acceptance**
- Contract version: `chapter1-random-event-portfolio48.v1`

This document is the single game-design authority for the final 48-event
Chapter 1 random-event portfolio. It fixes portfolio identity, purpose,
trade-off, exposure, batching, and acceptance rules. It does not authorize
runtime, JSON, SO, image, staging, commit, or push work by itself.

The staged `cached116` / `A2-G` Smithy delivery is the first implementation
baseline. It must remain intact and independently verifiable. The 48-event
portfolio must not be implemented, generated, staged, or integrated all at
once.

## 2. Product intent

Each event must have one Primary Purpose and a materially different decision.
Names, prose, and images alone do not constitute variety. Cost, risk, result,
executor, story motif, and alternative outcome must make the event distinct.

The final portfolio contains exactly 48 unique events that can be exposed by
the active pool. Follow-up nodes within an event chain do not increase this
count.

## 3. Portfolio accounting

```text
existing event01-event20                         20
- event11 independent exposure                   -1
+ Smithy growth derivative                        1
= reusable or revisable unique exposure          20
+ new events                                     28
= final active unique portfolio                  48
```

`event11` remains valid source content but is not an independent pool entry.
It becomes a child variant in the event18 water-exploitation chain.

Original event16 may return when the Relic capability passes its P1 gate. It
and the Smithy growth derivative belong to the same-run exclusion group:

```text
motif.exclusive.crying_bell_smithy
```

They must never be exposed in the same run.

## 4. Reusable 20 exact registry

The following 20 IDs are exhaustive. Aliases and display-name-derived IDs are
forbidden.

| # | Exact event ID | Portfolio condition |
|---:|---|---|
| 1 | `event.act1.random_event.01.name_swallowing_well` | Revisable, independent |
| 2 | `event.act1.random_event.02.rain_seller` | Revisable, independent |
| 3 | `event.act1.random_event.03.returning_straw_shoes` | Revisable, independent |
| 4 | `event.act1.random_event.04.red_thread_mountain_goat` | Maintain, independent |
| 5 | `event.act1.random_event.05.abandoned_miner_supper` | Revisable, independent |
| 6 | `event.act1.random_event.06.shrine_eaves_empty_perch` | Maintain original Route/World meaning |
| 7 | `event.act1.random_event.07.shadow_selling_child` | Revisable, independent chain root |
| 8 | `event.act1.random_event.08.spring_beneath_stone_grave` | Revisable, independent |
| 9 | `event.act1.random_event.09.black_cloth_herbalist` | Revisable, independent |
| 10 | `event.act1.random_event.10.silent_jangseung` | Revisable, independent |
| 11 | `event.act1.random_event.12.paper_flower_grave` | Revisable, independent |
| 12 | `event.act1.random_event.13.empty_bride_palanquin` | Maintain original Battle meaning |
| 13 | `event.act1.random_event.14.stone_laying_hen` | Revisable, independent |
| 14 | `event.act1.random_event.15.ash_eating_jar` | Revisable, independent chain root |
| 15 | `event.act1.random_event.16.crying_bell_smithy` | P1 conditional Relic activation; exclusion group |
| 16 | `event.act1.random_event.17.three_bowls_on_mountain_path` | Recovery revision; replacement image required |
| 17 | `event.act1.random_event.18.reverse_flowing_stream` | Maintain; owns event11 child variant |
| 18 | `event.act1.random_event.19.faceless_woodcarver` | Revisable, independent chain root |
| 19 | `event.act1.random_event.20.fog_sewing_old_woman` | Revisable, independent |
| 20 | `event.act1.random_growth.01.crying_bell_smithy_trial` | P0 Growth; exclusion group |

The excluded independent ID is preserved only as an event18 chain child:

```text
event.act1.random_event.11.dry_waterwheel
```

It is not a 49th pool event and must not be selected by the independent-event
selector.

## 5. Final purpose quota and selector weights

| Primary Purpose | Final count | Selector weight |
|---|---:|---:|
| Growth | 6 | 13% |
| Recovery | 6 | 12% |
| Battle | 8 | 17% |
| Gold | 6 | 12% |
| Relic | 6 | 12% |
| Character | 4 | 8% |
| Route | 6 | 13% |
| World | 6 | 13% |
| **Total** | **48** | **100%** |

Chapter 1 uses 12 selected-route logical encounters. Physical graph slots,
off-route projections, and chain children are not additional encounters.

Recommended per-run purpose limits:

| Purpose | Minimum | Maximum |
|---|---:|---:|
| Growth candidate | 2 | 3 |
| Recovery | 1 | 2 |
| Battle | 2 | 4 |
| Gold | 1 | 2 |
| Relic | 0 | 2 |
| Character | 0 | 2 |
| Route | 1 | 2 |
| World | 1 | 2 |

Minimums are placed first. The remaining logical encounters use the selector
weights while respecting maximums, capability gates, exclusions, and cooldowns.

## 6. New 28 canonical production registry

The following 28 rows are exhaustive production authority. Each row fixes the
event ID, Primary/Secondary Purpose, motif, trade-off, intended result/executor,
and production batch.

| # | Exact event ID | Purpose | Motif and event premise | Core choice, cost, and alternative | Intended result / executor | Batch |
|---:|---|---|---|---|---|---|
| 1 | `event.act1.random_growth.02.windworn_sword_marks` | Growth / World | Wind, silver grass, and wooden-sword marks form an old training path. | Observe now for a maximum two-card PartyWide offer, or leave and preserve the Chapter optional-growth claim. | Pending Growth or Declined / `RandomGrowthSafe`, `Decline` | A0 |
| 2 | `event.act1.random_growth.03.cut_signal_rope_ambush` | Growth / Battle | A cut signal rope reveals an ambush route toward the stockade. | Enter battle; victory opens a maximum three-card offer, defeat or abort grants no Growth. | Victory Pending or zero / `RandomGrowthBattle`, `Battle` | A1 |
| 3 | `event.act1.random_event.21.breath_between_water_drops` | Growth / Recovery | A limestone cave's falling water leaves a rhythm of silence between drops. | Endure the cold for a maximum two-card offer, or light a fire for minor Recovery and give up Growth. | Pending Growth or Heal / `RandomGrowthSafe`, `VitalHeal` | B1 |
| 4 | `event.act1.random_event.22.sleeping_hawk_watch` | Growth / Character | A sleeping hawk and dying lamp turn an overnight watch into a lesson in attention. | Keep watch through fatigue for a maximum three-card offer, or withdraw without waking the hawk. | Risk Pending or Declined / `RandomGrowthRisk`, `Decline` | B1 |
| 5 | `event.act1.random_event.23.temple_hundred_eight_steps` | Growth / Recovery | Worn stone steps and a carried rock test breath and endurance before dawn. | Carry the rock to the top for nonlethal HP cost and a maximum three-card offer, or stop for minor Recovery. | Risk Pending or Heal / `RandomGrowthRisk`, `VitalHeal` | B1 |
| 6 | `event.act1.random_event.24.herb_scent_empty_barracks` | Recovery / World | An abandoned barracks still holds medicine bowls, dried herbs, and owner tags. | Use the medicine now for Recovery, or seal it for the families and record a World flag. | Heal or World flag / `VitalHeal`, `WorldFlag` | B1 |
| 7 | `event.act1.random_event.25.hot_spring_beneath_ice` | Recovery / Relic | Steam rises beneath thin ice over blue mineral deposits. | Take safe minor Recovery, or break the ice for greater Recovery and a Relic chance with damage risk. | Heal or Vital-risk Relic claim / `VitalTrade`, `RelicClaim` | B1 |
| 8 | `event.act1.random_event.26.sleepless_waystation` | Recovery / Route | A cold, unlit waystation offers sleep or an exposed night watch. | Sleep for major Recovery and forfeit next-node information, or keep watch for Route information and no Recovery. | Heal or Route reveal / `RestRouteTrade` | B1 |
| 9 | `event.act1.random_event.27.paper_armor_bandits` | Battle / Gold | Rain weakens bandits disguised in layered paper armor. | Wait for lower danger and reduced reward, or fight immediately for normal reward and higher risk. | Two Battle variants / `Battle` | B2 |
| 10 | `event.act1.random_event.28.rockfall_scouts` | Battle / Route | Scouts use a signal mirror above a prepared rockfall. | Ambush them for a victory shortcut, or accept nonlethal rockfall cost to detour. | Battle Route unlock or Vital-cost Route / `Battle`, `RouteOutcome` | B2 |
| 11 | `event.act1.random_event.29.chain_bridge_tollkeepers` | Battle / Gold | Private tollkeepers hold a vertical chain bridge over a gorge. | Pay Gold for safe passage, or fight to preserve Gold while accepting HP and defeat risk. | Gold cost or Battle / `GoldSpend`, `Battle` | B2 |
| 12 | `event.act1.random_event.30.night_beacon_intruders` | Battle / World | A false order is about to light a mountain beacon at night. | Extinguish it through battle, or watch from hiding for a World flag while allowing a future alert risk. | Battle or World flag plus future-risk flag / `Battle`, `WorldFlag` | B2 |
| 13 | `event.act1.random_event.31.wounded_mountain_tiger_domain` | Battle / Relic | A wounded natural tiger stands beside a broken poacher spear. | Fight poachers to free it and gain a Relic clue, or drive it away for safety and a negative relationship flag. | Battle Relic claim or World flag / `Battle`, `RelicClaim`, `WorldFlag` | B2 |
| 14 | `event.act1.random_event.32.hidden_ledger_salt_cart` | Gold / World | Spilled salt and a double ledger expose withheld wages. | Return it for smaller Gold and a positive flag, or sell it for larger Gold and a corruption flag. | Two Gold/flag outcomes / `GoldWorldTrade` | B3 |
| 15 | `event.act1.random_event.33.false_mountain_rite_offering_box` | Gold / Battle | A crude false mountain rite hides a rigged offering box. | Expose it and fight for Gold, quietly sell evidence for smaller Gold, or decline involvement. | Battle Gold, Gold, or Declined / `Battle`, `GoldGrant` | B3 |
| 16 | `event.act1.random_event.34.half_vein_map` | Gold / Route | A torn mineral map aligns only partly with a real seam. | Sell it for certain Gold, follow a dangerous chain for larger value, or destroy it as a fraud. | Gold, Route chain, or Declined / `GoldGrant`, `NextEvent` | B3 |
| 17 | `event.act1.random_event.35.ownerless_wage_sack` | Gold / Recovery | An ownerless wage sack lies beside a miner roster. | Keep it for Gold, return it for Recovery and a positive flag, or take only part for a middle result. | Gold or Heal plus flag / `GoldWorldTrade`, `VitalHeal` | B3 |
| 18 | `event.act1.random_event.36.cracked_bronze_mirror` | Relic / World | A cracked mirror reflects an empty room incorrectly without showing a face. | Accept a build-changing Relic with drawback, or break it for safety and a World flag. | Relic with drawback or World flag / `RelicClaim`, `WorldFlag` | B3 |
| 19 | `event.act1.random_event.37.nameless_long_sword_in_rain` | Relic / Battle | A sheathed nameless long sword rests in rain by an unmarked wall. | Draw it and defeat its guardian for a Relic, or find its owner's name and record a burial flag. | Battle Relic or World flag / `Battle`, `RelicClaim`, `WorldFlag` | B3 |
| 20 | `event.act1.random_event.38.three_cups_of_moonlight` | Relic / Recovery | Three stone cups hold moonlight at different temperatures in an empty pavilion. | Drink one for Recovery, or combine all three into a Relic and forfeit Recovery. | Heal or Relic / `VitalHeal`, `RelicClaim` | B3 |
| 21 | `event.act1.random_event.39.self_knotting_rope` | Relic / Route | A thick rope ties itself beside a broken footbridge. | Keep it as a Relic, or spend it to open a shortcut and lose the Relic. | Relic or Route unlock / `RelicRouteTrade` | B3 |
| 22 | `event.act1.random_event.40.jihan_empty_medicine_folio` | Character / Recovery | Jihan's erased medicine folio lacks one rare ingredient. | Spend it to heal the party, or restore the prescription for a Jihan relationship and follow-up flag. | Heal or Character flag / `CharacterStoryChoice`, `VitalHeal` | B4 |
| 23 | `event.act1.random_event.41.yujin_broken_arrow_fletching` | Character / Battle | Yujin recognizes deliberately misaligned fletching on a broken practice arrow. | Follow the dangerous shot trail into battle, or repair the arrow for a safer relationship result. | Battle Character flag or Character flag / `CharacterStoryChoice`, `Battle` | B4 |
| 24 | `event.act1.random_event.42.twice_ringing_mountain_echo` | Route / World | One call returns from two opposed openings in a wide gorge. | Test quietly to reveal a safe node, or shout for a shortcut and an enemy-alert flag. | Route reveal or shortcut plus risk flag / `RouteOutcome` | B4 |
| 25 | `event.act1.random_event.43.reverse_growing_moss_marker` | Route / Relic | Moss grows on the sunward face of an unmarked boundary stone. | Follow it for a safe detour, or remove it for a Relic and lose Route information. | Route or Relic / `RouteRelicTrade` | B4 |
| 26 | `event.act1.random_event.44.buried_tax_stele` | World / Gold | A horizontal tax stele hides altered dates beneath field soil. | Publish it for smaller Gold and a justice flag, or sell the rubbing for larger Gold and a silence flag. | Two Gold World outcomes / `GoldWorldTrade` | B4 |
| 27 | `event.act1.random_event.45.false_wildfire_boundary_stones` | World / Route | Moved wildfire boundary stones reveal land seizure through mismatched soil. | Restore them for a positive flag and dangerous Route, or leave them for safe passage. | World flag plus Route risk or Declined / `WorldRouteChoice` | B4 |
| 28 | `event.act1.random_event.46.funeral_without_black_cloth` | World / Battle | A funeral without black cloth uses an empty bier and unfamiliar mourners. | Attend for a faction flag, inspect the empty bier and trigger an ambush, or leave. | World flag, Battle, or Declined / `WorldFlag`, `Battle` | B4 |

### 6.1 New-ID rules

- `random_growth.02` and `.03` retain their approved namespace and must not be
  duplicated as general event numbers.
- General numbers `.21` through `.46` map exactly and only to the slugs in the
  table above.
- Missing numbers, aliases, number reuse, slug inference, and display-name ID
  generation are forbidden.
- Follow-up IDs are children under the parent event's stage/node/result
  namespace. They never create another independent pool event.

### 6.2 Exact event/node allow-list

For each new event, the only intro node is the exact event suffix with an
`intro` node prefix:

```text
event.act1.random_growth.02.windworn_sword_marks
node.act1.random_growth.02.windworn_sword_marks.intro
event.act1.random_growth.03.cut_signal_rope_ambush
node.act1.random_growth.03.cut_signal_rope_ambush.intro
event.act1.random_event.21.breath_between_water_drops
node.act1.random_event.21.breath_between_water_drops.intro
event.act1.random_event.22.sleeping_hawk_watch
node.act1.random_event.22.sleeping_hawk_watch.intro
event.act1.random_event.23.temple_hundred_eight_steps
node.act1.random_event.23.temple_hundred_eight_steps.intro
event.act1.random_event.24.herb_scent_empty_barracks
node.act1.random_event.24.herb_scent_empty_barracks.intro
event.act1.random_event.25.hot_spring_beneath_ice
node.act1.random_event.25.hot_spring_beneath_ice.intro
event.act1.random_event.26.sleepless_waystation
node.act1.random_event.26.sleepless_waystation.intro
event.act1.random_event.27.paper_armor_bandits
node.act1.random_event.27.paper_armor_bandits.intro
event.act1.random_event.28.rockfall_scouts
node.act1.random_event.28.rockfall_scouts.intro
event.act1.random_event.29.chain_bridge_tollkeepers
node.act1.random_event.29.chain_bridge_tollkeepers.intro
event.act1.random_event.30.night_beacon_intruders
node.act1.random_event.30.night_beacon_intruders.intro
event.act1.random_event.31.wounded_mountain_tiger_domain
node.act1.random_event.31.wounded_mountain_tiger_domain.intro
event.act1.random_event.32.hidden_ledger_salt_cart
node.act1.random_event.32.hidden_ledger_salt_cart.intro
event.act1.random_event.33.false_mountain_rite_offering_box
node.act1.random_event.33.false_mountain_rite_offering_box.intro
event.act1.random_event.34.half_vein_map
node.act1.random_event.34.half_vein_map.intro
event.act1.random_event.35.ownerless_wage_sack
node.act1.random_event.35.ownerless_wage_sack.intro
event.act1.random_event.36.cracked_bronze_mirror
node.act1.random_event.36.cracked_bronze_mirror.intro
event.act1.random_event.37.nameless_long_sword_in_rain
node.act1.random_event.37.nameless_long_sword_in_rain.intro
event.act1.random_event.38.three_cups_of_moonlight
node.act1.random_event.38.three_cups_of_moonlight.intro
event.act1.random_event.39.self_knotting_rope
node.act1.random_event.39.self_knotting_rope.intro
event.act1.random_event.40.jihan_empty_medicine_folio
node.act1.random_event.40.jihan_empty_medicine_folio.intro
event.act1.random_event.41.yujin_broken_arrow_fletching
node.act1.random_event.41.yujin_broken_arrow_fletching.intro
event.act1.random_event.42.twice_ringing_mountain_echo
node.act1.random_event.42.twice_ringing_mountain_echo.intro
event.act1.random_event.43.reverse_growing_moss_marker
node.act1.random_event.43.reverse_growing_moss_marker.intro
event.act1.random_event.44.buried_tax_stele
node.act1.random_event.44.buried_tax_stele.intro
event.act1.random_event.45.false_wildfire_boundary_stones
node.act1.random_event.45.false_wildfire_boundary_stones.intro
event.act1.random_event.46.funeral_without_black_cloth
node.act1.random_event.46.funeral_without_black_cloth.intro
```

The canonical image path for each new event is:

```text
Assets/ImagesGenerated/Stage/popup_main/{exactNodeId}.main.png
```

## 7. Selector, one-shot, chain, and fallback contract

1. Build one deterministic manifest for the run before nodes are exposed.
2. Assign the required purpose minimums to the 12 selected-route logical
   encounters.
3. Fill remaining encounters using weights while respecting maximums.
4. Select an event uniformly within the eligible purpose unless an approved
   event-specific weight says otherwise.
5. Every independent event ID is one-shot per run.
6. The same Primary Purpose cannot appear in adjacent logical encounters.
7. The same motif tag requires at least three intervening encounters
   (`motifCooldown=3`).
8. A follow-up is a child of the same chain reservation. It is not a new random
   encounter and does not consume another purpose quota.
9. Off-route physical projections do not count as assigned, exposed, or
   consumed events.
10. Re-entry reuses stored assignment, fallback, and chain identity. It never
    rerolls.

### 7.1 Growth contract

- Fixed Chapter Growth remains two applied results.
- Optional Growth uses `optionalGranted <= 1`, `optionalApplied <= 1`, and
  `totalApplied <= 3`.
- Growth candidates may be exposed two to three times, but only one optional
  entitlement may be granted.
- Decline, Battle loss, or abort grants nothing and leaves later candidates
  available.
- Once an entitlement is Pending or Applied, every unexposed Growth candidate
  resolves to its manifest-fixed ordinary fallback before map/node disclosure.
- Candidate zero before grant creates no entitlement. A stale candidate after
  grant keeps the same Pending offer and never grants a reroll or second claim.

### 7.2 Capability and character fallback

Each assignment stores a deterministic fallback compatible with the remaining
purpose quota. It is used when an executor capability is disabled, a required
Character is absent, an exclusion group conflicts, or a Growth claim is
already held. The fallback is fixed in the manifest, remains one-shot, and is
resolved before visual disclosure. It must not break purpose maximums or expose
an icon/name morph.

## 8. Exposure targets

One run exposes 12 of 48 unique events, or 25% of the portfolio.

Under a uniform-without-replacement approximation:

```text
1 run: 12.00 unique = 25.0%
3 runs: 27.75 expected unique = 57.8%
5 runs: 36.61 expected unique = 76.3%
```

Acceptance floors are 25%, 55%, and 75%. Purpose-internal event exposure must
remain within +/-15% of the expected uniform share after eligibility is taken
into account.

## 9. P0, P1, and P2 scope

### P0 curated 12

| Purpose | Events |
|---|---|
| Growth | Smithy derivative, `random_growth.02`, `random_growth.03` |
| Recovery | existing event03, event05, event17 |
| Battle | existing event04, event13, event18 |
| World | existing event01, event12, event19 |

P0 excludes general Gold, Relic, Character, and Route activation until their
executors and receipts pass acceptance. Event prose or pending metadata cannot
stand in for an executable result.

### P1

- Restore approved Gold, Relic, Character, and Route events after their shared
  capability gates pass.
- Original event16 may activate only here and remains mutually exclusive with
  the Smithy derivative.
- Accept each purpose pilot before opening its production batch.

### P2

- Complete all 48 unique pool entries.
- Add long flags, approved chains, difficulty tuning, and final selector
  balance.
- Do not increase the portfolio count with chain children or cosmetic variants.

## 10. Production and visual batches

### 10.1 Content and runtime batches

| Batch | Scope | Acceptance order |
|---|---|---|
| A0 | Safe Growth `.02` pilot | First and blocking |
| A1 | P0 existing-event revisions and Battle Growth `.03` | After A0 |
| B1 | New Growth events21-23 and Recovery events24-26 | After matching capabilities |
| B2 | Battle events27-31 | After Battle result/return gate |
| B3 | Gold events32-35 and Relic events36-39 | After Gold and Relic gates |
| B4 | Character events40-41, Route42-43, World44-46 | After flag/route gates |
| Integration | Shared pool, registry, manifest, generated SO | Sequential after batch acceptance |

JSON, images, and generated SOs use unique event paths. Shared pool, registry,
index, and manifest files have one integration owner and are never edited in
parallel.

### 10.2 Visual batches

New events require exactly 28 unique final popup images. Final PNG reuse among
the new 28 is forbidden.

Additional existing-event work:

- event17 Recovery replacement: one required image;
- event02 and event07: zero to two conditional corrections;
- remaining bitmap workload: minimum 29, maximum 31;
- Smithy derivative is already approved and is not counted again.

| Visual batch | Images |
|---|---:|
| B0 | Growth `.02` pilot: 1 |
| B1 | event17 replacement: 1; Growth `.03`: 1 |
| B2 | Purpose pilots event32, 36, 40, 43, 46: 5 |
| B3 | Remaining new Growth3, Recovery3, Battle5: 11 |
| B4 | Remaining Gold3, Relic3, Character1, Route1, World2: 10 |

The new-28 total is B0 `1` + the `.03` image in B1 `1` + B2 `5` + B3
`11` + B4 `10` = `28`. The event17 replacement is outside that count.

## 11. Motif and visual diversity gates

- Same focus silhouette in adjacent exposed events: zero.
- Same dominant lighting in adjacent exposed events: zero.
- Fire, night, fog, and direct child-harm motifs cannot repeat adjacently.
- In a three-event window: at most one red-clue event, one
  gray-brown/red/mountain-road composition, and one centered display-object
  composition.
- Original event16 and the Smithy derivative cannot be adjacent or co-exist in
  one run.
- event09/event12/event13 cannot be adjacent to event46.
- event17 cannot be adjacent to event38.
- New-28 human-presence ratio target: 14 scenes without people and 14 with
  people; accepted range 12:16 through 16:12.
- New night/dawn scenes: six or fewer.
- Active fire occupies no more than 5% of an image.
- Red accent area is no more than 5%.
- Main focus occupies 18% to 42% of the source image.

Visual source images are 960x1280, RGB unless real transparency requires RGBA,
and contain no embedded writing or numbers. They must remain distinct at the
573x764 EventPopup mask and 144x192 contact thumbnail. WebGL max-1024 derivatives
must retain at least a 48-pixel effective focus width without banding or moire.

## 12. Reward and choice quality gates

- Each event has one recognizable Primary Purpose.
- Purpose recognition in playtest: at least 75%.
- No non-decline choice receives more than 80% selection without an approved
  intentional reason.
- Every non-decline alternative retains at least 40% of the main result's
  cost-adjusted value.
- After risk, time, expected HP loss, and failure chance are normalized, choice
  expected values remain within +/-15% of the event's target budget.
- Free rewards without opportunity cost, risk, condition, or future trade-off
  are forbidden.
- Invalid reward display, executor-free activation, and metadata-only reward
  claims: zero.
- Exact event re-exposure in one run: zero.
- Motif cooldown violations: zero.
- Growth rerolls, duplicate claims, or fallback disclosure changes: zero.
- Players can explain the difference between an available Growth event and an
  applied Growth result at least 80% of the time.
- A selected Growth change is noticed in the next Battle by at least 70% of
  players.

## 13. Validator and production blockers

Production fails when any of the following is true:

- reusable registry count is not 20;
- new registry count is not 28;
- final independent unique count is not 48;
- purpose quota does not sum to 48;
- selector weights do not sum to 100;
- a new ID is missing, aliased, reused, inferred from display text, or outside
  the exact allow-list;
- event11 is independently selectable;
- original16 and the Smithy derivative may co-exist in one run;
- a child chain is counted as another portfolio event;
- an event is active without an accepted executor and result receipt;
- a `rewards` or intent string claims an effect that runtime cannot apply;
- one-shot, purpose adjacency, motif cooldown, fallback, or off-route accounting
  is absent from the deterministic manifest evidence;
- a shared pool/registry file is edited concurrently by multiple production
  units;
- a batch expands before its representative pilot passes.

## 14. Required completion evidence per batch

Every batch returns:

- exact event and node IDs;
- produced and changed paths;
- choice, cost, result, executor, and fallback evidence;
- JSON/schema/builder/validator results;
- generated SO identity and reference evidence where authorized;
- image provenance, contact sheet, 3-resolution, and WebGL checks where
  applicable;
- purpose distribution, one-shot, cooldown, chain, and exposure tests;
- deviations, unresolved risks, next owner, and archive readiness.

Only accepted batches may be promoted into the shared active pool. The 48-event
portfolio is never accepted by file count alone.
