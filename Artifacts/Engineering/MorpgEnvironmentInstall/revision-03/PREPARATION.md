# Revision03 preparation — waiting for geometry authority

The canonical profile and28 reservation data are byte-identical to the task-start snapshot. No horizontal coordinates have been invented or applied.

Prepared production changes: map envelope and background dimensions/center now derive from profile.mapBounds; BattleMapBoundsContext has a Rect activation overload; optional cameraOrthographicSize (default3 preserves existing data); optional per-prop/decor flipX and rotation preserve authored opaque contact placement; per-instance visual sizes already support deterministic scale variation without collider scaling. Strict schema validation keeps unknown/missing fields rejected. Universal structural-overlap checks and data-driven camera-envelope validation are added. Grid reachability sampling no longer assumes the old vertical y range.

Validation: actual Unity compiler0 errors/31 existing warnings; production geometry12/12; live/environment/sorting26/26; git diff --check passes. No Unity GUI, staging, commit or push.

Required next input: the designer's revision03 horizontal-layout contract (map, zone polygons/cores, anchors/camera, structural inventory/coordinates and visual density rules). A text clarification is pending. Current shared files contain only prior environment and Z3 correction contracts; no horizontal revision contract was found.

Queued authorized work AFTER environment revision03 is completed/frozen: implement missing Dash transition against /private/tmp/projectbs-current-byeori-morpg-zone-transition-dash-addendum.txt SHA d7ed28cd4f31ae36c6b9d14ab1fb7c398af0874f61fcb0090929ab08f0724f92. No Dash implementation edits made yet, honoring the required serialization. Preserve clear-fixed camera, hidden atomic cut, body-only Dash presentation, alpha checkpoint, .36 entry/.22 fade/.16 Idle/.20 post-unlock wave delay, rollback and safe fallback.
