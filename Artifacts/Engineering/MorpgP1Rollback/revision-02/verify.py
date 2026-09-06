from pathlib import Path
import hashlib
import json
import subprocess
root = Path('/Users/pvenus/ProjectBS')
folder = Path(__file__).resolve().parent
manifest = json.loads((folder / 'manifest.json').read_text())
def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
checked = 0
for row in manifest['sourceChanges']:
    assert sha(root / row['path']) == row['sha256'], row['path']
    checked += 1
    if 'baseline' in row:
        assert sha(folder / row['baseline']) == row['baselineSha256'], row['baseline']
        checked += 1
for row in manifest['wholeP1Rollback']:
    if 'restoreFrom' in row:
        assert sha(folder / row['restoreFrom']) == row['restoreSha256'], row['restoreFrom']
        checked += 1
for row in manifest['preserved']:
    assert sha(root / row['path']) == row['sha256'], row['path']
    checked += 1
for row in manifest['evidence']:
    assert sha(folder / row['path']) == row['sha256'], row['path']
    checked += 1
shared = subprocess.check_output(['git', 'diff', '--', *manifest['sharedPaths']], cwd=root)
assert hashlib.sha256(shared).hexdigest() == manifest['sharedDiffSha256']
assert (folder / 'run-mono-1.log').read_bytes() == (folder / 'run-mono-2.log').read_bytes()
print(f'DRY_READBACK_PASS {checked}/{checked}; shared diff preserved; two-run diff0; restoreExecuted=false')
