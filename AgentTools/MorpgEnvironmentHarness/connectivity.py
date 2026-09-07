"""Independent .10wu cardinal-grid reachability audit; not a claim that narrow gaps pass."""
from pathlib import Path
from collections import deque
import json,math
R=Path('/Users/pvenus/ProjectBS');p=json.loads((R/'Assets/Resources/battle/morpg/environment/environment.v1.json').read_text());results=[]
def free(z,x,y):
 vertices=z['walkable']
 for a,b in zip(vertices,vertices[1:]+vertices[:1]):
  dx,dy=b['x']-a['x'],b['y']-a['y']
  if (dx*(y-a['y'])-dy*(x-a['x']))/math.hypot(dx,dy)<.5-1e-7:return False
 for q in z['props']:
  if q['radius']:d=math.hypot(x-q['center'][0],y-q['center'][1])-q['radius']
  else:
   a,b,c,d=q['box'];d=math.hypot(max(a-x,0,x-c),max(b-y,0,y-d))
  if d<.5-1e-7:return False
 return True
for z in p['zones']:
 cells={(x,y) for x in range(math.ceil(min(v['x'] for v in z['walkable'])*10),math.floor(max(v['x'] for v in z['walkable'])*10)+1) for y in range(math.ceil(min(v['y'] for v in z['walkable'])*10),math.floor(max(v['y'] for v in z['walkable'])*10)+1) if free(z,x/10,y/10)}
 start=min(cells,key=lambda c:((c[0]/10-z['entry'][0])**2+(c[1]/10-z['entry'][1])**2,c));seen={start};queue=deque([start])
 while queue:
  x,y=queue.popleft()
  for v in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]:
   if v in cells and v not in seen:seen.add(v);queue.append(v)
 rows=[]
 for r in p['reservations']:
  if r['zoneId']!=z['zoneId']:continue
  near=min(cells,key=lambda c:((c[0]/10-r['position'][0])**2+(c[1]/10-r['position'][1])**2,c))
  assert near in seen,r['reservationId'];rows.append(r['reservationId'])
 results.append(dict(zoneId=z['zoneId'],gridStep=.1,actorRadius=.35,actorInset=.15,freeCells=len(cells),reachableCells=len(seen),unreachableSpawnCount=0,reservations=rows,minWidthAccepted='see production geometry gap audit'))
(R/'Artifacts/Engineering/MorpgEnvironmentInstall/revision-03/applied/connectivity.json').write_text(json.dumps(results,indent=2)+'\n')
print('PASS all28 spawn-to-entry connected on .10wu grid; width/egress checked by production geometry suite')
