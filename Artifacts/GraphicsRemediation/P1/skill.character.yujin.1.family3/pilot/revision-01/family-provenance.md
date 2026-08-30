# A1 Yujin family3 provenance

- Strict style authority: promoted A0 multi_shot SHA `c63497f02308a9f51f1143f82375845c3d647d01add1c6c2c20cc52d08329a81`.
- Meaning-only references: current basic_attack and passive_1 canonicals; their black full-frame backgrounds were not accepted as style.
- Generator: built-in ImageGen, six independent calls, one per A/B/C candidate, correction calls zero.
- All raw candidates used a requested flat neutral matte `#D8D5CC`; generated files were copied without overwrite into this revision root.
- Basic selected style candidate: B. Passive selected style candidate: B.
- Alpha production1 used deterministic RGB-distance matte extraction and failed full-frame bbox.
- Allowed correction1 changed only the mask algorithm to border-connected neutral-matte flood fill with 1.2px feather and deterministic matte decontamination; RGB design and geometry were unchanged.
- Correction1 also failed the contract bbox/margin gate because generated edge gradients remained foreground. No threshold relaxation, further correction, Unity, importer, canonical promotion, staging, commit, or push occurred.
- Runtime-512 files are failed derivatives retained only as provenance and are not promotable.
