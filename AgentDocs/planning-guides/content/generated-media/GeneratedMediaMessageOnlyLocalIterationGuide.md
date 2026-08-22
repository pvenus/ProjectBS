# Generated Media Message-Only Local Iteration Guide

## 1. Policy identity and selection

`generated_media_message_only_local_iteration_v1` is the normative,
forward-going override for an explicitly requested `local_unpublished` media
iteration. The requester selects it in a self-contained message that includes
all instructions, inputs, existing attachment or path references, provider
limits, and safety constraints needed for that iteration. A role must not load
thread history or infer inherited planning, routing, prompt, evaluation, or
publication context.

Selection of this policy does not alter or delete historical planning,
routing, prompt, generation, preservation, evaluation, receipt, index, or
publication artifacts. Existing strict, promotable, fast-preview, and
deterministic artifact workflows retain their published meanings. This policy
applies only to a new local iteration whose accepted output has not entered a
separately authorized publication process.

## 2. Closed three-message workflow

The complete workflow is exactly:

1. **Planning message** — the requester supplies one self-contained message.
2. **Generation** — the generation role uses that message and any attached
   image or exact existing media path, performs only the bounded generation
   requested, persists the resulting media output, and returns its output
   image path in chat.
3. **Chat evaluation** — the evaluator opens the returned media from its
   attachment or exact path and returns chat-only `PASS | FAIL`.

Routing is message forwarding only. It forwards the self-contained requester
message and attachment/path references without creating, normalizing,
projecting, hashing, indexing, or publishing a routing artifact or prompt
artifact. It must not expand the flow with a planning, authoring, preservation,
package, evaluation-record, or Git stage.

## 3. Artifact prohibition and media persistence

During an iteration, do not create planning handoffs, routing records, prompt
records, evaluation records, receipts, manifests, packages, indexes, snapshots,
request records, profile projections, lineage chains, JCS payloads, or metadata
sidecars. Do not require snapshot/request/profile/index/hash chains or raw Git
BLOB validation. Do not perform registry/schema work, Git publication, or a
full suite (`Generated Media` contract-suite) run.

Only actual media outputs produced by generation are persisted. Input images
are passed as attachments or exact existing paths. The generation response is
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
2. Use only the self-contained requester message and its attachments/paths.
3. Forward the message once to generation; create no route or prompt artifact.
4. Return the generated media path to chat.
5. Forward that path once for chat-only `PASS | FAIL` evaluation.
6. Stop after the evaluation message; persist only actual media outputs.
7. Require separate authorization before any publication or project copy.
