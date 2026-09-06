"""Compile the current Unity assembly with installed Unity Roslyn, without opening Unity."""
from pathlib import Path
import subprocess, tempfile, sys
root = Path('/Users/pvenus/ProjectBS')
source = root / 'Library/Bee/artifacts/200b0aEDbg.dag/Assembly-CSharp.rsp'
out = Path(tempfile.mkdtemp(prefix='projectbs-morpg-compile.', dir='/private/tmp'))
lines = [x for x in source.read_text(encoding='utf-8-sig').splitlines()
         if not x.startswith(('-out:', '-refout:', '-analyzer:'))]
sources = {x.strip('"') for x in lines if x.strip('"').endswith('.cs')}
for path in (root / 'Assets/Scripts').rglob('*.cs'):
    relative = path.relative_to(root)
    if relative.parts[2] == 'Progression': continue  # separate asmdef, already referenced by Unity
    if str(relative) not in sources: lines.append('"' + str(relative) + '"')
if '--baseline' in sys.argv:
    baseline = root / 'Artifacts/Engineering/MorpgIntegration/before'
    additions = ('BattleMorpgLiveRoute.cs', 'MorpgOwnedObject.cs')
    replaced = []
    for line in lines:
        path = Path(line.strip('"'))
        if path.name in additions: continue
        if path.suffix == '.cs' and (baseline / path.name).exists():
            line = '"' + str(baseline / path.name) + '"'
        replaced.append(line)
    lines = replaced
lines.append('-out:' + str(out / 'Assembly-CSharp.dll'))
rsp = out / 'compile.rsp'
rsp.write_text('\n'.join(lines))
unity = Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting')
result = subprocess.run([str(unity / 'NetCoreRuntime/dotnet'), str(unity / 'DotNetSdkRoslyn/csc.dll'), '@' + str(rsp)], cwd=root, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
log = root / ('Artifacts/Engineering/MorpgIntegration/build-baseline.log' if '--baseline' in sys.argv else 'Artifacts/Engineering/MorpgIntegration/build.log')
log.write_text(result.stdout)
print(f'compiler exit={result.returncode}; errors={result.stdout.count(": error CS")}; warnings={result.stdout.count(": warning CS")}; log={log}')
sys.exit(result.returncode)
