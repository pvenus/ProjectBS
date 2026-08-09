# Content Folder Create Prompt

## Master Concept Reference

Before using this prompt, read and apply:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
```

Use this prompt to create or complete the `json`/`so` structure and required
generated-image folders for one content domain. It does not generate content
files or migrate existing runtime data.

## Prompt

```text
작업 폴더 = {repository_root}

참조 가이드:
- AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
- AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
- {domain_schema_or_so_guide}

Input:
- contentDomain: {PascalCase_content_domain}
- domainDescription: {domain_ownership_description}
- primaryJsonType: {json_type}
- primarySOType: {so_type}
- builderPath: {builder_path_or_not_implemented}
- generatedImageArtifactTypes: [{image_artifact_type}] or []
- allowCreate: {true_or_false}
- migrationApproved: false

작업:
1. contentDomain이 단수 PascalCase이며 ^[A-Z][A-Za-z0-9]*$ 규칙을 만족하는지 확인한다.
2. domainDescription, primaryJsonType, primarySOType을 비교해 하나의 명확한 SO 소유권 경계인지 검증한다.
3. Assets/Contents, Assets/ImagesGenerated, 관련 스키마, SO 및 이미지 가이드, 빌더, 런타임 로더를 검색해 같은 도메인 또는 동일 소유권의 기존 폴더가 있는지 확인한다.
4. 동일 이름의 폴더 .meta만 존재하면 해당 GUID를 보존하면서 실제 도메인 폴더를 생성 대상으로 판정한다.
5. allowCreate=true이고 소유권 검증을 통과한 경우에만 다음 구조를 생성하거나 완성한다.
   - Assets/Contents/{ContentDomain}
   - Assets/Contents/{ContentDomain}/json
   - Assets/Contents/{ContentDomain}/so
6. generatedImageArtifactTypes가 비어 있지 않으면 다음 구조를 만들고 요청된 artifact type만 추가한다.
   - Assets/ImagesGenerated/{ContentDomain}
   - Assets/ImagesGenerated/{ContentDomain}/{image_artifact_type}
7. 각 image_artifact_type이 lowercase snake_case 및 ^[a-z][a-z0-9_]*$ 규칙을 만족하는지 확인한다. 목록이 비어 있으면 ImagesGenerated 아래에 빈 도메인 폴더를 만들지 않는다.
8. 기존 폴더와 .meta는 그대로 보존한다. 새 폴더는 Unity를 통해 .meta를 생성하는 방식을 우선하고, Unity를 사용할 수 없으면 중복되지 않는 새 GUID로 올바른 folderAsset .meta를 만든다.
9. json과 so는 정확히 같은 깊이의 lowercase sibling 폴더로 만든다.
10. JSON, SO, 이미지, README, .gitkeep, 샘플 파일, 임시 폴더는 생성하지 않는다.
11. 기존 Generated 폴더를 so로 간주하지 않는다. migrationApproved=false이므로 Generated와 Assets/Resources의 파일, 기존 이미지 경로, 빌더, 런타임 로더는 이동·복사·삭제·수정하지 않는다.
12. 실제 폴더 없이 .meta만 남은 경로, Resources.Load 의존성, 기존 이미지 가이드의 Assets/Resources 출력, 빌더 미구현 상태를 별도 마이그레이션 항목으로 보고한다.
13. 생성 후 두 루트의 폴더 구조, .meta GUID 중복, 작업 범위 밖 변경 여부를 검증한다.

Output:
- 판정된 contentDomain과 SO 소유권 근거
- 생성한 폴더 경로
- 생성한 Assets/ImagesGenerated 도메인 및 artifact type 경로
- 기존 GUID를 보존하며 완성한 폴더 경로
- 이미 유효하여 재사용한 폴더 경로
- 생성한 .meta 경로와 GUID 중복 검사 결과
- 불완전한 기존 scaffold 경로
- 빌더 및 런타임 마이그레이션 필요 항목
- 기존 이미지 가이드 및 프로젝트 대상 경로 마이그레이션 필요 항목
- 최종 검증 결과

실패 시 Output:
- 생성하지 않은 경로
- 실패 사유
- 충돌하는 기존 도메인 또는 소유권
- 부족한 스키마, SO, 빌더 정보
- 유효하지 않거나 소유권이 불명확한 imageArtifactType
- 결정이 필요한 마이그레이션 항목

검증:
- 최종 구조는 Assets/Contents/{ContentDomain}/json 및 Assets/Contents/{ContentDomain}/so이다.
- 이미지 타입이 있으면 최종 구조는 Assets/ImagesGenerated/{ContentDomain}/{image_artifact_type}이다.
- ContentDomain은 PascalCase이고 json과 so는 lowercase sibling이다.
- image_artifact_type은 lowercase snake_case이다.
- 기존 .meta와 GUID가 보존되었다.
- placeholder 및 추가 하위 폴더가 없다.
- migrationApproved=false 상태에서 기존 데이터나 로더가 변경되지 않았다.
- 생성 이미지가 Assets/Resources 아래에 저장되지 않았다.
- AgentDocs 아래에 .meta가 생성되지 않았다.
```
