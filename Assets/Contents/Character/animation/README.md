# Character Animation Spec v2

`character-animation-profile-v2.schema.json` is the planning source of truth. The
Unity asset is generated from it; numeric clip timing remains in clips/cast data and
is not duplicated here.

Authoring requirements:

- Every player grade supplies `Idle`, `Move`, `BasicAttack`, and `Death`.
- A player that can receive attack-disabled CC also supplies `AttackDisabledCc`.
- Every equipped active skill has either a dedicated `skillId` entry or an explicit,
  reviewed fallback. IDs are executed equipment IDs, never slot indices.
- Locomotion and CC clips loop. Attacks, skills, and death are one-shot (death holds
  its terminal frame). Root motion is unsupported and must be false.
- Missing required slots or duplicate skill IDs fail materialization. Runtime still
  falls back to legacy directional clips and never cancels gameplay for missing art.
- Dedicated skill clips and code-driven body presenters are mutually exclusive.

Generation workflow:

1. Copy the example, assign stable IDs and existing `Assets/...anim` paths.
2. Reference it from character JSON with `animationProfileJsonPath`.
3. Run CharacterSO generation. The builder uses `CreateAsset`, persisted object
   references, synchronous import, and reload; it does not emit YAML.
4. Review the generated validation report before promotion.

Priority is `Death > AttackDisabledCc > SkillAction > BasicAttack > Move > Idle`.
Knockback/root alone does not select the CC animation.
