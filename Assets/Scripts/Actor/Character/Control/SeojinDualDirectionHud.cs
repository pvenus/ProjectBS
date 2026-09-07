using UnityEngine;
using Battle.Presentation;
namespace Character.Control
{
    [DefaultExecutionOrder(1100)]
    [DisallowMultipleComponent]
    public sealed class SeojinDualDirectionHud:MonoBehaviour
    {
        private const string ResourceRoot="battle/Presentation/SeojinDualDirectionCrescent/revision-01/";
        // Presentation-only multiplier. Keep the red/blue lane ratio in CreateArc unchanged.
        internal const float FootprintScale=2f;
        private SeojinManualControl control;
        private CharacterManager actor;
        private BattleCharacterAuraView aura;
        private Transform ground,redPivot,bluePivot;
        private SpriteRenderer red,blue;
        private Camera cachedCamera;
        private float cameraRefresh;
        private Vector2 auraSpriteSize;
        private static Sprite redSprite,blueSprite;

        internal static void Ensure(SeojinManualControl owner,CharacterManager character)
        {
            var view=character.GetComponent<BattleCharacterAuraBinding>()?.AuraView;
            if(view==null)return;
            var hud=view.GetComponent<SeojinDualDirectionHud>()??view.gameObject.AddComponent<SeojinDualDirectionHud>();
            hud.Bind(owner,character,view);
        }
        private void Bind(SeojinManualControl owner,CharacterManager character,BattleCharacterAuraView view)
        {
            control=owner;actor=character;aura=view;
            if(redSprite==null)redSprite=Resources.Load<Sprite>(ResourceRoot+"attack-mouse-red");
            if(blueSprite==null)blueSprite=Resources.Load<Sprite>(ResourceRoot+"movement-keyboard-blue");
            if(ground==null)
            {
                ground=new GameObject("SeojinDirectionGround").transform;ground.SetParent(transform,false);
                red=CreateArc("MouseRed",redSprite,1.15f,out redPivot);
                blue=CreateArc("WasdBlue",blueSprite,1f,out bluePivot);
            }
            red.sprite=redSprite;blue.sprite=blueSprite;
            var baseSprite=aura.BackArcRenderer!=null?aura.BackArcRenderer.sprite:null;
            auraSpriteSize=baseSprite!=null?baseSprite.rect.size/baseSprite.pixelsPerUnit:new Vector2(2.56f,1.28f);
            Hide();cachedCamera=null;cameraRefresh=0f;
        }
        private SpriteRenderer CreateArc(string name,Sprite sprite,float radius,out Transform pivot)
        {
            pivot=new GameObject(name+"Pivot").transform;pivot.SetParent(ground,false);
            var child=new GameObject(name).transform;child.SetParent(pivot,false);child.localScale=Vector3.one*radius;
            var renderer=child.gameObject.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=Color.white;renderer.enabled=false;
            return renderer;
        }
        private void LateUpdate()
        {
            // The owner suspends all manual intent over UI, so both HUD lanes hide together.
            if(aura==null||!aura.isActiveAndEnabled||
                !((aura.BackArcRenderer!=null&&aura.BackArcRenderer.enabled)||(aura.FrontArcRenderer!=null&&aura.FrontArcRenderer.enabled))||
                SeojinInputReader.UiBlocked||control==null||!control.ManualAuthorized||actor==null)
            {Hide();return;}
            if(cachedCamera==null||!cachedCamera.isActiveAndEnabled||Time.unscaledTime>=cameraRefresh)
            {cachedCamera=Camera.main;cameraRefresh=Time.unscaledTime+1f;}
            var input=SeojinInputReader.Read(cachedCamera,default,actor.transform.position);
            var mouse=input.Aim.AtOrigin(input.CasterPosition).Direction;
            // Same ground projection as the aura, with the red radial lane outside blue.
            ground.localPosition=aura.PositionOffset;
            ground.localScale=new Vector3(auraSpriteSize.x*Mathf.Abs(aura.VisualScale.x)*1.2f*FootprintScale/5.12f,
                auraSpriteSize.y*Mathf.Abs(aura.VisualScale.y)*1.2f*FootprintScale/5.12f,1f);
            // Undo the floor ellipse before atan2 so the visible tip still points at the world input.
            UpdateArc(red,redPivot,ProjectToGround(mouse,ground.localScale));
            UpdateArc(blue,bluePivot,ProjectToGround(input.Axes,ground.localScale));
            bool morpg=Battle.Morpg.MorpgEnvironmentRuntime.Active!=null;
            int order=morpg?Battle.BattlePresentationSortingPolicy.MorpgGroundDirectionHud:
                Battle.BattlePresentationSortingPolicy.GroundDirectionHud;
            red.sortingLayerID=blue.sortingLayerID=Battle.BattlePresentationSortingPolicy.MorpgSortingLayerId;
            red.sortingOrder=blue.sortingOrder=order;
        }
        internal static ControlVector ProjectToGround(ControlVector direction,Vector3 scale)
            =>Mathf.Abs(scale.x)>.0001f&&Mathf.Abs(scale.y)>.0001f
                ?new ControlVector(direction.X/scale.x,direction.Y/scale.y):default;
        internal static void UpdateArc(SpriteRenderer renderer,Transform pivot,ControlVector direction)
        {
            if(renderer==null||pivot==null)return;
            bool visible=renderer.sprite!=null&&direction.Valid&&direction.Nonzero;
            renderer.enabled=visible;
            pivot.localRotation=visible?Quaternion.Euler(0f,0f,Mathf.Atan2(direction.Y,direction.X)*Mathf.Rad2Deg):Quaternion.identity;
        }
        private void Hide()
        {
            UpdateArc(red,redPivot,default);UpdateArc(blue,bluePivot,default);
        }
        private void OnDisable(){Hide();cachedCamera=null;cameraRefresh=0f;}
    }
}
