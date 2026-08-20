# Generated Media Character Expression Evaluation Prompt

```text
역할: registered sparse-ink, bold-outline compressed-detail 또는 open ink-wash character 표현 평가 담당

필수 참조:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaStyleReferenceBindingGuide.md

Input:
- evaluationPackagePath: {project_relative_path}
- expressionProfileKey: projectbs_character_sparse_ink_pastel_motion@1.0.0 | projectbs_character_bold_outline_compressed_detail@1.0.0 | projectbs_character_bold_outline_compressed_detail@2.0.0 | projectbs_character_open_ink_wash_dynamic_contour@1.0.0 | projectbs_character_open_ink_wash_dynamic_contour@2.0.0 | projectbs_character_bold_outline_attack_motion_flow@1.0.0
- expressionProfilePayloadHash: {exact_registered_hash}
- artifactType: character_main_image | character_animation
- animationRequestId / finalFrameCount: character_animation에서 package의 exact approved 값; main에서는 omit

작업:
1. evaluation package, planning snapshot, 선택된 provenance branch와 모든 media hash를 read-only로 재검증한다. strict branch는 기존 prompt/profile/generation record를 사용한다. accepted-result branch는 accepted capture record/path/raw SHA, receipt/JCS hash와 closed acceptedPromptEvidence를 사용하며 fake prompt/generation record를 요구하거나 만들지 않는다.
2. strict branch의 profile key/payload/hash 또는 accepted-result branch의 planning/routing profile identity와 conditional acceptedPromptEvidence가 package evidence와 exact 일치하지 않으면 채점 전에 blocked로 종료한다. animation recovered branch는 providerPromptPayloadHash/promptFileSha256를 검증한다. historical prompt가 unavailable인 character_single_image는 source=accepted_result_capture,status=unavailable_observed,claim=not_claimed와 prompt member 부재를 검증하고 prompt identity/prose를 만들지 않는다. mixed/partial/unknown branch fields는 `evaluation_package_input_branch_conflict`, `evaluation_package_input_branch_incomplete`, 또는 `evaluation_package_unknown_branch_field`로 차단한다.
3. GeneratedMediaCharacterExpressionEvaluationGuide의 pre-score fatal gate를 순서대로 적용한다.
4. main image는 비례, omission, no-fill, pigment area/accent count, palette와 identity anchor를 확인한다.
4a. bold-outline v2는 공통 비례/outline/face와 total/internal/fold 64/56/5, exact ochre anchor sites 및 35/4 color limits, closed halo branch를 독립 측정한다. opaque/scenic/noncentered/nonfading/directional-shadow halo는 fatal이고 재현 가능한 측정이 없으면 `character_evaluation_evidence_insufficient`로 중단한다.
4a. bold-outline main image는 4.0-5.0 heads, exact authored outside/internal thickness와 ratio >=2, closed facial mark total/component budget, compressed-detail forbidden set, hue anchor/coverage/mass limits 및 neutral outline/weapon colors를 독립 검증한다. profile이 single-image-only이므로 animation 입력이면 profile mismatch로 차단한다.
4b. open ink-wash main image는 4-5 heads/4.25 target 및 young adult/no child, 35-55 omission/45 target, pressure-variable mok-seon의 brush start/directional drag/dry end/directional weight, broad rough watercolor-pastel bleed/misalignment, separate three-role palette, figure interior와 canvas 각각 achromatic/unpainted >=70%, removable warm-ivory/no halo/vignette/scene/shadow, exact Korean/Joseon identity/equipment anchors를 독립 검증한다. reference는 semantic-only audit evidence이거나 exact six-member reviewed durable `role=style_only` binding이어야 하며, 후자는 asset/review/index hash와 profile scope를 재검증하고 person/identity/pose/action/clothing/equipment/edit-target 전이를 금지한다. profile이 single-image-only이므로 animation 입력이면 profile mismatch로 차단한다.
4c. open ink-wash v2는 hard fail을 material identity/project-usability defect로만 제한한다: wrong person/gender presentation, material child/adult mismatch, major species/body/face/costume/weapon/equipment/handedness substitution, wrong semantic action/direction, corrupt/missing member, severe clipping, broken alpha/background, unstable canvas/anchor, another-design 수준의 extreme proportion/silhouette/style divergence, visible text/watermark/UI이다. line/brush/omission, low-density flat armor marks/scales, wrap bands/fold counts, pigment/palette/negative-space nuance, minor proportion, modest detail/polish/readability/aesthetic variance는 same planned adult character가 usable하면 soft score finding만 허용한다. 특히 one broad shoulder mass 또는 existing leg-wrap region 안의 bounded marks는 `minor_expressive_surface_variance`, regenerationRequired=false로 기록한다. dense enumerable plates/scales/rivets/lacing/fasteners, realistic construction/material 또는 material planning/identity/usability conflict만 `character_evaluation_open_ink_wash_v2_surface_detail_gate_failed`이다. 이 모델은 animation/다른 profile·adapter에는 적용하지 않는다.
5. character animation은 ordered member count가 package/animationRequest의 positive approved finalFrameCount와 exact 일치하는지 확인하고, 임의의 유효한 frame count 전체에 각 frame budget, line/pigment motion cue, action 변화와 identity anchor 안정성을 적용한다. six-frame은 테스트 fixture일 뿐 runtime 조건이 아니다.
5a. motion-flow successor는 base v2 key/hash와 exact 18/8, 64/56/5, color anchors/halo를 먼저 재검증한다. 이후 indigo sword/torso 3-5 directional flow, gray-brown shoulder/hem inertia, bounded dark-neutral trajectory, non-static ordered continuity 및 모든 identity/equipment anchor를 독립 fatal gate로 평가한다. generic clean-vector sheet, arbitrary speed lines 또는 magic VFX는 motion-flow gate failure이다.
6. 불안정한 pixel 측정을 정확한 수치로 가장하지 않는다. 관찰 불충분이면 evidence blocker를 반환한다.
7. exact open ink-wash v2 main은 observable 100-point score를 유지하고 hard fail 없음+total>=80이면 PASS, total<80 또는 hard fail이면 FAIL이다. soft category minimum은 없고 CONDITIONAL_PASS를 만들지 않는다. 모든 deduction과 Major/Minor/Suggestion finding을 evidence와 함께 기록하며 PASS만 passForProjectCopy=true이다. 다른 profile은 기존 점수 체계를 유지한다.
8. 이미지 생성·수정, provider 호출, preservation, promotion, Unity, Slack, Git을 수행하지 않는다.

Output:
- status: PASS | FAIL | BLOCKED
- artifactType / contentId / animationRequestId / approved finalFrameCount / ordered frame identities
- expressionProfileKey / expressionProfilePayloadHash
- fatalGateResults / evidenceRefs
- score: fatal gate가 없고 근거가 충분할 때만
- findings / requiredActions / safeToReevaluate

실패 시 Output:
- status: FAIL 또는 BLOCKED
- failureType: 중앙 ImageGen-only contract 8.4.1의 exact evaluation token 하나
- failedGate / evidenceRefs / requiredDecision / safeToReevaluate

검증:
- fatal gate는 점수로 상쇄하지 않는다.
- main과 animation 예산을 혼용하지 않는다.
- sparse, bold-outline, open ink-wash gate/token을 혼용하지 않는다.
- animation ordered member count는 approved finalFrameCount와 exact 일치해야 하며 6으로 hardcode하지 않는다.
- 입력은 ContentFolderStructureGuide의 staging/evaluation 경계를 따르고 이 prompt는 새 storage path나 project artifact를 만들지 않는다.
- media 또는 upstream record를 수정하지 않는다.
- open ink-wash v2 main의 bounded readability marks만으로 regeneration을 요구하지 않으며, attack animation을 이 정책 변경 때문에 재평가·재생성하지 않는다.
- `generated_media_true_alpha_foreground@1.0.0` / `2671524f7215ceb69218a0a951b17ffff6d9b3671a8c7fe7642b00ddabfab108` package는 quality score 전에 payload hash/selection/receipt/member alpha evidence를 재검증한다. outside alpha0, inside-only partial alpha, no background/fringe, safe margin/no clipping과 animation exact root/baseline/scale/no-recenter/no-flicker/no-fragment가 모두 PASS해야 score한다. failure는 central `true_alpha_*` hard token이며 opaque branch 의미를 바꾸지 않는다.
- `projectbs_character_open_ink_wash_attack_motion@1.0.0` / `07865d41a83bfebcebc62dcdc1a50590724f4344e2fe492a5f5996509ed2026c` animation package는 exact open-ink v2 base style/identity/equipment, ordered single-attack continuity, true-alpha hard gates 순서로 검증한다. 각 실패는 `character_evaluation_open_ink_attack_style_gate_failed`, `character_evaluation_open_ink_attack_motion_continuity_gate_failed`, `character_evaluation_open_ink_attack_true_alpha_gate_failed`이며 sparse-motion evidence로 대체하지 않는다.
```
