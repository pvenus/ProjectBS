import hashlib, json, math, os, struct, zlib

SRC = os.path.join(os.path.dirname(__file__), "source", "seojin-charge-g1-g2-g3-3x6-source.png")
OUT = os.path.join(os.path.dirname(__file__), "extracted-rgba")
CONTACT = os.path.join(os.path.dirname(__file__), "contacts")
X = [52, 264, 483, 790, 1042, 1286, 1527]
Y = [46, 367, 679, 995]

def read_png(path):
    b=open(path,'rb').read(); pos=8; dat=b''
    while pos<len(b):
        n=struct.unpack('>I',b[pos:pos+4])[0]; typ=b[pos+4:pos+8]; d=b[pos+8:pos+8+n]; pos+=12+n
        if typ==b'IHDR': w,h,depth,color,_,_,_=struct.unpack('>IIBBBBB',d)
        elif typ==b'IDAT': dat+=d
        elif typ==b'IEND': break
    assert depth==8 and color in (2,6)
    ch=3 if color==2 else 4; raw=zlib.decompress(dat); stride=w*ch; rows=[]; off=0; prev=bytearray(stride)
    for _ in range(h):
        f=raw[off]; s=bytearray(raw[off+1:off+1+stride]); off+=1+stride
        for i in range(stride):
            a=s[i-ch] if i>=ch else 0; bb=prev[i]; c=prev[i-ch] if i>=ch else 0
            if f==1: s[i]=(s[i]+a)&255
            elif f==2: s[i]=(s[i]+bb)&255
            elif f==3: s[i]=(s[i]+((a+bb)//2))&255
            elif f==4:
                p=a+bb-c; pa=abs(p-a); pb=abs(p-bb); pc=abs(p-c); pr=a if pa<=pb and pa<=pc else bb if pb<=pc else c
                s[i]=(s[i]+pr)&255
        rows.append(s); prev=s
    pix=[]
    for s in rows:
        row=[]
        for i in range(0,len(s),ch): row.append(tuple(s[i:i+ch]))
        pix.append(row)
    return w,h,pix

def chunk(t,d): return struct.pack('>I',len(d))+t+d+struct.pack('>I',zlib.crc32(t+d)&0xffffffff)
def write_png(path,pix):
    h=len(pix); w=len(pix[0]); raw=b''.join(b'\0'+bytes(v for p in row for v in p) for row in pix)
    data=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(raw,9))+chunk(b'IEND',b'')
    os.makedirs(os.path.dirname(path),exist_ok=True); open(path,'wb').write(data)

def bilerp(pix,x,y):
    h=len(pix); w=len(pix[0]); x=max(0,min(w-1,x)); y=max(0,min(h-1,y)); x0=int(x); y0=int(y); x1=min(w-1,x0+1); y1=min(h-1,y0+1); fx=x-x0; fy=y-y0
    out=[]
    for k in range(len(pix[0][0])):
        v=(pix[y0][x0][k]*(1-fx)*(1-fy)+pix[y0][x1][k]*fx*(1-fy)+pix[y1][x0][k]*(1-fx)*fy+pix[y1][x1][k]*fx*fy)
        out.append(int(v+0.5))
    return tuple(out)

def median(vals):
    s=sorted(vals); return s[len(s)//2]

def cell_rgba(src,x0,y0,x1,y1,frame):
    x0+=2; y0+=2; x1-=2; y1-=2; sw=x1-x0; sh=y1-y0
    samples=[]
    for yy in list(range(y0,min(y0+12,y1)))+list(range(max(y0,y1-12),y1)):
        for xx in list(range(x0,min(x0+12,x1)))+list(range(max(x0,x1-12),x1)): samples.append(src[yy][xx][:3])
    bg=tuple(median([p[k] for p in samples]) for k in range(3)); out=[]
    for oy in range(256):
        row=[]; sy=y0+(oy+.5)*sh/256-.5
        for ox in range(256):
            sx=x0+(ox+.5)*sw/256-.5; rgb=bilerp(src,sx,sy)[:3]
            d=math.sqrt(sum((rgb[k]-bg[k])**2 for k in range(3)))
            a=max(0,min(255,int((d-8)*255/36+0.5)))
            if frame != 3 and sum(rgb)/3 >= sum(bg)/3-5: a=0
            if ox < 2 or oy < 2 or ox >= 254 or oy >= 254: a=0
            if a==0: row.append((0,0,0,0))
            else: row.append(tuple(rgb)+(a,))
        out.append(row)
    return out,bg

def resize_rgba(src,n,gray=False):
    h=len(src); w=len(src[0]); out=[]
    for y in range(n):
        row=[]; sy=(y+.5)*h/n-.5
        for x in range(n):
            sx=(x+.5)*w/n-.5
            # premultiplied bilinear via four-point sampling
            x0=max(0,min(w-1,int(math.floor(sx)))); y0=max(0,min(h-1,int(math.floor(sy)))); x1=min(w-1,x0+1); y1=min(h-1,y0+1); fx=sx-math.floor(sx); fy=sy-math.floor(sy)
            ps=[(src[y0][x0],(1-fx)*(1-fy)),(src[y0][x1],fx*(1-fy)),(src[y1][x0],(1-fx)*fy),(src[y1][x1],fx*fy)]
            a=sum(p[3]*q for p,q in ps); rgb=[]
            for k in range(3): rgb.append(int(sum(p[k]*p[3]*q for p,q in ps)/a+0.5) if a>0 else 0)
            if gray:
                g=int(.2126*rgb[0]+.7152*rgb[1]+.0722*rgb[2]+.5); rgb=[g,g,g]
            row.append(tuple(rgb)+(int(a+0.5),))
        out.append(row)
    return out

def contact(frames,n,gray=False):
    gap=2; W=6*n+5*gap; H=3*n+2*gap; out=[[(24,24,24,255) for _ in range(W)] for _ in range(H)]
    for g in range(3):
        for f in range(6):
            im=resize_rgba(frames[g][f],n,gray); ox=f*(n+gap); oy=g*(n+gap)
            for y,row in enumerate(im):
                for x,p in enumerate(row):
                    a=p[3]/255; base=out[oy+y][ox+x]; out[oy+y][ox+x]=tuple(int(p[k]*a+base[k]*(1-a)+.5) for k in range(3))+(255,)
    return out

def sha(path): return hashlib.sha256(open(path,'rb').read()).hexdigest()

w,h,src=read_png(SRC); assert (w,h)==(1536,1024)
frames=[[None]*6 for _ in range(3)]; records=[]
for g in range(3):
    for f in range(6):
        im,bg=cell_rgba(src,X[f],Y[g],X[f+1],Y[g+1],f); frames[g][f]=im
        name=f"seojin-charge-g{g+1}-f{f}.rgba.png"; path=os.path.join(OUT,name); write_png(path,im)
        cov=sum(p[3]>0 for row in im for p in row); opaque=sum(p[3]==255 for row in im for p in row); partial=cov-opaque
        corners=[im[0][0][3],im[0][-1][3],im[-1][0][3],im[-1][-1][3]]
        records.append({'grade':g+1,'frame':f,'timestamp':round(f*.08,2),'source_rect':[X[f]+2,Y[g]+2,X[f+1]-X[f]-4,Y[g+1]-Y[g]-4],'canvas':[256,256],'pivot':[0.5,0.5],'background_rgb':bg,'sha256':sha(path),'alpha_nonzero':cov,'alpha_opaque':opaque,'alpha_partial':partial,'corners_alpha':corners,'path':path})
for n in (200,80,32):
    for gray in (False,True):
        path=os.path.join(CONTACT,f"seojin-charge-exact18-{n}px-{'gray' if gray else 'color'}.png"); write_png(path,contact(frames,n,gray))
manifest={'status':'EXTRACTED_RGBA_VISUAL_REVIEW_PENDING','source':SRC,'source_sha256':sha(SRC),'source_dimensions':[w,h],'grid_x':X,'grid_y':Y,'rows':['G1','G2','G3'],'columns':['F0','F1','F2','F3','F4','F5'],'timestamps':[0,.08,.16,.24,.32,.40],'stopTime':.48,'terminal_alpha0_at':.48,'frames':records,'contacts':[],'basic_extracted':False,'canonical_write':False}
for n in (200,80,32):
    for gray in (False,True):
        path=os.path.join(CONTACT,f"seojin-charge-exact18-{n}px-{'gray' if gray else 'color'}.png"); manifest['contacts'].append({'path':path,'sha256':sha(path)})
mp=os.path.join(os.path.dirname(__file__),'seojin-charge-exact18-manifest.json'); open(mp,'w').write(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
print(mp,sha(mp)); print('frames',len(records)); print('first',records[0]['path'],records[0]['sha256'])
