# Content Folder Structure Guide

## Master Concept Reference

Before using this document, read and apply:

```text
AgentDocs/planning-guides/common/DisignMasterConcept_rule.md
```

The master concept takes precedence when content data includes names,
descriptions, visuals, narrative, cultural references, or other game design
facts. This folder guide defines storage and generation boundaries only; it does
not create an exception to the master concept.

## 1. Purpose

`Assets/Contents` is the canonical Unity project root for content JSON and the
ScriptableObject assets generated from that JSON. `Assets/ImagesGenerated` is
the canonical Unity project root for generated images promoted for project use.

Generated images must not be stored under `Assets/Resources`.

The folder is intentionally expanded one content domain at a time. The current
`Battle`, `Bless`, `Effect`, and `Skill` entries are an initial scaffold, not a
complete domain list and not an exception to this guide.

This guide defines the target structure for content added from now on. Existing
paths are normalized only through an explicitly requested migration; folder
creation alone must not move or delete existing files.

## 2. Canonical Domain Layout

Every active content domain has exactly these sibling folders:

```text
Assets/Contents/{ContentDomain}/
├── json/
└── so/
```

| Folder | Responsibility | Primary file types |
| --- | --- | --- |
| `json` | Authoring source and builder input | `*.json` |
| `so` | Unity ScriptableObject output generated from validated JSON | `*.asset` |

Unity `.meta` files accompany folders and Unity assets. They are not content
payloads.

The intended expansion shape is:

```text
Assets/Contents/
├── Battle/
│   ├── json/
│   └── so/
├── Bless/
│   ├── json/
│   └── so/
├── Effect/
│   ├── json/
│   └── so/
└── Skill/
    ├── json/
    └── so/
```

Do not create all possible domains in advance. Create or complete a domain only
when a content task requires it.

### 2.1 Generated image layout

Generated images use the same owning `ContentDomain` as JSON and SO data, then
one lower `snake_case` folder describing the image artifact type:

```text
Assets/ImagesGenerated/{ContentDomain}/{imageArtifactType}/
```

Default file path:

```text
Assets/ImagesGenerated/{ContentDomain}/{imageArtifactType}/{contentId}.{imageRole}.png
```

Examples:

```text
Assets/ImagesGenerated/Skill/icon/{skillId}.icon.png
Assets/ImagesGenerated/Skill/animation/{skillId}.animation.png
Assets/ImagesGenerated/Item/icon/{itemId}.icon.png
Assets/ImagesGenerated/Battle/background/{battleId}.background.png
Assets/ImagesGenerated/Stage/popup_main/{eventId}.main.png
Assets/ImagesGenerated/Character/portrait/{characterId}.portrait.png
```

Create only the artifact-type folders required by actual generated images. Do
not pre-create every example.

## 3. Naming Rules

### 3.1 Content domain

`ContentDomain` is the owner of a content data type and its primary SO class.

Rules:

- use singular PascalCase, matching the current Unity content convention;
- match `^[A-Z][A-Za-z0-9]*$`;
- use a stable ownership name such as `Battle`, `Bless`, `Effect`, or `Skill`;
- do not use a UI screen, shop section, rarity, chapter, team, or temporary
  production label as a domain;
- do not create a second domain merely because one content type references
  another.

Invalid examples:

```text
Temp
NewContent
Final
Shop01
RareItems
TeamA
```

A subtype stays in the domain that owns its SO. For example, a strategic skill
belongs to `Skill`, while the item that activates it belongs to `Item`. The item
stores the skill ID; it does not own a copied skill payload.

If ownership is ambiguous, do not create a folder. Report the candidate SO
types and request an ownership decision.

### 3.2 Fixed child names

The child folder names are always lowercase and exact:

```text
json
so
```

Do not substitute `Json`, `JSON`, `SO`, `Generated`, `Data`, or `Resources`.

### 3.3 Content files

The default paths are:

```text
Assets/Contents/{ContentDomain}/json/{contentId}.json
Assets/Contents/{ContentDomain}/so/{contentId}.asset
```

`contentId` is the stable canonical ID defined by the domain guide. The JSON
filename and primary SO filename must use the same ID exactly unless an existing
builder contract explicitly requires a different, documented asset filename.

When one JSON produces supporting SO assets, keep them under the same `so`
folder and use the domain builder's stable suffix rule, for example:

```text
{contentId}.{componentRole}.asset
```

Do not invent suffixes during generation.

### 3.4 Generated image names

`imageArtifactType` describes the production use of the image and must:

- use lowercase `snake_case`;
- match `^[a-z][a-z0-9_]*$`;
- use a stable term such as `icon`, `animation`, `background`, `popup_main`, or
  `portrait`;
- not encode approval state, attempt number, generator name, date, assignee, or
  temporary workflow state.

`imageRole` is the stable filename suffix defined by the domain image guide. It
must not be invented per attempt. The canonical image filename uses the same
`contentId` as its owning content record.

Use lowercase `.png` by default for generated project images. Another format is
allowed only when a domain-specific guide documents its Unity import and runtime
requirements.

## 4. JSON Ownership Rules

1. JSON is the canonical authored input for generated content SOs.
2. One canonical content ID owns one primary JSON unless the domain schema
   explicitly defines an aggregate or split-file contract.
3. JSON must conform to the domain schema before SO generation starts.
4. Cross-domain relations use stable IDs rather than embedded copies of another
   domain's complete data.
5. JSON must not store generated Unity GUIDs as content identity.
6. A JSON-only task must not create or modify `.asset` files.
7. Do not create placeholder, empty, or example JSON merely to make a folder
   visible in Git.

Domain schema guides define fields and ID patterns. They do not override the
`json` and `so` storage boundary.

## 5. SO Generation Rules

1. Generate an SO only from a schema-valid JSON in the same content domain.
2. Treat generated SO fields as build output. Correct the JSON or builder rather
   than hand-editing generated values.
3. Preserve the existing `.meta` file and GUID when updating an SO.
4. Give every new folder and asset a new unique Unity GUID. Never copy another
   path's `.meta` file.
5. Resolve cross-domain IDs through the approved builder or resolver. Do not
   duplicate the referenced SO under the consumer's domain.
6. Stop without partial output when a required ID cannot be resolved.
7. Do not place JSON in `so`, or `.asset` files in `json`.
8. Do not generate images, audio, localization, or unrelated resources into
   `Assets/Contents`. Generated images belong under `Assets/ImagesGenerated`.

## 6. Generated Image Storage and Promotion Rules

1. Every image generation, download, evaluation, promotion, and generated-image
   consumer guide or prompt must explicitly reference this file by its exact
   project-relative path. Missing this reference is
   `missing_content_storage_guide_reference` and blocks file output.
2. Read this guide before applying the domain image guide. If their storage
   paths conflict, this guide's `Assets/ImagesGenerated` contract wins and the
   conflicting domain document must be reported for correction.
3. `Assets/ImagesGenerated` contains project-use generated images, not JSON or
   ScriptableObject assets.
4. The image `ContentDomain` must match the owner of the related content ID.
   An item icon belongs to `Item`; a skill icon belongs to `Skill`.
5. Generation candidates, failed attempts, evaluation reports, previews,
   contact sheets, masks, hashes, and temporary edits must remain in the
   approved evaluation workspace. They do not belong in
   `Assets/ImagesGenerated`.
6. Copy or save an image into `Assets/ImagesGenerated` only after it satisfies
   the domain image guide and its required approval or promotion gate.
7. Preserve the existing image `.meta` and GUID when updating an approved image.
   A new canonical image receives a new unique Unity GUID.
8. When `allowOverwrite=false`, stop if the canonical image or its `.meta`
   already exists.
9. Do not append attempt suffixes such as `_v2`, `_final`, `_retry`, or
   `_approved` to canonical filenames. Evaluation workspaces own attempt names.
10. Do not copy a reference image into this root and present it as generated
   output.
11. Validate dimensions, alpha, color mode, sprite import settings, compression,
   and naming with the domain image guide before promotion.
12. Generated images are outside Unity's `Resources` folders. Runtime consumers
    must use serialized references or another approved loading strategy; do not
    assume `Resources.Load` will find them.

## 7. Folder Creation and Expansion Procedure

For one requested domain:

1. Read the domain's schema, SO, builder, and runtime loading rules.
2. Confirm that the requested name represents the owning SO type.
3. Search `Assets/Contents` for an existing folder or orphan folder `.meta` with
   the same domain name.
4. If only `{ContentDomain}.meta` exists, create the corresponding physical
   directory and preserve that `.meta` GUID.
5. Create missing `json` and `so` sibling folders beneath the domain.
6. If the domain produces generated images, create
   `Assets/ImagesGenerated/{ContentDomain}` and only the requested
   `{imageArtifactType}` folders. If it does not, do not create an empty image
   domain.
7. Let Unity create new folder `.meta` files when Unity is available. Otherwise,
   create valid folder `.meta` files with unique GUIDs and validate them before
   commit.
8. Leave existing files and `.meta` files unchanged unless migration is
   explicitly approved.
9. Do not create additional subtype folders, placeholder files, `.gitkeep`, or
   sample content.
10. Re-run the same operation safely: existing valid folders must be reused, not
   recreated. Folder expansion must be idempotent.
11. Report created, reused, incomplete, and blocked paths for both roots.

A folder `.meta` without its physical folder is an incomplete scaffold. It can
be preserved temporarily, but a domain is not ready for content generation
until the physical domain, `json`, and `so` folders all exist.

## 8. Existing Scaffold Normalization

The current repository can contain initial entries such as:

```text
Assets/Contents/Battle.meta
Assets/Contents/Bless.meta
Assets/Contents/Effect.meta
Assets/Contents/Effect/Generated.meta
Assets/Contents/Skill.meta
```

Apply these rules while expanding:

- preserve an existing domain folder GUID;
- create the missing physical folder when activating that domain;
- add `json` and `so` as siblings;
- do not treat `Generated` as a replacement for `so`;
- do not rename or delete `Generated` until its files, references, builder
  output, and GUID impact have been audited in a migration task;
- after an approved migration, all generated SO output for that domain must use
  `so` and the obsolete folder must be removed only after reference validation.

## 9. Prohibited Structures

Do not create:

```text
Assets/Contents/json/{ContentDomain}
Assets/Contents/so/{ContentDomain}
Assets/Contents/{ContentDomain}/Json
Assets/Contents/{ContentDomain}/SO
Assets/Contents/{ContentDomain}/Generated
Assets/Contents/{ContentDomain}/{Subtype}/json
Assets/Contents/{ContentDomain}/json/so
Assets/Contents/{ContentDomain}/so/json
Assets/Contents/{ContentDomain}/images
Assets/Contents/{ContentDomain}/icon
Assets/Resources/{ContentDomain}/generated_images
Assets/ImagesGenerated/json/{ContentDomain}
Assets/ImagesGenerated/{ContentDomain}/Resources
Assets/ImagesGenerated/{ContentDomain}/temp
Assets/ImagesGenerated/{ContentDomain}/approved
```

An existing pre-guide path is migration input, not permission to reproduce that
shape in new domains.

## 10. Runtime and Migration Boundary

Some existing builders, prompts, image guides, and runtime code still reference
image or SO paths under `Assets/Resources`, or use `Resources.Load` and
`Resources.LoadAll`. Unity does not load assets from `Assets/Contents` or
`Assets/ImagesGenerated` through the Resources API.

Therefore, a domain migration must audit together:

```text
task prompts and planning guides
JSON schema and canonical IDs
editor builders and default input/output paths
image generation, evaluation, download, and promotion prompts
image importer settings and Sprite lookup behavior
Resources.Load / Resources.LoadAll calls
serialized references and Unity GUIDs
cross-domain resolvers
validation tests and build inclusion
```

Folder creation does not authorize moving, copying, or deleting existing
`Assets/Resources` data. Do not maintain two writable canonical copies. A
migration must name one source of truth, update all consumers, validate the
build, and define rollback.

New image guides and prompts must use `Assets/ImagesGenerated`. Existing
image-specific documents that still name an `Assets/Resources` output are
migration inputs and must be updated together with their Unity consumers before
the old path is removed.

## 11. AgentDocs Boundary

Keep operational documents outside Unity import scope:

```text
AgentDocs/planning-guides/content/ContentFolderStructureGuide.md
AgentDocs/task-prompts/content/ContentFolderCreatePrompt.md
```

Do not create Unity `.meta` files under `AgentDocs`.

## 12. Validation Checklist

- [ ] The domain name is singular PascalCase and has clear SO ownership.
- [ ] The physical domain folder exists; it is not represented by `.meta` only.
- [ ] `json` and `so` exist as lowercase sibling folders.
- [ ] Existing domain and asset GUIDs are preserved.
- [ ] New `.meta` GUIDs are unique.
- [ ] JSON filenames and primary SO filenames follow canonical IDs.
- [ ] Cross-domain data is referenced by ID rather than copied.
- [ ] Generated image paths use
      `Assets/ImagesGenerated/{ContentDomain}/{imageArtifactType}`.
- [ ] Canonical image filenames use the owning content ID and stable role.
- [ ] No generated image was written under `Assets/Resources`.
- [ ] Evaluation candidates and reports were not promoted as project images.
- [ ] No placeholder files or unapproved subtype folders were created.
- [ ] No existing `Assets/Resources` data was silently moved or duplicated.
- [ ] Runtime and builder migration is handled separately when required.
- [ ] No `.meta` file was added under `AgentDocs`.
- [ ] Every related image workflow document explicitly references this guide.

## 13. Related Prompt

```text
AgentDocs/task-prompts/content/ContentFolderCreatePrompt.md
```
