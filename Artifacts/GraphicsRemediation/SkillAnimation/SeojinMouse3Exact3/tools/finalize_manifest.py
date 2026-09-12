from pathlib import Path
import json, hashlib

ROOT=Path('/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/SkillAnimation/SeojinMouse3Exact3/selected/revision-01')
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    sm=ROOT/'static-vfx-icons-manifest.json'; bm=ROOT/'body-manifest.json'
    s=json.loads(sm.read_text()); b=json.loads(bm.read_text())
    assets=[x for x in s['assets'] if x['kind'] in ('icon','vfx','vfxGif','contact')]+b['assets']
    out={'task':'M3-ART-V1','status':'COMPLETE39_ART_CANDIDATE_READY_FOR_DOWNSTREAM_T0','authority':{'path':'/private/tmp/projectbs-byeori-recovery-mouse3-smart-ai-contract.md','sha256':'15cb5535c40f4a584172932d15d7935ffd0012dbfd913e3408d7db4e88dab9dc'},'skills':['active_5 command_chain','active_6 thunder_command','active_7 blockade_cut'],'counts':{'icons':3,'bodyFrames':18,'vfxFrames':18,'totalRuntimePng':39,'bodyGifs':3,'vfxGifs':3},'canvas':{'icon':[512,512],'body':[568,340],'vfx':[256,256]},'timing':s['timing'],'sourceManifests':[{'path':str(sm),'sha256':sha(sm)},{'path':str(bm),'sha256':sha(bm)}],'qaReceipts':[{'path':str(ROOT/'qa-receipt-v001.json'),'sha256':sha(ROOT/'qa-receipt-v001.json')},{'path':str(ROOT/'body-qa-receipt-v001.json'),'sha256':sha(ROOT/'body-qa-receipt-v001.json')}],'assets':assets,'boundaries':{'selectedSource':True,'install':False,'canonical':False,'unity':False,'rollback':'remove revision-01 artifact directory only; existing project assets unchanged'}}
    mp=ROOT/'manifest.json'; mp.write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n')
    receipt=ROOT/'art-selection-receipt.txt'
    receipt.write_text('M3-ART-V1 COMPLETE39 ART CANDIDATE\nmanifest='+str(mp)+'\nmanifest_sha256='+sha(mp)+'\nicons=3 body=18 vfx=18\nselection=command inward-pull / thunder vertical-impact / blockade 55deg control-fan\nqa=static-vfx-icons strict PASS; body strict PASS\ninstall=0 canonical=0 unity=0\nrollback=artifact revision-01 removal; project unchanged\n')
    print(mp); print(sha(mp)); print(receipt); print(sha(receipt))
if __name__=='__main__': main()
