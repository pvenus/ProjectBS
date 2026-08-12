# PixelLab Character Prompt Legacy Audit Prompt

## Prompt

```text
이미 존재하는 PixelLab character legacy prompt evidence만 읽기 전용으로 감사해줘.

필수 참조:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabCharacterPipelineGuide.md

Input:
- legacyEvidencePaths: {explicit_project_relative_existing_paths}
- expectedSchema / expectedIdentity / expectedStoredHashes

작업:
1. immutable prompt/record/index bytes와 identity/hash 연결만 검증한다.
2. prompt를 작성·번역·수정하지 않고 provider/tool을 열거나 호출하지 않는다.
3. record/index/media 생성·수정, 다운로드, 비용 발생을 수행하지 않는다.

Output: status=audited / mode=read_only_legacy_audit / findings / hashVerification / mutationsPerformed=false / providerCalled=false / costIncurred=false
실패: legacy guide의 failure registry만 사용한다. 실행·수정 요청은 failureType=legacy_execution_forbidden으로 즉시 종료한다.
```
