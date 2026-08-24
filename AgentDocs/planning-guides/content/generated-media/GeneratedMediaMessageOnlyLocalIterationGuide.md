# Generated Media Message-Only Local Iteration Guide

## 1. Policy identity and selection

`generated_media_message_only_local_iteration_v1` is the normative,
forward-going handoff rule for skill animation generation and for an explicitly
requested `local_unpublished` media iteration. The planning stage may retain its
own authoritative document, but every downstream handoff is one self-contained
chat message. No downstream role may require that file, a generated record file,
or inherited chat context.

The planning document remains owned by the planning stage. It is never
transferred, referenced, opened, or consumed by routing, generation, or
evaluation. Those stages receive only their self-contained chat inputs.

Selection of this policy does not alter or delete historical planning,
routing, prompt, generation, preservation, evaluation, receipt, index, or
publication artifacts. Existing strict, promotable, fast-preview, and
deterministic artifact workflows retain their published meanings. This policy
applies only to a new local iteration whose accepted output has not entered a
separately authorized publication process.

## 2. Closed four-stage message workflow

The skill-animation workflow is exactly:

1. **Skill planning handoff** — planning sends one self-contained chat message
   containing skill identity, gameplay meaning, motion, frame count/order,
   timing/loop, canvas, effect origin, visual locks, and prohibitions.
2. **Image-generation request** — the receiving chat turns that message into one
   complete ImageGen request. It does not ask for or pass a planning, routing,
   prompt, generation, receipt, manifest, or package file.
3. **Animation generation** — ImageGen creates the approved animation. Required
   reference media is attached directly to the message; a repository path may be
   used only to open an already existing reference image, never as a handoff.
4. **Image delivery** — the generated GIF and/or ordered frame images themselves
   are attached or rendered in chat with a compact factual summary. A later chat
   receives the media attachment plus a self-contained message, not an output
   path, record path, hash manifest, or package path.

Routing is message forwarding only. It forwards the self-contained generation
message and media attachments without opening or referencing the
planning document and without creating, normalizing,
projecting, hashing, indexing, or publishing a routing artifact or prompt
artifact. It must not expand the flow with a planning, authoring, preservation,
package, evaluation-record, or Git stage.

## 3. Artifact prohibition and media persistence

The one authoritative planning document is the only planning-stage file. Do
not create any inter-stage planning handoff, routing record, prompt record,
evaluation record, receipt, manifest, package, index, snapshot, request record,
profile projection, lineage chain, JCS payload, or metadata sidecar. Do not
require snapshot/request/profile/index/hash chains or raw Git BLOB validation.
Do not perform registry/schema work, Git publication, or a full suite
(`Generated Media` contract-suite) run.

Outside the planning stage's one authoritative document, only actual media
outputs produced by generation are persisted. Input images are passed as
attachments; an exact existing path is allowed only to open a reference already
available in the same workspace. The generation response delivers the actual
media in chat plus only the minimum facts needed to understand it; it is not a
receipt artifact. The evaluation response is
chat-only `PASS | FAIL`; it is not an evaluation record and does not confer
publication, preservation, promotion, or project-copy eligibility.

## 4. Safety and iteration boundary

This override removes artifact ceremony, not the bounds of the authenticated
request. Generation must obey its explicit submit/retry limit, provider safety
rules, and exact input/path scope. It must not overwrite an existing media
output unless the requester expressly authorized replacement. A missing
self-contained instruction or unavailable referenced attachment is
reported in chat without fabricating metadata or starting an artifact pipeline.

No stage creates, edits, copies, normalizes, validates, or deletes Unity `.meta`
files. Unity owns importer metadata. Media delivery and later project import
leave every existing `.meta` byte unchanged and never include `.meta` files in
the chat handoff.

An accepted local result remains `local_unpublished`. Publishing, preserving,
promoting, or copying that result requires a new, explicit authorization and a
separate applicable workflow. Selection of this policy never silently
publishes, preserves, promotes, copies, or imports media.

## 5. Coordinator checklist

1. Use this policy by default for skill animation; otherwise confirm the
   requester selected message-only local iteration.
2. Retain exactly one complete authoritative document inside planning.
3. Extract all generation facts into one self-contained chat message; never
   transfer, reference, open, or consume the planning file downstream.
4. Include identity locks, allowed deltas, prohibitions, output settings,
   submit limit, and required reference attachments in the generation message.
5. Forward the message once to generation; create no route or prompt artifact.
6. Attach or render the generated media itself in chat.
7. Forward only the actual media attachment and a self-contained message; do not
   relay artifact paths or metadata files.
8. Stop after evaluation; retain only the planning document and actual media.
9. Require separate authorization before any publication or project copy.
