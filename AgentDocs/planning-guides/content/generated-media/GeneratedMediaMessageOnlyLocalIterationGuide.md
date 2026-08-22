# Generated Media Message-Only Local Iteration Guide

## 1. Policy identity and selection

`generated_media_message_only_local_iteration_v1` is the normative,
forward-going override for an explicitly requested `local_unpublished` media
iteration. The planning stage retains exactly one complete authoritative
planning document. The requester extracts the facts required for generation
from that document and sends a self-contained generation chat message. No role
may rely on inherited chat context.

The planning document remains owned by the planning stage. It is never
transferred, referenced, opened, or consumed by routing, generation, or
evaluation. Those stages receive only their self-contained chat inputs.

Selection of this policy does not alter or delete historical planning,
routing, prompt, generation, preservation, evaluation, receipt, index, or
publication artifacts. Existing strict, promotable, fast-preview, and
deterministic artifact workflows retain their published meanings. This policy
applies only to a new local iteration whose accepted output has not entered a
separately authorized publication process.

## 2. Closed three-message workflow

The complete workflow is exactly:

1. **Planning message** — the planning stage retains exactly one complete
   authoritative planning document. The requester extracts the required facts
   and sends one self-contained generation chat message without attaching,
   naming, linking, or quoting the planning file.
2. **Generation** — the generation message contains all identity locks, allowed
   deltas, prohibitions, output settings, submit limit, and the reference image
   attachment or exact existing path. The generation role uses only that
   message, persists the resulting media output, and returns its output image
   path or paths in chat.
3. **Chat evaluation** — the evaluator receives only the output image path or
   paths plus explicit evaluation gates, opens the media, and returns chat-only
   `PASS | FAIL`.

Routing is message forwarding only. It forwards the self-contained generation
message and attachment/path references without opening or referencing the
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
attachments or exact existing paths. The generation response is
the output image path plus only the minimum factual provider counters required
by the requester; it is not a receipt artifact. The evaluation response is
chat-only `PASS | FAIL`; it is not an evaluation record and does not confer
publication, preservation, promotion, or project-copy eligibility.

## 4. Safety and iteration boundary

This override removes artifact ceremony, not the bounds of the authenticated
request. Generation must obey its explicit submit/retry limit, provider safety
rules, and exact input/path scope. It must not overwrite an existing media
output unless the requester expressly authorized replacement. A missing
self-contained instruction or unavailable referenced attachment/path is
reported in chat without fabricating metadata or starting an artifact pipeline.

An accepted local result remains `local_unpublished`. Publishing, preserving,
promoting, or copying that result requires a new, explicit authorization and a
separate applicable workflow. Selection of this policy never silently
publishes, preserves, promotes, copies, or imports media.

## 5. Coordinator checklist

1. Confirm the requester explicitly selected message-only local iteration.
2. Retain exactly one complete authoritative document inside planning.
3. Extract all generation facts into one self-contained chat message; never
   transfer, reference, open, or consume the planning file downstream.
4. Include identity locks, allowed deltas, prohibitions, output settings,
   submit limit, and one reference attachment/path in the generation message.
5. Forward the message once to generation; create no route or prompt artifact.
6. Return the generated media path or paths to chat.
7. Forward only those paths and explicit gates for chat-only `PASS | FAIL`.
8. Stop after evaluation; retain only the planning document and actual media.
9. Require separate authorization before any publication or project copy.
