# PixelLab Animation Generation Legacy Audit Prompt

## Prompt

```text
이미 존재하는 PixelLab animation generation evidence만 읽기 전용으로 감사해줘. 생성은 수행하지 마.

필수 참조:
- AgentDocs/planning-guides/content/generated-media/GeneratedMediaLegacyV1CompatibilityGuide.md
- AgentDocs/planning-guides/content/generated-media/PixelLabAnimationPipelineGuide.md

Input: legacyEvidencePaths / expectedSchema / expectedIdentity / expectedStoredHashes
작업: 저장된 reference/sheet/frame/provider-ref/index/hash 관계만 검증한다. provider 접근, 생성·재시도·비용, 새 record/index, 다운로드·추출, 원본 수정을 금지한다.
Output: status=audited / mode=read_only_legacy_audit / findings / hashVerification / mutationsPerformed=false / providerCalled=false / costIncurred=false
실패: legacy registry만 사용하며 실행·수정 요청은 legacy_execution_forbidden으로 종료한다.
```
