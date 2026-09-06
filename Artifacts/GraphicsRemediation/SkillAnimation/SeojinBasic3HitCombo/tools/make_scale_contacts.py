#!/usr/bin/env python3
import glob
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path('/Users/pvenus/ProjectBS')
OUT=ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-02-scale-normalized/contacts'
groups={
 'body_reference':glob.glob(str(ROOT/'Assets/ImagesGenerated/Character/animation/character.seojin.2/character.seojin.2.attack.stable-right-forward/frame-*.png')),
 'body_normalized':glob.glob(str(ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-02-scale-normalized/body/**/*.png'),recursive=True),
 'vfx_hit0_reference':glob.glob(str(ROOT/'Assets/ImagesGenerated/Skill/animation/skill.character.seojin.*.basic_attack.basic_attack/frame-*.png')),
 'vfx_normalized':glob.glob(str(ROOT/'Artifacts/GraphicsRemediation/SkillAnimation/SeojinBasic3HitCombo/selected/revision-02-scale-normalized/vfx/**/*.png'),recursive=True),
}
OUT.mkdir(parents=True,exist_ok=True)
for size in (200,80,32):
 for dark,bg in ((True,(18,21,27)),(False,(232,226,211))):
  cols=18; sheet=Image.new('RGB',(cols*size,4*(size+16)),bg); d=ImageDraw.Draw(sheet)
  for row,(name,files) in enumerate(groups.items()):
   for col,p in enumerate(sorted(files)[:cols]):
    im=Image.open(p).convert('RGBA').resize((size,size),Image.Resampling.LANCZOS)
    tile=Image.new('RGBA',(size,size),bg+(255,)); tile.alpha_composite(im); sheet.paste(tile.convert('RGB'),(col*size,row*(size+16)))
   d.text((2,row*(size+16)+size),name,fill=(230,120,70) if dark else (20,40,70))
  sheet.save(OUT/f'scale-audit-{size}-{"dark" if dark else "light"}.png')
