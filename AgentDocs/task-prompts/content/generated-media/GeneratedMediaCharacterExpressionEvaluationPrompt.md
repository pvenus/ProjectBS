# Generated Media Character Expression Evaluation Prompt

```text
역할: registered sparse-ink, bold-outline compressed-detail 또는 open ink-wash character 표현 평가 담당

필수 참조:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaCharacterExpressionEvaluationGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaEvaluationPackageGuide.md
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaRecordGuide.md

Input:
- evaluationPackagePath: {project_relative_path}
- expressionProfileKey: projectbs_character_sparse_ink_pastel_motion@1.0.0 | projectbs_character_bold_outline_compressed_detail@1.0.0 | projectbs_character_bold_outline_compressed_detail@2.0.0 | projectbs_character_open_ink_wash_dynamic_contour@1.0.0 | projectbs_character_open_ink_wash_dynamic_contour@2.0.0 | projectbs_character_bold_outline_attack_motion_flow@1.0.0
- expressionProfilePayloadHash: {exact_registered_hash}
- artifactType: character_main_image | character_animation
- animationRequestId / finalFrameCount: character_animation에서 package의 exact approved 값; main에서는 omit

작업:
1. evaluation package, planning snapshot, prompt/profile record와 모든 media hash를 read-only로 재검증한다.
2. profile key/payload/hash가 registry 및 media의 prompt evidence와 exact 일치하지 않으면 채점 전에 blocked로 종료한다.
3. GeneratedMediaCharacterExpressionEvaluationGuide의 pre-score fatal gate를 순서대로 적용한다.
4. main image는 비례, omission, no-fill, pigment area/accent count, palette와 identity anchor를 확인한다.
4a. bold-outline v2는 공통 비례/outline/face와 total/internal/fold 64/56/5, exact ochre anchor sites 및 35/4 color limits, closed halo branch를 독립 측정한다. opaque/scenic/noncentered/nonfading/directional-shadow halo는 fatal이고 재현 가능한 측정이 없으면 `character_evaluation_evidence_insufficient`로 중단한다.
4a. bold-outline main image는 4.0-5.0 heads, exact authored outside/internal thickness와 ratio >=2, closed facial mark total/component budget, compressed-detail forbidden set, hue anchor/coverage/mass limits 및 neutral outline/weapon colors를 독립 검증한다. profile이 single-image-only이므로 animation 입력이면 profile mismatch로 차단한다.
4b. open ink-wash main image는 4-5 heads/4.25 target 및 young adult/no child, 35-55 omission/45 target, pressure-variable mok-seon의 brush start/directional drag/dry end/directional weight, broad rough watercolor-pastel bleed/misalignment, separate three-role palette, figure interior와 canvas 각각 achromatic/unpainted >=70%, removable warm-ivory/no halo/vignette/scene/shadow, exact Korean/Joseon identity/equipment anchors, audit-only/no-binding reference role을 독립 검증한다. profile이 single-image-only이므로 animation 입력이면 profile mismatch로 차단한다.
4c. open ink-wash v2는 위 gate에 full-body head-count measurement, sparse surface detail, uniform #F2EFE6/no radial-or-edge-darkening background를 추가한다. realistic modeled face, individual armor plates/scales/rivets/lacing/fasteners, garment microfold, microtexture, modeled light 또는 realistic material은 `character_evaluation_open_ink_wash_v2_surface_detail_gate_failed`이다. compact preview receipt는 참고 evidence일 뿐 evaluation package/media 재검증을 대체하거나 preview를 evaluation input으로 승격하지 않는다.
5. character animation은 ordered member count가 package/animationRequest의 positive approved finalFrameCount와 exact 일치하는지 확인하고, 임의의 유효한 frame count 전체에 각 frame budget, line/pigment motion cue, action 변화와 identity anchor 안정성을 적용한다. six-frame은 테스트 fixture일 뿐 runtime 조건이 아니다.
5a. motion-flow successor는 base v2 key/hash와 exact 18/8, 64/56/5, color anchors/halo를 먼저 재검증한다. 이후 indigo sword/torso 3-5 directional flow, gray-brown shoulder/hem inertia, bounded dark-neutral trajectory, non-static ordered continuity 및 모든 identity/equipment anchor를 독립 fatal gate로 평가한다. generic clean-vector sheet, arbitrary speed lines 또는 magic VFX는 motion-flow gate failure이다.
6. 불안정한 pixel 측정을 정확한 수치로 가장하지 않는다. 관찰 불충분이면 evidence blocker를 반환한다.
7. fatal gate가 없을 때만 공통 evaluation package의 점수 체계를 적용한다.
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
```
