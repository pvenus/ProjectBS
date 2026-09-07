from pathlib import Path
import subprocess,tempfile
root=Path(__file__).resolve().parents[2]
unity=Path('/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting')
out=Path(tempfile.mkdtemp(prefix='projectbs-aim-compile-',dir='/private/tmp'))
for name in ['Assembly-CSharp','Assembly-CSharp-Editor']:
 lines=[x for x in (root/f'Library/Bee/artifacts/200b0aEDbg.dag/{name}.rsp').read_text(encoding='utf-8-sig').splitlines() if not x.startswith(('-out:','-refout:','-analyzer:'))]
 if name=='Assembly-CSharp':
  sources={x.strip('"') for x in lines if x.strip('"').endswith('.cs')}
  for p in (root/'Assets/Scripts').rglob('*.cs'):
   rel=p.relative_to(root)
   if rel.parts[2]!='Progression' and str(rel) not in sources:lines.append('"'+str(rel)+'"')
 else:
  lines=['-r:"'+str(out/'Assembly-CSharp.dll')+'"' if x.startswith('-r:') and x.endswith('/Assembly-CSharp.ref.dll"') else x for x in lines]
 lines.append('-out:"'+str(out/(name+'.dll'))+'"');rsp=out/(name+'.rsp');rsp.write_text('\n'.join(lines))
 r=subprocess.run([str(unity/'NetCoreRuntime/dotnet'),str(unity/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp)],cwd=root,text=True,capture_output=True)
 log=root/'Artifacts/Engineering/SeojinManualControl'/('build.log' if name=='Assembly-CSharp' else 'editor-build.log');log.write_text(r.stdout+r.stderr)
 print(f'{name}: exit={r.returncode}, errors={r.stdout.count(": error CS")}, warnings={r.stdout.count(": warning CS")}')
 if r.returncode:raise SystemExit(r.returncode)
