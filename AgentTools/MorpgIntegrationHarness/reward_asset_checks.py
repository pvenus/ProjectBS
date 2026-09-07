from pathlib import Path
import hashlib,json,struct
root=Path('/Users/pvenus/ProjectBS')
manifest=json.loads((root/'Artifacts/GraphicsRemediation/RewardAbsorbPresentation/selected/revision-01-exact2/manifest.json').read_text())
for asset in manifest['assets']:
    p=root/'Assets/Resources/battle/morpg/rewards'/(asset['id']+'.png')
    blob=p.read_bytes()
    assert hashlib.sha256(blob).hexdigest()==asset['runtimeCandidate']['sha256']
    assert blob[:8]==b'\x89PNG\r\n\x1a\n'
    assert struct.unpack('>II',blob[16:24])==(128,128) and blob[25]==6
    meta=Path(str(p)+'.meta').read_text()
    for token in ('spriteMode: 1','spritePixelsToUnits: 100','enableMipMap: 0','filterMode: 1','wrapU: 1','wrapV: 1','textureCompression: 0'):
        assert token in meta,(p,token)
    print('PASS selected-byte-exact-rgba-sprite-import '+asset['id'])
route=(root/'Assets/Scripts/Battle/Morpg/BattleMorpgLiveRoute.cs').read_text()
on_death=route.split('private void OnDied(')[1].split('private bool TryCommitArrivedReward(')[0]
assert 'delivery.TryReserve(' in on_death and 'ledger.TryCredit(' not in on_death
assert 'transitionView.Tick(delta,delivery.PendingCount==0' in route and 'delivery.PendingCount > 0) return;' in route
assert 'session?.BattleSO?.BattleId != BattleMorpgDefinitionValidator.BattleId' in route
view=(root/'Assets/Scripts/Battle/Morpg/MorpgRewardHudMono.cs').read_text()
for token in ('RenderMode.ScreenSpaceOverlay','goldAnchor.position','xpAnchor.position','Camera.main.WorldToScreenPoint','/ .30f','/ .12f','coalesceAge > .10f','goldSprite = Resources.Load<Sprite>','xpSprite = Resources.Load<Sprite>'):
    assert token in view,token
assert 'Time.unscaled' not in view and 'Time.unscaled' not in (root/'Assets/Scripts/Battle/Morpg/MorpgRewardDeliveryQueue.cs').read_text()
print('PASS exact-gate-reservation-only-death-settlement-barrier-real-HUD-targets-scaled-clock')
