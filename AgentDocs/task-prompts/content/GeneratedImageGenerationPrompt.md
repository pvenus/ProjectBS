# Generated Image Generation Prompt

Use this prompt to request one generated visual artifact from a dedicated
generation task. The execution task selects PixelLab or ImageGen and the exact
content-domain guide internally. It returns a generation record for a separate
download task.

## Prompt

~~~text
현재 작업은 생성 요청 부모 역할로 동작하고, 아래 일반화된 요청 하나를 동일 ProjectBS 저장소 맥락의 전용 이미지 생성 작업으로 전달해줘. 실제 PixelLab 또는 ImageGen 조작은 전용 생성 작업만 수행하고 부모 작업은 생성 도구를 직접 사용하지 마.

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- AgentDocs/planning-guides/content/GeneratedImageGenerationPipelineGuide.md
- AgentDocs/planning-guides/content/GeneratedImagePromptAuthoringGuide.md

Input:
- requestId: {optional_stable_external_request_id}
- artifactType: {skill_icon | item_icon | skill_animation | character_image | character_animation | story_popup_main_image | battle_background}
- contentId: {canonical_content_id}
- promptRecordId: {optional_stable_non_path_generated_image_prompt_v1_record_id}
- contentName: {optional_display_name}
- contentSummary: {optional_concise_gameplay_or_narrative_meaning}
- visualIntent: {optional_desired_moment_or_visual_emphasis}
- requiredElements: [{optional_semantic_must_show_element}]
- forbiddenElements: [{optional_semantic_exclusion}]
- contextTags: {optional_generalized_key_value_object}

외부 입력 경계:
- 외부에서는 콘텐츠의 일반화된 식별 정보와 의미만 받는다.
- 저장소, 기획 문서, 생성 가이드, PixelLab/ImageGen, 도구 URL, prompt path/text, 설정, 크기, 로컬 폴더, 다운로드 경로, project target과 평가 기준을 외부 입력으로 요구하지 않는다.
- 외부 payload에 provider나 경로가 있어도 routing 근거로 신뢰하지 않는다.
- 다른 PC의 절대 경로를 사용하지 않는다.

부모 작업:
1. 현재 작업은 generation_parent다. artifactType과 contentId가 있는지 확인하고 요청 내용을 정규화한다.
2. 같은 requestId 또는 동일 artifactType/contentId를 처리 중인 전용 생성 작업이 있는지 확인한다.
3. 기존 활성 생성 작업이 있으면 새로 만들지 말고 같은 작업에 요청 또는 재시도를 전달한다.
4. 기존 작업이 없을 때만 현재 저장소 맥락을 상속하는 generation_execution 작업을 정확히 하나 만든다.
5. 생성 작업에는 parentTaskId, requestId, optional promptRecordId와 Input의 일반화된 필드만 전달한다. provider, 절대 경로, prompt text, 점수, 복사 경로 또는 가이드 본문을 대신 결정해 전달하지 않는다.
6. 작업 생성 결과가 timeout 또는 불명확하면 즉시 중복 작업을 만들지 말고 작업 목록에서 동일 요청의 실행 작업 존재 여부를 다시 확인한다.
7. 생성 작업이 완료되거나 blocker를 반환할 때까지 동일 작업을 추적한다.
8. retry 또는 입력 보완은 같은 generation_execution 작업에 전달한다.
9. 부모 작업은 PixelLab 브라우저 조작, ImageGen 호출과 generation record 작성을 수행하지 않는다.
10. 실행 작업이 반환한 generationTaskId, generationRecordId, artifact identity, provider, record visibility, record path/hash와 generationStatus의 일치 여부만 검증하고 그대로 보고한다. isolated_worktree 또는 message_only를 shared_workspace로 추정하지 않는다.

generation_execution 작업:
1. 전달받은 parentTaskId와 requestId를 확인하고 generation_execution 역할을 선언한다. 다른 생성 작업을 만들거나 중첩 위임하지 않는다.
2. 현재 작업 workspace와 Git 정보에서 ProjectBS 저장소를 확인하고 AgentDocs와 Assets가 같은 저장소인지 검증한다.
3. GeneratedImageGenerationPipelineGuide.md의 registry에서 artifactType과 정확히 일치하는 ContentDomain, provider, 구조 기대값과 도메인 generation adapter를 하나 선택한다.
4. provider나 adapter가 없거나 불완전하면 다른 생성기로 대체하지 말고 중단한다.
5. contentId를 기준으로 AgentDocs/planning-data, Assets/Contents의 canonical JSON, 도메인 가이드가 허용한 legacy source 순서로 콘텐츠 근거를 찾는다.
6. 외부 설명과 canonical source가 충돌하면 canonical source를 우선하고 accepted/rejected external facts를 분리한다. identity나 타입이 모호하면 생성하지 않는다.
7. promptRecordId가 있으면 generated_image_prompt_v1의 정확한 record를 찾고 artifactType/contentId를 확인한다. 없으면 현재 contentSnapshotHash, provider와 adapter revision이 일치하는 유일한 ready_for_generation record만 선택한다.
8. prompt record가 없거나 여러 개이거나 stale이면 generation brief나 provider prompt를 현재 작업에서 새로 작성하지 말고 prompt_record_not_found, ambiguous_prompt_record 또는 prompt_record_stale로 중단하고 GeneratedImagePromptAuthoringPrompt.md를 다음 작업으로 보고한다.
9. prompt record JSON/Markdown의 copy-ready provider payload 일치, record SHA-256, contentSnapshotHash, provider/tool, providerPromptProfile, providerPromptPayloadHash, domain adapter와 expected structure를 현재 canonical source 및 routing과 대조한다. routed provider에 맞는 payload branch 하나만 채워져 있지 않으면 중단한다.
10. generation brief, planningOriginalContent, providerPromptPayload와 providerSettingsIntent를 검증된 prompt record에서 읽고 변경하거나 다른 provider 형식으로 변환하지 않는다.
11. imagePolicy가 reuse 또는 none인 prompt record는 provider를 호출하지 않고 reuse_requested 또는 skipped 기록과 정책 근거를 반환한다.
12. PixelLab route이면 providerPromptProfile=pixellab_fielded_pixel_prompt_v1인지 확인하고 providerPromptPayload.pixelLab.fieldPrompts의 textOriginal을 order 순서대로 정확한 toolField에 그대로 입력한다. 이를 하나의 장면 문단으로 합치지 않는다. 정확한 PixelLab tool/page와 settings intent를 사용하고 로그인, credit와 실행 비용을 확인하며 다른 이미지 생성기를 사용하지 않는다.
13. ImageGen route이면 providerPromptProfile=imagegen_composed_scene_prompt_v1인지 확인하고 providerPromptPayload.imageGen.scenePromptOriginal 하나를 그대로 제출한다. 이를 PixelLab식 필드 조각으로 분해하지 않는다. record의 비율·설정 intent를 사용해 ImageGen만 실행한다.
14. provider에 제출한 정확한 promptRecordId, providerPromptProfile, native submitted payload, submitted payload hash, 설정, 도구, 페이지, 시간, 비용 근거와 attempt 정보를 기록한다.
15. provider 결과가 요청과 연결되어 있고 정상적으로 표시되며 필요한 variation, reference/sheet 또는 named animation이 생성되었는지 provider-operation 수준에서 확인한다.
16. 정식 이미지 평가 rubric을 실행하거나 점수, PASS, Conditional Pass, project-copy eligibility를 부여하지 않는다.
17. 여러 variation이 있는 경우 도메인 generation guide가 허용하는 provisional preferred result를 지정할 수 있지만 selectionStatus=provisional_not_evaluated로 기록하고 모든 result ref를 보존한다.
18. provider-operation 또는 명백한 generation-contract 실패만 도메인 제한과 전체 최대 2회 중 더 엄격한 범위에서 재시도한다. prompt text 변경이 필요하면 이 작업에서 수정하지 말고 새 promptRecordId를 요청한다.
19. generationRecordId를 gen.{artifactType}.{contentId}.{UTC}.{requestHashPrefix12} 규칙으로 만든다.
20. 현재 실행 작업의 workspace visibility를 shared_workspace, isolated_worktree 또는 message_only로 판정한다.
21. 파일 쓰기가 가능한 경우 현재 실행 workspace의 AgentDocs/planning-data/image-generation/v1/{artifactType}/{contentId}/{generationRecordId}.json에 promptRecordId/hash를 포함한 generated_image_generation_v1 레코드를 작성하고 SHA-256을 기록한다. message_only이면 완전한 검증 결과 payload와 hash를 반환하며 파일 저장을 주장하지 않는다.
22. 파일을 작성한 경우 같은 artifact 폴더의 generation_index.json에 record ID, promptRecordId, request identity, provider, providerPromptProfile, submittedProviderPayloadHash, status, 생성 시각과 상대 record 경로를 등록한다.
23. downloadHandoff에 generationTaskId, generationRecordId, promptRecordId/hash, recordVisibility, record path/hash, artifactType, contentId, provider, expected structure, 모든 provider result ref, provisional preferred ref, expected download roles와 기술적 다운로드 기대값을 기록한다.
24. PixelLab 결과 페이지 또는 ImageGen 결과 첨부를 후속 다운로드 작업이 식별 가능한 상태로 유지한다.
25. provider 결과를 로컬로 다운로드하거나 preview를 source 파일로 저장하지 않는다.
26. 이미지를 평가·수정·정규화하거나 Assets/ImagesGenerated 또는 Assets/Resources로 복사하지 않는다.
27. Slack, Unity .meta/SO/animation clip/runtime binding, Git commit/push/merge와 배포를 수행하지 않는다.
28. 생성 레코드와 index가 같은 request/prompt/artifact/provider/result refs를 가리키는지 검증하고 record visibility를 사실대로 기록한 뒤 부모 작업에 결과를 반환한다.

Output:
- Parent Task ID / Generation Task ID
- Request ID
- Artifact Type / Content Domain / Content ID
- Routed Provider / Provider Tool
- Routed Domain Generation Adapter
- Expected Structure Profile
- Canonical Content Sources
- Accepted / Rejected External Facts
- Generation Brief Summary
- Prompt Record ID / Path / SHA-256 / Content Snapshot Hash
- Provider Prompt Profile / Provider Prompt Payload Hash
- Attempt Count / Attempt Status
- Exact Prompt Record Status
- Provider Result References
- Provisional Preferred Result / Selection Status
- Generation Status
- Generation Record ID / Visibility / Project-Relative Record Path / SHA-256
- Generation Index Path
- Download Handoff
- Blockers
- Required Next Task: download | none

실패 시 Output:
- status: blocked | failed
- failureType
- Parent Task ID / Generation Task ID
- 실패한 dispatch, routing, content resolution, provider 또는 record 단계
- Provider가 호출되었는지 여부
- 발생한 비용 또는 credit 사용 근거
- 확인한 Prompt Record ID와 stale/mismatch 여부
- 보존된 attempt와 provider result refs
- 생성하지 않은 generation record 또는 index
- 호출하지 않은 후속 단계
- Required Next Action

검증:
- 부모와 실행 작업의 generation owner가 각각 하나이고 실제 provider 조작은 실행 작업만 해야 한다.
- retry는 같은 generation_execution 작업을 사용해야 한다.
- 외부 입력에 provider, 절대 경로, 도구 설정, download/project 경로와 평가 점수가 없어야 한다.
- artifactType은 provider와 ready domain adapter 하나에 정확히 매핑되어야 한다.
- generated_image_prompt_v1 record가 current contentSnapshotHash, provider와 adapter revision에 일치해야 한다.
- providerPromptProfile과 유일한 providerPromptPayload branch가 routed provider에 일치해야 한다.
- PixelLab fieldPrompts 또는 ImageGen scenePromptOriginal은 prompt record에서 그대로 사용하고 generation 작업에서 다시 작성·병합·분해·변환하지 않아야 한다.
- PixelLab과 ImageGen을 서로 대체하지 않아야 한다.
- exact provider-native submitted payload와 hash, settings, attempts와 result refs가 보존되어야 한다.
- provisional selection을 평가 PASS로 표현하지 않아야 한다.
- generated_image_generation_v1과 generation_index.json이 같은 요청과 artifact를 가리켜야 한다.
- generation record의 shared/isolated/message-only 가시성을 사실대로 보고해야 한다.
- downloadHandoff가 모든 예상 artifact role과 provider result ref를 포함해야 한다.
- 로컬 다운로드 파일, 평가 결과, project hash, promotion 상태와 Unity metadata를 생성 레코드에 꾸며 넣지 않아야 한다.
- 다운로드, 평가, 프로젝트 복사, Slack, Unity 작업, Git과 배포를 수행하지 않아야 한다.
~~~
