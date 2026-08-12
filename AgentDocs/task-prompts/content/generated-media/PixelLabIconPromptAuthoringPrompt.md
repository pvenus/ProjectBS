# PixelLab Icon Prompt Legacy Audit Prompt

## Prompt

```text
이미 존재하는 PixelLab icon legacy prompt evidence만 읽기 전용으로 감사해줘.

필수 참조:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabIconPipelineGuide.md

Input: legacyEvidencePaths / expectedSchema / expectedIdentity / expectedStoredHashes
작업: immutable prompt/record/index의 identity와 hash만 검증한다. prompt 작성·변환, provider 호출, 새 record/index, 다운로드, 비용, 원본 수정을 금지한다.
Output: status=audited / mode=read_only_legacy_audit / findings / hashVerification / mutationsPerformed=false / providerCalled=false / costIncurred=false
실패: legacy registry만 사용하며 실행·수정 요청은 legacy_execution_forbidden으로 종료한다.
```
