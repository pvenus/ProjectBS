from collections import deque
from pathlib import Path
import hashlib, json
import numpy as np
from PIL import Image

ROOT = Path(__file__).parent
PROJECT = ROOT.parents[5]
SOURCE = PROJECT / "Artifacts/GraphicsRemediation/PortraitCategory/character.training_ground_captain.2/revision-02/selected/captain2-selected-B.png"
REGIONS = PROJECT / "Artifacts/GraphicsRemediation/PortraitCategory/character.training_ground_captain.2/revision-02/method-rescope/segmentation-region-map.json"

def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()

def smoothstep(x, lo, hi):
    t = np.clip((x - lo) / (hi - lo), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)

def components(mask):
    h, w = mask.shape
    seen = np.zeros_like(mask, dtype=bool)
    result = []
    for y, x in zip(*np.nonzero(mask & ~seen)):
        if seen[y, x]:
            continue
        q = deque([(int(y), int(x))]); seen[y, x] = True; pts = []
        while q:
            yy, xx = q.popleft(); pts.append((yy, xx))
            for dy, dx in ((-1,0),(1,0),(0,-1),(0,1)):
                ny, nx = yy+dy, xx+dx
                if 0 <= ny < h and 0 <= nx < w and mask[ny,nx] and not seen[ny,nx]:
                    seen[ny,nx] = True; q.append((ny,nx))
        result.append(pts)
    return result

def main():
    rgb = np.asarray(Image.open(SOURCE).convert("RGB"), dtype=np.uint8)
    h, w, _ = rgb.shape
    assert (w, h) == (1024, 1536)
    border = np.concatenate((rgb[:48].reshape(-1,3), rgb[-48:].reshape(-1,3),
                             rgb[:,:48].reshape(-1,3), rgb[:,-48:].reshape(-1,3))).astype(np.float64)
    centers = np.array((np.percentile(border, 25, axis=0), np.percentile(border, 75, axis=0)))
    for _ in range(32):
        d2 = ((border[:,None,:] - centers[None,:,:]) ** 2).sum(2)
        labels = d2.argmin(1)
        updated = np.array([border[labels == i].mean(0) for i in range(2)])
        if np.max(np.abs(updated-centers)) < 1e-7: break
        centers = updated
    px = rgb.astype(np.float64)
    distance = np.sqrt(((px[:,:,None,:] - centers[None,None,:,:]) ** 2).sum(3).min(2))

    # Background likelihood is color-derived; ownership is border-connected.
    bg_candidate = distance <= 8.0
    bg_connected = np.zeros((h,w), dtype=bool); q = deque()
    for x in range(w):
        for y in (0,h-1):
            if bg_candidate[y,x] and not bg_connected[y,x]: bg_connected[y,x]=True; q.append((y,x))
    for y in range(h):
        for x in (0,w-1):
            if bg_candidate[y,x] and not bg_connected[y,x]: bg_connected[y,x]=True; q.append((y,x))
    while q:
        y,x=q.popleft()
        for dy,dx in ((-1,0),(1,0),(0,-1),(0,1)):
            ny,nx=y+dy,x+dx
            if 0<=ny<h and 0<=nx<w and bg_candidate[ny,nx] and not bg_connected[ny,nx]:
                bg_connected[ny,nx]=True; q.append((ny,nx))

    alpha_f = smoothstep(distance, 2.5, 12.0)
    alpha_f[bg_connected & (distance <= 8.0)] = 0.0
    # Remove one-pixel high-confidence specks, retaining the torso and every
    # edge-supported dry-brush/sword fragment of area >= 2.
    candidate = alpha_f >= (16/255)
    comps = components(candidate)
    keep = np.zeros_like(candidate)
    largest = max(comps, key=len)
    for pts in comps:
        area=len(pts); ys=np.fromiter((p[0] for p in pts),int); xs=np.fromiter((p[1] for p in pts),int)
        sword=np.any((xs>=92)&(xs<318)&(ys>=805)&(ys<1330))
        edge_conf=float(distance[ys,xs].max()) >= 18.0
        if pts is largest or sword or (area >= 2 and edge_conf): keep[ys,xs]=True
    alpha_f[~keep] = 0.0
    alpha = np.rint(alpha_f * 255).astype(np.uint8)
    alpha[alpha < 4] = 0; alpha[alpha > 251] = 255
    trimap = np.where(alpha == 0, 0, np.where(alpha == 255, 255, 128)).astype(np.uint8)

    out = np.zeros((h,w,4), dtype=np.uint8); out[:,:,3]=alpha
    opaque = alpha == 255; partial = (alpha>0)&(alpha<255)
    out[opaque,:3] = rgb[opaque]
    # Partial-edge-only local inward foreground propagation, radius <= 3.
    for y,x in zip(*np.nonzero(partial)):
        samples=[]
        for radius in (1,2,3):
            y0=max(0,y-radius); y1=min(h,y+radius+1); x0=max(0,x-radius); x1=min(w,x+radius+1)
            om=opaque[y0:y1,x0:x1]
            if np.any(om): samples=rgb[y0:y1,x0:x1][om]; break
        out[y,x,:3] = samples.mean(0).astype(np.uint8) if len(samples) else rgb[y,x]

    Image.fromarray(trimap,"L").save(ROOT/"region-trimap.png",compress_level=9,optimize=False)
    Image.fromarray(alpha,"L").save(ROOT/"alpha-mask.png",compress_level=9,optimize=False)
    Image.fromarray(out,"RGBA").save(ROOT/"final-selected-B-alpha.png",compress_level=9,optimize=False)
    fg=Image.fromarray(out,"RGBA"); neutral=Image.new("RGB",(w,h),(216,213,204)); dark=Image.new("RGB",(w,h),(35,38,42))
    neutral.paste(fg,(0,0),fg); dark.paste(fg,(0,0),fg)
    contact=Image.new("RGB",(w*2,h)); contact.paste(neutral,(0,0)); contact.paste(dark,(w,0))
    contact.resize((1024,768),Image.Resampling.LANCZOS).save(ROOT/"contact-neutral-dark.png",compress_level=9,optimize=False)

    region_map=json.loads(REGIONS.read_text())
    regional={}
    for region in region_map["regions"]:
        x0,y0,x1,y1=region["bbox"]; aa=alpha[y0:y1,x0:x1]; dd=distance[y0:y1,x0:x1]
        regional[region["id"]]={"bbox":region["bbox"],"alpha_ge16":int((aa>=16).sum()),
            "background_model_positive_alpha_ge16":int(((dd<=8)&(aa>=16)).sum()),
            "partial":int(((aa>0)&(aa<255)).sum())}
    model={"source":str(SOURCE.relative_to(PROJECT)),"source_sha256":sha(SOURCE),"region_authority_sha256":sha(REGIONS),
        "border_px":48,"components":centers.tolist(),"distance":{"background_connected_max":8.0,"alpha_low":2.5,"alpha_high":12.0},
        "ownership":"largest torso + sword-overlap + area>=2 edge-confidence>=18"}
    (ROOT/"background-model.json").write_text(json.dumps(model,indent=2)+"\n")
    bbox=Image.fromarray(alpha).getbbox(); residue=int(np.any(out[alpha==0,:3]!=0,axis=1).sum())
    metrics={"source_sha256":sha(SOURCE),"dimensions":[w,h],"alpha":{"transparent":int((alpha==0).sum()),
        "partial":int(partial.sum()),"opaque":int(opaque.sum()),"bbox":bbox,"corners":[int(alpha[0,0]),int(alpha[0,-1]),int(alpha[-1,0]),int(alpha[-1,-1])],
        "alpha0_rgb_residue":residue,"opaque_source_rgb_mismatch":int(np.any(out[opaque,:3]!=rgb[opaque],axis=1).sum())},
        "components":{"candidate":len(comps),"retained":int(sum(1 for p in comps if np.any(keep[tuple(zip(*p))])))},"regions":regional,
        "outputs":{n:sha(ROOT/n) for n in ("background-model.json","region-trimap.png","alpha-mask.png","final-selected-B-alpha.png","contact-neutral-dark.png")}}
    (ROOT/"provenance-metrics.json").write_text(json.dumps(metrics,indent=2)+"\n")

if __name__ == "__main__": main()
