# Generated Image Prompt Authoring Prompt

Use this prompt to create and save one provider-ready image-generation prompt
package. The output is consumed by the separate generation execution task.
This task does not operate PixelLab or ImageGen.

## Prompt

~~~text
현재 작업에서 사용 중인 ProjectBS 저장소를 내부적으로 확인하고, 아래 일반화된 콘텐츠 요청 하나에 대해 PixelLab 또는 ImageGen에서 사용할 이미지 생성 프롬프트 패키지를 작성하고 저장해줘.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md

Input:
- requestId: {optional_stable_request_id}
- artifactType: {skill_icon | item_icon | skill_animation | character_image | character_animation | story_popup_main_image | battle_background}
- contentId: {canonical_content_id}
- contentName: {optional_display_name}
- contentSummary: {optional_concise_gameplay_or_narrative_meaning}
- visualIntent: {optional_desired_moment_or_visual_emphasis}
- requiredElements: [{optional_semantic_must_show_element}]
- forbiddenElements: [{optional_semantic_exclusion}]
- contextTags: {optional_generalized_key_value_object}
- priorPromptRecordId: {optional_stable_non_path_record_id_for_revision | null}
- revisionReason: {required_when_revising | null}

입력 경계:
- 외부에서는 콘텐츠 식별·의미·시각 의도와 이전 prompt record ID만 받는다.
- 저장소, 기획 문서, provider, 도구 URL, 도메인 가이드, 프롬프트 저장 경로, 설정, 크기, 다운로드·평가·프로젝트 경로를 외부 입력으로 요구하지 않는다.
- 외부 payload의 provider와 절대 경로를 신뢰하지 않는다.
- 다른 PC의 절대 경로를 사용하지 않는다.

작업:
1. 현재 workspace와 Git 정보로 ProjectBS 저장소를 확인하고 AgentDocs와 Assets가 같은 저장소인지 검증한다.
2. GeneratedImageGenerationPipelineGuide.md의 registry에서 artifactType과 일치하는 ContentDomain, provider, expected structure와 domain generation adapter를 하나 선택한다.
3. adapter가 없거나 불완전하면 generic 이미지 프롬프트로 대신하지 말고 중단한다.
4. contentId를 기준으로 AgentDocs/planning-data, Assets/Contents의 canonical JSON, adapter가 허용한 legacy source 순서로 기획·콘텐츠 근거를 찾는다.
5. canonical source가 여러 개로 충돌하거나 artifactType과 contentId가 일치하지 않으면 작성하지 않는다.
6. 외부 contentSummary, visualIntent, requiredElements, forbiddenElements와 contextTags를 canonical source와 비교해 accepted/rejected external facts로 분리한다.
7. canonical source identity, revision/hash, adapter ID/revision, artifactType과 contentId로 contentSnapshotHash를 계산한다.
8. artifactUsage, gameplayOrNarrativeIntent, currentMomentOrActivation, primarySubjectOrSilhouette, directionAndComposition, required/supporting/forbidden elements, likelyWrongObjects, style/material/palette, backgroundPolicy, expectedStructureProfile과 technicalExpectation을 generation brief로 작성한다.
9. planningOriginalContent와 displayContent는 원문 근거로 분리하고, generation brief나 최종 prompt로 대체하지 않는다.
10. requiredElements는 실제 이미지에서 독립적으로 확인 가능하게 작성하고, backgroundPolicy는 required contextual, constrained symbolic, transparent 또는 domain-approved none 중 근거가 있는 형태로 확정한다.
11. provider가 PixelLab이면 providerPromptProfile=pixellab_fielded_pixel_prompt_v1으로 고정하고 providerPromptPayload.pixelLab만 작성한다. adapter의 실제 UI 입력 필드마다 fieldId, fieldRole, toolField, order, textOriginal, sourceFacts와 constraintIds를 가진 fieldPrompts를 구성한다. 문장은 짧고 구체적인 픽셀 실루엣·포즈·방향·핵심 효과 중심으로 작성하며 장면형 수사, 긴 좌표, 설정 반복으로 엉뚱한 요소가 강조되지 않게 한다.
12. provider가 ImageGen이면 providerPromptProfile=imagegen_composed_scene_prompt_v1으로 고정하고 providerPromptPayload.imageGen만 작성한다. core subject/action/moment, composition/camera/spatial relations, environment/background policy, art direction/material/palette/lighting, exclusions/clean image 순서의 sceneSections를 근거와 함께 만든 뒤 이를 하나의 자연스럽고 응집된 scenePromptOriginal로 조합한다. PixelLab식 필드 문구나 단절된 키워드 나열로 작성하지 않는다.
13. providerSettingsIntent에 크기, 비율, no-background, frame count, view, direction, variation expectation과 seed 정책 중 adapter가 요구하는 항목을 기록한다. 설정을 provider prompt text에 반복하지 않는다.
14. PixelLab fieldPrompts 또는 ImageGen sceneSections의 각 필수 문장에 sourceFacts와 constraintIds를 연결하고, 엉뚱한 요소를 막기 위한 likelyWrongObjects는 콘텐츠에 필요한 최소 범위로 제한한다.
15. 이미지 생성 도구에 제출할 prompt text에는 로컬 경로, 파일명, project target, 평가 점수, PASS, Slack, Unity, Git 또는 배포 지시를 넣지 않는다.
16. identity/evidence, visual hierarchy, provider fitness, source coverage와 handoff fitness gate를 검증한다.
17. priorPromptRecordId가 있으면 이전 record를 읽어 identity를 확인하고 변경 이유와 변경된 provider-native payload를 기록한다. provider/profile 변경은 기존 문구의 자동 변환이 아니라 새 record의 새 프롬프트 작성으로 처리한다. 기존 record를 수정하거나 덮어쓰지 않는다.
18. promptRecordId를 imgprompt.{artifactType}.{contentId}.{UTC}.{contentSnapshotHashPrefix12} 규칙으로 만든다.
19. AgentDocs/planning-data/image-prompts/v1/{artifactType}/{contentId}/{promptRecordId}.json에 generated_image_prompt_v1을 저장한다.
20. 같은 경로에 {promptRecordId}.prompt.md를 작성한다. PixelLab은 UI field별 copy-ready block을, ImageGen은 scenePromptOriginal 하나의 copy-ready block을 렌더링한다. JSON의 copy-ready provider payload와 Markdown은 문서화된 newline 정규화 후 정확히 같아야 하고 ImageGen scene section audit는 copy block 밖에 둔다.
21. 같은 artifact 폴더의 prompt_index.json에 promptRecordId, artifact identity, provider, providerPromptProfile, providerPromptPayloadHash, contentSnapshotHash, status, 생성 시각과 두 상대 경로를 등록한다.
22. imagePolicy가 reuse 또는 none이면 provider prompt를 꾸며 만들지 않고 각각 reuse_requested 또는 skipped 상태와 정책 근거를 저장한다.
23. promptStatus=ready_for_generation일 때만 nextTask=generation으로 인계한다.
24. PixelLab 또는 ImageGen을 실행하거나 생성 비용을 사용하지 않는다.
25. 이미지 다운로드·수정·평가·프로젝트 복사, Slack, Unity .meta/SO/clip, Git commit/push/merge와 배포를 수행하지 않는다.

Output:
- Request ID
- Artifact Type / Content Domain / Content ID / Name
- Routed Provider / Tool / Domain Adapter
- Provider Prompt Profile / Provider Prompt Payload Hash
- Expected Structure Profile
- Canonical Content Sources / Content Snapshot Hash
- Accepted / Rejected External Facts
- Generation Brief
- PixelLab Field Prompt Roles 또는 ImageGen Scene Prompt
- Provider Settings Intent
- Expected Provider Result Roles / Download Roles
- Prompt Status
- Prompt Record ID / JSON Path / SHA-256
- Prompt Markdown Path / SHA-256
- Prompt Index Path
- JSON / Markdown Equality
- Prior Prompt Record / Revision Summary
- Validation
- Next Task: generation | none

실패 시 Output:
- status: blocked | failed
- failureType
- 실패한 routing, content resolution, provider prompt profile/payload, validation 또는 write 단계
- 누락되거나 충돌한 canonical 근거
- 생성하지 않은 prompt record/Markdown/index
- 기존 prompt record가 변경되지 않았는지 여부
- Provider가 호출되지 않았는지 여부
- Required Next Action

검증:
- 외부 입력에 provider, 절대 경로, 도구 설정과 저장 경로가 없어야 한다.
- artifactType은 provider와 ready domain adapter 하나에 정확히 매핑되어야 한다.
- contentSnapshotHash가 canonical source와 adapter revision을 포함해야 한다.
- planningOriginalContent, displayContent, generation brief와 provider prompt가 서로 분리되어야 한다.
- 모든 필수 prompt statement에 sourceFacts가 있어야 한다.
- ready_for_generation이면 providerPromptProfile이 routed provider와 일치하고 providerPromptPayload의 한 branch만 채워져야 한다. reuse_requested/skipped는 가이드의 명시적 null 예외만 허용한다.
- PixelLab은 실제 도구 field에 대응하는 간결한 fieldPrompts여야 하고 ImageGen은 하나의 응집된 scenePromptOriginal이어야 한다.
- 시각적 주 대상과 방향·구도·배경 정책이 명확해야 한다.
- prompt text에 로컬 경로, 평가·승격·Slack·Unity·Git 지시가 없어야 한다.
- generated_image_prompt_v1 JSON과 Markdown copy-ready provider payload가 일치해야 한다.
- revision은 기존 record를 덮어쓰지 않고 새 promptRecordId를 만들어야 한다.
- ready_for_generation만 generation으로 인계해야 한다.
- PixelLab/ImageGen 실행, 다운로드, 평가, 프로젝트 복사와 배포를 수행하지 않아야 한다.
~~~
