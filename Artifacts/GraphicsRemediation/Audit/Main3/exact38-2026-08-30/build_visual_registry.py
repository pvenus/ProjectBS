from pathlib import Path
import json, hashlib

ROOT=Path(__file__).resolve().parent
m=json.loads((ROOT/'main3-exact38-contact-manifest.json').read_text())

def classify(char, aid):
    # Returns disposition, correction type, concise visual authority reason.
    if char=='Yujin':
        if '.multi_shot.' in aid:
            return 'FAIL','STRUCTURAL_GRADE', 'Three-arrow lineage is clear, but G2/G3 silhouettes remain near-duplicates; clone follows corrected Y3 source.'
        if '.hwalbin_barrage.' in aid:
            return 'FAIL','STRUCTURAL_GRADE', 'Parallel corridor meaning reads, but G2/G3 use the same three-line corridor without command-scale growth; clone follows source.'
        if '.passive_1.' in aid:
            return 'PASS','KEEP_RUNTIME_EQUIVALENT', 'Tracking-arrow cause/effect is readable; G2/G3 runtime meaning is identical, so stable artwork and clone projection are correct.'
        if '.outlaw_appearance.' in aid:
            return 'FAIL','READABILITY_32_80', 'Central hat/tie motif occupies too little area and collapses into noise at 32/80.'
        if '.basic_attack.' in aid and ('.3.' in aid):
            return 'FAIL','STRUCTURAL_GRADE', 'G3 becomes a dense black firing wedge instead of extending the one-arrow empty-range structure; clone follows corrected source.'
        return 'PASS','KEEP', 'G2 basic retains one physical horizontal arrow, ordered tail, and open aiming space.'
    if char=='Jihan':
        if '.medicine_prescription.' in aid and ('.2.' in aid or '.3.' in aid):
            return 'FAIL','STRUCTURAL_GRADE_COLOR_BALANCE', 'G2/G3 become similar botanical rings; G3 relies on yellow ornament rather than an open cause-to-circuit structure.'
        if '.ten_tonic_soup.' in aid:
            return 'FAIL','STRUCTURAL_GRADE', 'G2/G3 four-lobe rotations exchange texture but do not establish a stronger connected support structure.'
        if '.divine_acupuncture.' in aid:
            return 'FAIL','READABILITY_32_80', 'Needle/meridian detail is meaningful at 200 but too thin and dispersed at 80/32.'
        return 'PASS','KEEP', 'Center/circulation silhouette and herb/teal recognition anchors remain readable with acceptable structural growth.'
    if char=='Seojin':
        if '.charge.' in aid:
            return 'FAIL','QUALITY_AND_STRUCTURAL_GRADE', 'G2 broad wing has more authority than sparse G3 planes; material weight regresses and grade order reverses.'
        if '.3.basic_attack.' in aid:
            return 'FAIL','LINEAGE_AND_STRUCTURAL_GRADE', 'G3 changes the grounded cut into a separate post-like sweep and loses the G1/G2 family axis.'
        if '.passive_1.' in aid and ('.2.' in aid or '.3.' in aid):
            return 'FAIL','LINEAGE_AND_MISREAD', 'G2 sign/roof and G3 anchor-fortress are individually legible but weakly related and risk non-skill object misread.'
        if '.turtle_ship_assault.' in aid:
            return 'FAIL','STYLE_AND_READABILITY_32_80', 'Literal detailed ship becomes a dark blob at 32 and departs from the abstract ink tactical grammar.'
        if '.turtle_ship_cannon_zone.' in aid:
            return 'FAIL','READABILITY_32_80', 'Crater/zone reads at 200 but becomes a generic dark oval at 32 without a command-fire cue.'
        return 'PASS','KEEP', 'Heavy forward silhouette, tactical formation structure, and navy/ink/russet anchor placement remain readable.'

entries=[]
for char,section in m.items():
    for e in section['entries']:
        disp,ctype,reason=classify(char,e['assetId'])
        entries.append({
            'character':char,'assetId':e['assetId'],'canonicalPng':e['canonicalPng'],'canonicalSha256':e['sha256'],
            'disposition':disp,'correctionType':ctype,'reason':reason,
            'evaluation':['quality/material','structural grade separation','32/80 readability','signature anchor and support colors','silhouette/effect lineage']
        })

batches=[
 {'order':0,'gate':'USER_CALIBRATION','character':'Seojin','family':'charge','scope':'representative G1/G2/G3 strip; G1 authority, correct G2/G3'},
 {'order':0,'gate':'USER_CALIBRATION','character':'Jihan','family':'medicine_prescription','scope':'representative G1/G2/G3 strip; G1 authority, correct G2/G3'},
 {'order':0,'gate':'USER_CALIBRATION','character':'Yujin','family':'multi_shot','scope':'representative G1/G2/G3 strip; G1 authority, correct G2/G3 then project clone'},
 {'order':1,'gate':'AFTER_CALIBRATION_PASS','character':'Yujin','family':'hwalbin_barrage','scope':'G2/G3 structural corridor growth then clone projection'},
 {'order':2,'gate':'AFTER_CALIBRATION_PASS','character':'Yujin','family':'basic_attack','scope':'G3 open-range structure correction then clone projection; retain G2'},
 {'order':3,'gate':'AFTER_CALIBRATION_PASS','character':'Yujin','family':'outlaw_appearance','scope':'G3 occupancy/readability correction'},
 {'order':4,'gate':'AFTER_CALIBRATION_PASS','character':'Jihan','family':'ten_tonic_soup','scope':'G2/G3 connected-support structural growth'},
 {'order':5,'gate':'AFTER_CALIBRATION_PASS','character':'Jihan','family':'divine_acupuncture','scope':'G3 32/80 silhouette consolidation'},
 {'order':6,'gate':'AFTER_CALIBRATION_PASS','character':'Seojin','family':'basic_attack','scope':'G3 lineage correction using G1/G2 authority'},
 {'order':7,'gate':'AFTER_CALIBRATION_PASS','character':'Seojin','family':'indomitable','scope':'G2/G3 common defensive lineage and object-misread removal'},
 {'order':8,'gate':'AFTER_CALIBRATION_PASS','character':'Seojin','family':'turtle_ship_assault','scope':'G3 tactical silhouette/style consolidation'},
 {'order':9,'gate':'AFTER_CALIBRATION_PASS','character':'Seojin','family':'turtle_ship_cannon_zone','scope':'G3 zone+command-fire 32/80 correction'},
]
out={
 'schema':'projectbs.main3.visual-reaudit.v1','scope':'exact38 only; Non-Main58 excluded','status':'READ_ONLY_AUDIT_COMPLETE_PILOT_GENERATION_BLOCKED',
 'palettePolicy':'signature color is a recognition anchor, not an exclusive palette; support/accent hues unrestricted when motivated',
 'hardFails':['cross-character identity confusion','unmotivated neon/gold flooding','color-only grade escalation','grayscale structural order below 80%','lineage below 90%','other-slot confusion above 10%'],
 'counts':{'total':len(entries),'pass':sum(e['disposition']=='PASS' for e in entries),'fail':sum(e['disposition']=='FAIL' for e in entries)},
 'byCharacter':{c:{'total':sum(e['character']==c for e in entries),'pass':sum(e['character']==c and e['disposition']=='PASS' for e in entries),'fail':sum(e['character']==c and e['disposition']=='FAIL' for e in entries)} for c in ('Yujin','Jihan','Seojin')},
 'contacts':{c:{'path':v['contact'],'sha256':v['contactSha256']} for c,v in m.items()},
 'entries':entries,'productionGraph':batches,
 'boundary':{'pilotGeneration':0,'canonicalWrite':0,'metaGuidStagingUnity':0,'nextGate':'user reviews representative3 calibration contacts'}
}
p=ROOT/'main3-exact38-visual-registry.json'; p.write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(out['counts']),json.dumps(out['byCharacter']))
print(hashlib.sha256(p.read_bytes()).hexdigest())
