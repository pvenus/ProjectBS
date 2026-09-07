using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Morpg
{
    // Explicitly registered legacy renderers only; never search scene objects by name or touch assets.
    internal sealed class MorpgLegacyBackground : MonoBehaviour
    {
        private static readonly List<MorpgLegacyBackground> registrations=new();
        private static readonly Dictionary<BattleRuntime,int> leases=new();
        private BattleRuntime owner;
        private SpriteRenderer sprite;
        private bool hidden,baselineEnabled;
        internal static void Register(BattleRuntime owner,SpriteRenderer renderer)
        {
            if(owner==null || renderer==null)return;
            var marker=renderer.gameObject.AddComponent<MorpgLegacyBackground>();marker.owner=owner;marker.sprite=renderer;
            registrations.Add(marker);
            if(leases.ContainsKey(owner))marker.Hide();
        }
        private void Hide(){if(hidden||sprite==null)return;baselineEnabled=sprite.enabled;hidden=true;sprite.enabled=false;}
        private void Restore(){if(!hidden)return;if(sprite!=null)sprite.enabled=baselineEnabled;hidden=false;}
        private void OnDestroy()=>registrations.Remove(this);
        internal static IDisposable Acquire(BattleRuntime owner)
        {
            leases.TryGetValue(owner,out int count);leases[owner]=count+1;
            registrations.RemoveAll(r=>r==null);
            foreach(var marker in registrations)if(ReferenceEquals(marker.owner,owner))marker.Hide();
            return new Lease(owner);
        }
        private sealed class Lease:IDisposable
        {
            private BattleRuntime owner;
            internal Lease(BattleRuntime owner){this.owner=owner;}
            public void Dispose()
            {
                if(owner==null)return;var key=owner;owner=null;
                if(!leases.TryGetValue(key,out int count))return;
                if(count>1){leases[key]=count-1;return;}
                leases.Remove(key);
                foreach(var marker in registrations)if(marker!=null&&ReferenceEquals(marker.owner,key))marker.Restore();
            }
        }
    }
}
