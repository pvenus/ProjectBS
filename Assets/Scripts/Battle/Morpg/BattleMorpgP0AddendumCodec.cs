using System;
using System.Collections.Generic;

namespace Battle.Morpg
{
    public static class BattleMorpgRootShape
    {
        private static readonly string[] Root={"schemaVersion","flowId","dropProfileId","hudProfileId","battleId","fallbackProfileId","zones","drops","expected","suppressionIds","backtracking"};
        private static readonly string[] Zone={"index","id","waveId","entryWarpAnchorId","bounds","spawnBounds","entry","counts","radius","reservations"};
        private static readonly string[] Counts={"black","chain"};
        private static readonly string[] Reservation={"reservationId","sourceZoneId","unitKey","localDueTime","positionCandidateKey","position"};
        private static readonly string[] Drop={"unitKey","gold","xp"};
        private static readonly string[] Expected={"deaths","gold","xp"};
        public static bool TryValidate(string json,out string error)
        {
            if(!StrictCanonicalJson.TryParse(json,out object n,out error)||!(n is StrictCanonicalJson.ObjectNode root))return false;
            if(!Keys(root,Root,out error))return false;
            if(!root.TryGetValue("zones",out object zv)||!(zv is StrictCanonicalJson.ArrayNode zones))return Fail("zones array missing",out error);
            foreach(object item in zones)
            {
                if(!(item is StrictCanonicalJson.ObjectNode zone)||!Keys(zone,Zone,out error)||
                   !Object(zone,"counts",Counts,out error)||!Objects(zone,"reservations",Reservation,out error,null))return false;
            }
            if(!Objects(root,"drops",Drop,out error,null))return false;
            return Object(root,"expected",Expected,out error);
        }
        private delegate bool Extra(StrictCanonicalJson.ObjectNode o);
        private static bool Objects(StrictCanonicalJson.ObjectNode p,string key,string[] fields,out string error,Extra extra){error=null;if(!p.TryGetValue(key,out object v)||!(v is StrictCanonicalJson.ArrayNode a))return Fail(key+" array missing",out error);foreach(object n in a){if(!(n is StrictCanonicalJson.ObjectNode o)||!Keys(o,fields,out error)||extra!=null&&!extra(o))return false;}return true;}
        private static bool Object(StrictCanonicalJson.ObjectNode p,string key,string[] fields,out string error){error=null;if(!p.TryGetValue(key,out object v)||!(v is StrictCanonicalJson.ObjectNode o))return Fail(key+" object missing",out error);return Keys(o,fields,out error);}
        private static bool Keys(StrictCanonicalJson.ObjectNode o,string[] expected,out string error){error=null;var set=new HashSet<string>(expected,StringComparer.Ordinal);foreach(string k in o.Keys)if(!set.Remove(k))return Fail("unknown property: "+k,out error);return set.Count==0||Fail("missing property: "+string.Join(",",set),out error);}
        private static bool Fail(string m,out string error){error=m;return false;}
    }

    public static class BattleMorpgP0AddendumCodec
    {
        public const string Schema = "battle-morpg-zone-reward.p0-addendum.v1";
        private static readonly string[] RootFields = {"schemaVersion","documentKind","baseReceiptSha","dependencies","generatorVersion","waveRows","anchorRows","keyFormats","stateGraphs","checkpointMatrix","identityEquivalence","limits","overrides","acceptanceNames","changeLog","sourceAuthority","contentDigest"};
        private static readonly Dictionary<string,string[]> ChildFields = new(StringComparer.Ordinal)
        {
            ["waveRows"]=new[]{"zoneId","waveId"}, ["anchorRows"]=new[]{"zoneId","entryWarpAnchorId"},
            ["dependencies"]=new[]{"name","sha256"}, ["keyFormats"]=new[]{"name","format"},
            ["stateGraphs"]=new[]{"name","states"}, ["checkpointMatrix"]=new[]{"state","durableSave","reload"},
            ["identityEquivalence"]=new[]{"name","format"}, ["limits"]=new[]{"name","value"},
            ["overrides"]=new[]{"path","value"}, ["changeLog"]=new[]{"fieldPath","owner","consumer","priorState","newValue","compatibility","sourceReceiptSha"}
        };

        public static bool TryValidate(string json, out string digest, out string error)
        {
            digest=null;
            if(!StrictCanonicalJson.TryParse(json,out object node,out error)||!(node is StrictCanonicalJson.ObjectNode root))return false;
            if(!ExactKeys(root,RootFields,out error))return false;
            if(!Text(root,"schemaVersion",out string schema)||schema!=Schema)return Fail("schemaVersion mismatch",out error);
            if(!Text(root,"documentKind",out string kind)||kind!="strict-fragment")return Fail("documentKind mismatch",out error);
            foreach(var pair in ChildFields)
                if(!ArrayObjects(root,pair.Key,pair.Value,out error))return false;
            if(!StringArray(root,"acceptanceNames",out error)||!StringArray(root,"sourceAuthority",out error))return false;
            if(!Text(root,"contentDigest",out string expected)||expected.Length!=64)return Fail("contentDigest missing",out error);
            root.Remove("contentDigest"); digest=StrictCanonicalJson.Sha256(root); root["contentDigest"]=expected;
            return digest==expected || Fail("contentDigest mismatch",out error);
        }

        private static bool ExactKeys(StrictCanonicalJson.ObjectNode o,string[] expected,out string error){error=null;var set=new HashSet<string>(expected,StringComparer.Ordinal);foreach(string k in o.Keys)if(!set.Remove(k))return Fail("unknown property: "+k,out error);return set.Count==0||Fail("missing property: "+string.Join(",",set),out error);}
        private static bool ArrayObjects(StrictCanonicalJson.ObjectNode root,string key,string[] fields,out string error){error=null;if(!root.TryGetValue(key,out object v)||!(v is StrictCanonicalJson.ArrayNode a))return Fail(key+" array missing",out error);foreach(object n in a)if(!(n is StrictCanonicalJson.ObjectNode o)||!ExactKeys(o,fields,out error))return false;return true;}
        private static bool StringArray(StrictCanonicalJson.ObjectNode root,string key,out string error){if(!root.TryGetValue(key,out object v)||!(v is StrictCanonicalJson.ArrayNode a))return Fail(key+" array missing",out error);foreach(object n in a)if(!(n is string))return Fail(key+" requires strings",out error);error=null;return true;}
        private static bool Text(StrictCanonicalJson.ObjectNode root,string key,out string value){value=null;return root.TryGetValue(key,out object v)&&(value=v as string)!=null;}
        private static bool Fail(string message,out string error){error=message;return false;}
    }
}
