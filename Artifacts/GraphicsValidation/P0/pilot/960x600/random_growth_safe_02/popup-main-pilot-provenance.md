# B0 Safe Growth `.02` popup_main pilot provenance

- Status: `original_visual_approved / runtime_validation_pending`
- Canonical node: `node.act1.random_growth.02.windworn_sword_marks.intro`
- Runtime candidate: `Assets/ImagesGenerated/Stage/popup_main/node.act1.random_growth.02.windworn_sword_marks.intro.main.png`
- Generation mode: Codex built-in ImageGen, new generation followed by one targeted correction
- Generated source: 1086×1448 RGB PNG
- Runtime normalization: proportional resize only, 1086×1448 → 960×1280 RGB PNG; crop 0; padding 0
- Generated source SHA-256: `586355f0b49cee118b39d4b14481ca4f72de7beceede441d72933ed895c276e2`
- Runtime candidate SHA-256: `dc928d2cee3dcfa5f1a727e47ae64caf3984afa8f8383f3f6bc6c9417baf1c9a`
- 573×764 mask preview SHA-256: `27cf5099abbfdd4a6b6eeaa789bbaacdf76d612c048b83b61ab0ffc0aae8e409`
- 144×192 contact evidence SHA-256: `baf030b13602545a8ab52782f5a6847e4722aca4d6fe211e707ae10ba8d9e799`

## Initial prompt

```text
Use case: historical-scene
Asset type: ProjectBS game Stage vertical popup_main illustration, one canonical pilot
Primary request: Create a quiet Joseon-folktale scene titled conceptually “Sword Forms Left in the Wind”: an empty field of tall silver grass bending together in a steady crosswind. On the exposed dry earth, several old wooden-sword practice marks appear disconnected at close range, but from this pulled-back viewpoint they align into one coherent flowing sword path. The scene must communicate observation, imitation, and learning without combat or reward.
Scene/backdrop: broad open silver-grass field on a gentle low ridge in early morning; no buildings, shrine, road, mountain-path corridor, bowls, forge, people, animals, weapons, treasure, UI.
Subject: the single focal subject is the relationship between wind-bent grass and the sequence of weathered wooden-sword marks in soil; the marks should read as human practice traces, not letters, runes, arrows, blood, or magical glyphs.
Style/medium: refined Korean ink-and-light-color painting on aged hanji; restrained brushwork, natural mineral washes, hand-painted texture, historically grounded Joseon folktale atmosphere; consistent with a serious narrative game, not anime, not photorealistic.
Composition/framing: portrait 3:4. Camera pulled back and slightly elevated. The flowing sequence begins in the middle foreground and arcs gently through the grass toward the upper middle, forming one readable movement line without becoming a literal road. No central display pedestal. Keep the lowest 28% visually quiet with pale hanji, sparse grass tips, and no essential focal detail for popup body/crop safety. Preserve generous breathing space.
Lighting/mood: bright overcast early morning, calm and lucid, contemplative rather than ominous.
Color palette: bright warm gray-white, pale blue-gray, dry straw green, muted earth beige; no red, no orange, no gold glow, no fire; moderate value separation.
Materials/textures: fibrous hanji, dry soil, soft silver-grass plumes, worn shallow practice cuts and scuffs.
Constraints: exactly no people or human silhouettes; no text, numbers, seals, calligraphy, watermark, UI, icon, border. The practice marks must remain recognizable after reduction to a small popup thumbnail. No rarity, recommendation, treasure, magic, victory, battle, cost, blood, relic, or reward impression.
Avoid: shrine architecture or eaves; crowded mountain trail; three bowls; smithy or bell; characters posing on a path; deep fog; night; flames; red accents; black-cloth imagery; dramatic fantasy lighting; photographic depth of field.
```

## Correction prompt

```text
Correct the composition so it no longer reads as a road or trail. Remove the continuous cleared-earth corridor from foreground to horizon. Restore silver grass and small irregular earth patches across that corridor, leaving only a sequence of separate, weathered wooden-sword practice scuffs and shallow curved cuts visible through gaps in the grass. From this slightly elevated pulled-back viewpoint, those disconnected marks should align into one flowing sword-form rhythm, but there must be no walkable path and no continuous central lane.

Preserve the portrait composition, empty field, early bright overcast light, pale warm gray/pale blue-gray/dry straw green palette, Korean ink-and-light-color painting on aged hanji, calm observational mood, lowest 28% quiet UI space, and zero red. Practice marks must remain physical scuffs rather than writing, arrows, runes, magic, blood, or calligraphy. Avoid roads, trails, corridors, footprints, people, buildings, shrines, bowls, forges, weapons, treasure, text, numbers, seals, watermark, UI, fire, night, fog, and reward lighting.
```

## Art-direction receipt

- PASS: no person, combat, cost, relic, reward, rarity, recommendation, text, red accent, fire, night, or fog focal.
- PASS: bright gray-white, pale blue-gray, and dry grass palette remains distinct from shrine, episode04_1 trail, three-bowls, and Smithy references.
- PASS: wind-bent grass and separate soil scuffs form the only visual idea.
- PASS: lower 28% contains no essential focal object.
- PASS: focal rhythm survives 573×764 mask and 144×192 contact reduction.
- CORRECTED: the first generation read as a continuous mountain path; one targeted correction restored grass across the center and broke the marks into separate practice traces.
- Remaining runtime risk: at very small display size the aligned traces may still be read as a route line. G2 UI composite must verify that title/body context resolves this without adding a route icon or highlight.
- Deferred: Unity importer/meta, UI composition, WebGL max-size/format, and 1920×1080 / 960×600 / 2560×1440 runtime approval.
