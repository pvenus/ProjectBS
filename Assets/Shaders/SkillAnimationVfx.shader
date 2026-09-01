Shader "Custom/SkillAnimationVfx"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _VfxTintColor ("VFX Tint", Color) = (1,1,1,1)
        _VfxTintStrength ("VFX Tint Strength", Range(0,.5)) = 0
        _VfxColorA ("VFX Color A", Color) = (1,1,1,1)
        _VfxColorB ("VFX Color B", Color) = (1,1,1,1)
        _VfxColorPhase ("VFX Color Phase", Range(0,1)) = 0
        _VfxColorShiftStrength ("VFX Color Shift", Range(0,.35)) = 0
        _VfxEmissionColor ("VFX Emission", Color) = (0,0,0,1)
        _VfxEmissionIntensity ("VFX Emission Intensity", Range(0,1.1)) = 0
        _VfxGlobalAlpha ("VFX Global Alpha", Range(0,1)) = 1
        _VfxDesaturate ("VFX Desaturate", Range(0,.18)) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,.78)
        _OutlineWidth ("Outline Width", Range(0,2)) = 1
        _RimIntensity ("Rim Intensity", Range(0,.85)) = .35
        _PulseIntensity ("Pulse Intensity", Range(0,.28)) = .1
        _PulseSpeed ("Pulse Speed", Range(0,3)) = 2
        _FlowSpeedX ("Flow Speed X", Range(-5,5)) = 0
        _FlowSpeedY ("Flow Speed Y", Range(-5,5)) = 0
        _AlphaClip ("Alpha Clip", Range(0,1)) = .01
        _VfxRimColor ("VFX Rim Color", Color) = (1,1,1,1)
        _VfxSpatialPattern ("VFX Spatial Pattern", Float) = 0
        _VfxRimPulseStrength ("VFX Rim Pulse Strength", Range(0,.6)) = 0
        _VfxRimPulsePhase ("VFX Rim Pulse Phase", Range(0,1)) = 0
        _VfxSweepColor ("VFX Sweep Color", Color) = (1,1,1,1)
        _VfxSweepDirection ("VFX Sweep Direction", Vector) = (1,0,0,0)
        _VfxSweepStrength ("VFX Sweep Strength", Range(0,.4)) = 0
        _VfxSweepWidth ("VFX Sweep Width", Range(.02,.2)) = .1
        _VfxSweepSoftness ("VFX Sweep Softness", Range(.01,.12)) = .05
        _VfxSweepPhase ("VFX Sweep Phase", Range(0,1)) = 0
        _VfxGlintColor ("VFX Glint Color", Color) = (1,1,1,1)
        _VfxGlintStrength ("VFX Glint Strength", Range(0,.3)) = 0
        _VfxGlintWidth ("VFX Glint Width", Range(.01,.08)) = .03
        _VfxImpactColor ("VFX Impact Color", Color) = (1,1,1,1)
        _VfxImpactPeak ("VFX Impact Peak", Range(0,1)) = 0
        _VfxAfterglowColor ("VFX Afterglow Color", Color) = (0,0,0,1)
        _VfxAfterglow ("VFX Afterglow", Range(0,.4)) = 0
        _VfxInkDensity ("VFX Ink Density", Range(0,.6)) = .3
        _VfxInkBreakup ("VFX Ink Breakup", Range(0,1)) = .5
        _VfxInkScale ("VFX Ink Scale", Range(4,64)) = 24
        _VfxInkFlow ("VFX Ink Flow", Vector) = (1,0,0,0)
        _VfxNeonRimStrength ("VFX Neon Rim Strength", Range(0,1)) = .5
        _VfxNeonPeakGain ("VFX Neon Peak Gain", Range(0,1.5)) = 1
        _VfxNeonAfterglowGain ("VFX Neon Afterglow Gain", Range(0,1)) = .5
        _VfxSignatureColor ("VFX Signature Color", Color) = (1,1,1,1)
        _VfxAuxiliaryColor ("VFX Auxiliary Color", Color) = (0,0,0,1)
        _VfxNeutralPeakColor ("VFX Neutral Peak Color", Color) = (1,1,1,1)
        _VfxSignatureCoverage ("VFX Signature Coverage", Range(.7,.9)) = .8
        _VfxAuxiliaryEnvelope ("VFX Auxiliary Envelope", Range(0,.3)) = .18
        _VfxNeutralPeakEnvelope ("VFX Neutral Peak Envelope", Range(0,.06)) = .04
        _VfxBodyOpacityGain ("VFX Body Opacity Gain", Range(0,1)) = 0
        _VfxLocalizedGlowAlpha ("VFX Localized Glow Alpha", Range(0,.2)) = 0
        _VfxSpriteUvRect ("VFX Sprite UV Rect", Vector) = (0,0,1,1)
        _VfxGroundFieldMode ("VFX Ground Field Mode", Float) = 0
        _VfxFieldPhase ("VFX Field Phase", Range(0,1)) = 0
        _VfxFieldAspect ("VFX Field Aspect", Vector) = (1,.62,0,0)
        _VfxFieldEdge ("VFX Field Edge", Vector) = (.18,.1,0,0)
        _VfxFieldInk ("VFX Field Ink", Vector) = (.62,.85,3.4,.18)
        _VfxFieldSupport ("VFX Field Support", Vector) = (.14,2,0,0)
        _VfxFieldScars ("VFX Field Scars", Vector) = (3,.025,0,0)
        _VfxFieldSeed ("VFX Field Seed", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            sampler2D _MainTex; float4 _MainTex_TexelSize;
            fixed4 _Color, _VfxTintColor, _VfxColorA, _VfxColorB, _VfxEmissionColor, _OutlineColor;
            fixed4 _VfxRimColor, _VfxSweepColor, _VfxGlintColor, _VfxImpactColor, _VfxAfterglowColor;
            fixed4 _VfxSignatureColor, _VfxAuxiliaryColor, _VfxNeutralPeakColor;
            float _VfxTintStrength, _VfxColorPhase, _VfxColorShiftStrength, _VfxEmissionIntensity;
            float _VfxGlobalAlpha, _VfxDesaturate, _OutlineWidth, _RimIntensity, _PulseIntensity, _PulseSpeed, _FlowSpeedX, _FlowSpeedY, _AlphaClip;
            float _VfxSpatialPattern, _VfxRimPulseStrength, _VfxRimPulsePhase, _VfxSweepStrength, _VfxSweepWidth, _VfxSweepSoftness, _VfxSweepPhase;
            float4 _VfxSweepDirection;
            float _VfxGlintStrength, _VfxGlintWidth, _VfxImpactPeak, _VfxAfterglow;
            float _VfxInkDensity, _VfxInkBreakup, _VfxInkScale, _VfxNeonRimStrength, _VfxNeonPeakGain, _VfxNeonAfterglowGain;
            float _VfxSignatureCoverage, _VfxAuxiliaryEnvelope, _VfxNeutralPeakEnvelope;
            float _VfxBodyOpacityGain, _VfxLocalizedGlowAlpha;
            float4 _VfxSpriteUvRect;
            float _VfxGroundFieldMode, _VfxFieldPhase, _VfxFieldSeed;
            float4 _VfxFieldAspect, _VfxFieldEdge, _VfxFieldInk, _VfxFieldSupport, _VfxFieldScars;
            float4 _VfxInkFlow;
            float inkHash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float valueNoise(float2 p)
            {
                float2 c=floor(p), f=frac(p); f=f*f*(3.0-2.0*f);
                float a=inkHash(c+_VfxFieldSeed), b=inkHash(c+float2(1,0)+_VfxFieldSeed);
                float d=inkHash(c+float2(0,1)+_VfxFieldSeed), e=inkHash(c+1.0+_VfxFieldSeed);
                return lerp(lerp(a,b,f.x),lerp(d,e,f.x),f.y);
            }
            float fieldPulse(float phase, float center, float width)
            {
                float d=abs(frac(phase-center+.5)-.5);
                float x=saturate(1.0-d/max(.001,width));
                return x*x*(3.0-2.0*x);
            }
            float elongatedLobe(float2 p,float2 center,float2 axis,float length,float width)
            {
                axis=normalize(axis);
                float2 q=p-center;
                float along=dot(q,axis), across=dot(q,float2(-axis.y,axis.x));
                return exp(-(along*along/max(.001,length*length)+across*across/max(.001,width*width)));
            }
            float dryScar(float2 p,float2 center,float2 axis,float length,float width,float erosion)
            {
                axis=normalize(axis);
                float2 q=p-center;
                float along=dot(q,axis), across=dot(q,float2(-axis.y,axis.x));
                float capsule=saturate(1.0-abs(across)/width)*saturate(1.0-abs(along)/length);
                float grain=valueNoise((p+center)*8.7+float2(.31,.73));
                return smoothstep(.18+erosion*.22,.72,capsule*(.72+grain*.38));
            }
            fixed4 groundField(float2 uv, fixed4 vertexColor)
            {
                float2 p=(uv-.5)*2.0;
                float bodyPhase=fieldPulse(_VfxFieldPhase,.02,.24);
                float supportPhase=fieldPulse(_VfxFieldPhase,.4,.28);
                float pressurePhase=fieldPulse(_VfxFieldPhase,.6,.20);
                float2 noiseP=p+sin(_VfxFieldPhase*6.2831853)*float2(.022,.009);
                float low=valueNoise(noiseP*_VfxFieldInk.y+float2(.13,.37));
                float high=valueNoise(noiseP*_VfxFieldInk.z+float2(.71,.19));
                float irregular=((low-.5)*2.0*_VfxFieldEdge.y+(high-.5)*.08);
                float r=length(float2(p.x,p.y/max(.01,_VfxFieldAspect.y)));
                float signedEdge=1.0-r+irregular*smoothstep(.78,1.0,r);
                float footprint=smoothstep(0.0,max(.01,_VfxFieldEdge.x),signedEdge);
                float interior=smoothstep(1.0,.74,r);
                footprint=max(footprint,interior*.82);

                float supportPulse=lerp(.72,1.0,supportPhase);
                float support=elongatedLobe(p,float2(-.20,.06),float2(1,.18),.46,.16);
                support+=elongatedLobe(p,float2(.16,-.08),float2(1,-.12),.43,.15);
                float rearRelay=elongatedLobe(p,float2(-.12,.28),float2(.92,.38),.38,.12);
                support+=step(2.5,_VfxFieldSupport.y)*rearRelay;
                support=saturate(support*.62)*footprint;

                float scar=0.0;
                scar=max(scar,dryScar(p,float2(.48,.02),float2(.96,.28),.14,.045,.15));
                scar=max(scar,dryScar(p,float2(.35,.22),float2(.98,-.20),.12,.042,.28)*.78);
                scar=max(scar,dryScar(p,float2(.39,-.23),float2(.94,.34),.13,.040,.35)*.70);
                scar=max(scar,step(3.5,_VfxFieldScars.x)*dryScar(p,float2(.12,-.31),float2(.96,-.28),.11,.038,.38)*.64);
                scar*=footprint*lerp(.78,1.0,pressurePhase);

                float densityPulse=lerp(.91,1.04,bodyPhase*.55+supportPhase*.45);
                float densityBreakup=1.0+(low-.5)*.24+(high-.5)*.08;
                float grain=lerp(low,high,_VfxFieldInk.w);
                float alpha=footprint*saturate((_VfxFieldInk.x+grain*.12)*densityBreakup*densityPulse)*_VfxGlobalAlpha*vertexColor.a;
                float3 navy=_VfxSignatureColor.rgb*lerp(.52,.78,grain);
                float supportWeight=saturate((_VfxFieldSupport.x+0.06)*supportPulse*support);
                float3 supportColor=lerp(navy,float3(.34,.42,.48),supportWeight);
                float rustMask=scar*saturate(_VfxFieldScars.y/.04);
                float3 rgb=lerp(supportColor,_VfxAuxiliaryColor.rgb,rustMask);
                clip(alpha-_AlphaClip);
                return fixed4(rgb*alpha,alpha);
            }
            v2f vert(appdata i) { v2f o; o.vertex=UnityObjectToClipPos(i.vertex); o.uv=i.uv; o.color=i.color*_Color; return o; }
            float4 frag(v2f i):SV_Target
            {
                if (_VfxGroundFieldMode > .5) return groundField(i.uv,i.color);
                fixed4 s=tex2D(_MainTex,i.uv)*i.color;
                float2 t=_MainTex_TexelSize.xy*_OutlineWidth;
                float2 uvRight=clamp(i.uv+float2(t.x,0),_VfxSpriteUvRect.xy,_VfxSpriteUvRect.zw);
                float2 uvLeft=clamp(i.uv-float2(t.x,0),_VfxSpriteUvRect.xy,_VfxSpriteUvRect.zw);
                float2 uvUp=clamp(i.uv+float2(0,t.y),_VfxSpriteUvRect.xy,_VfxSpriteUvRect.zw);
                float2 uvDown=clamp(i.uv-float2(0,t.y),_VfxSpriteUvRect.xy,_VfxSpriteUvRect.zw);
                float n=max(max(tex2D(_MainTex,uvRight).a,tex2D(_MainTex,uvLeft).a),
                            max(tex2D(_MainTex,uvUp).a,tex2D(_MainTex,uvDown).a));
                float globalAlpha=saturate(_VfxGlobalAlpha);
                float readableSourceAlpha=lerp(s.a,sqrt(saturate(s.a)),saturate(_VfxBodyOpacityGain));
                float alpha=readableSourceAlpha*globalAlpha;
                if(s.a<=_AlphaClip && n>_AlphaClip) { float a=_OutlineColor.a*n*saturate(_VfxGlobalAlpha); return fixed4(_OutlineColor.rgb*a,a); }
                clip(alpha-_AlphaClip);
                float lum=dot(s.rgb,float3(.2126,.7152,.0722));
                float3 rgb=lerp(lum.xxx,s.rgb*_VfxSignatureColor.rgb,
                    saturate(_VfxSignatureCoverage));
                float2 inkDir=normalize(_VfxInkFlow.xy+float2(1e-5,0));
                float2 inkUv=(i.uv+inkDir*_VfxColorPhase*.035)*max(4.0,_VfxInkScale);
                float grain=inkHash(floor(inkUv));
                float fibers=smoothstep(saturate(_VfxInkBreakup),1.0,grain);
                float inkMass=saturate(_VfxInkDensity*(.72+.28*(1.0-lum)));
                float inkValue=lerp(.52,.28,fibers);
                rgb*=lerp(1.0,inkValue,inkMass);
                float spatialPhase=frac(_VfxColorPhase+i.uv.x*_FlowSpeedX+i.uv.y*_FlowSpeedY);
                // Palette hierarchy: motion changes masks, never hue. Signature is
                // the only persistent chroma; auxiliary and neutral are bounded events.
                rgb=lerp(rgb,rgb*_VfxSignatureColor.rgb,
                    saturate(_VfxColorShiftStrength*_VfxSignatureCoverage));
                float mn=min(min(tex2D(_MainTex,uvRight).a,tex2D(_MainTex,uvLeft).a),
                             min(tex2D(_MainTex,uvUp).a,tex2D(_MainTex,uvDown).a));
                float innerRim=saturate((s.a-mn)*4.0);
                float rimPulse=_VfxRimPulseStrength*saturate(_VfxRimPulsePhase);
                float2 dir=normalize(_VfxSweepDirection.xy+float2(1e-5,0));
                float sweepCoord=dot(i.uv-.5,dir)+.5;
                if (_VfxSpatialPattern > .5 && _VfxSpatialPattern < 1.5)
                {
                    float lane=floor(saturate(i.uv.y)*4.0);
                    sweepCoord=frac(i.uv.x-lane*.045);
                }
                else if (_VfxSpatialPattern > 1.5 && _VfxSpatialPattern < 2.5)
                {
                    float2 centered=i.uv-.5;
                    float radial=saturate(length(centered)*2.0);
                    float opening=step(.22,abs(atan2(centered.y,centered.x)/6.2831853));
                    sweepCoord=lerp(1.0-radial,radial,step(.55,_VfxSweepPhase))*opening;
                }
                else if (_VfxSpatialPattern > 2.5)
                {
                    float bilateral=abs(i.uv.x-.5)*2.0;
                    sweepCoord=1.0-bilateral;
                }
                float sweepDistance=abs(sweepCoord-_VfxSweepPhase);
                float sweep=1.0-smoothstep(_VfxSweepWidth,_VfxSweepWidth+_VfxSweepSoftness,sweepDistance);
                float glint=1.0-smoothstep(_VfxGlintWidth,_VfxGlintWidth+max(.01,_VfxSweepSoftness),sweepDistance);
                float emissionMask=saturate(lum*.65+innerRim*.35);
                float localizedEmission=saturate(innerRim+sweep*.55+glint*.25);
                rgb+=_VfxSignatureColor.rgb*_VfxEmissionIntensity*emissionMask*localizedEmission;
                float brokenRim=innerRim*lerp(.55,1.0,fibers);
                rgb+=_VfxSignatureColor.rgb*brokenRim*(_RimIntensity+rimPulse)*_VfxNeonRimStrength;
                rgb+=_VfxSignatureColor.rgb*sweep*_VfxSweepStrength*s.a*_VfxNeonRimStrength;
                rgb+=_VfxSignatureColor.rgb*glint*_VfxGlintStrength*brokenRim*_VfxNeonRimStrength;
                rgb+=_VfxNeutralPeakColor.rgb*_VfxImpactPeak*emissionMask*
                    _VfxNeonPeakGain*_VfxNeutralPeakEnvelope;
                rgb+=_VfxAuxiliaryColor.rgb*_VfxImpactPeak*emissionMask*
                    _VfxNeonPeakGain*_VfxAuxiliaryEnvelope;
                float afterMask=innerRim;
                if (_VfxSpatialPattern > .5 && _VfxSpatialPattern < 1.5) afterMask*=saturate(1.0-i.uv.x)*step(.18,frac(i.uv.y*4.0));
                else if (_VfxSpatialPattern > 1.5 && _VfxSpatialPattern < 2.5) afterMask*=saturate(i.uv.y*1.8);
                else if (_VfxSpatialPattern > 2.5) afterMask*=1.0-smoothstep(.18,.24,i.uv.y);
                rgb+=_VfxAuxiliaryColor.rgb*_VfxAfterglow*afterMask*
                    _VfxNeonAfterglowGain*_VfxAuxiliaryEnvelope;
                float glowSignal=saturate(_VfxEmissionIntensity*emissionMask*localizedEmission+
                    _VfxImpactPeak*emissionMask*(_VfxNeutralPeakEnvelope+_VfxAuxiliaryEnvelope));
                float sourceSupport=smoothstep(_AlphaClip,.32,s.a);
                alpha=max(alpha,glowSignal*sourceSupport*_VfxLocalizedGlowAlpha*globalAlpha);
                return fixed4(rgb*alpha,alpha);
            }
            ENDCG
        }
    }
}
