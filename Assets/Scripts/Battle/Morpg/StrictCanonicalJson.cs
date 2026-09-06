using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Battle.Morpg
{
    public static class StrictCanonicalJson
    {
        public sealed class ObjectNode : SortedDictionary<string, object> { public ObjectNode() : base(StringComparer.Ordinal) {} }
        public sealed class ArrayNode : List<object> {}

        public static bool TryParse(string json, out object root, out string error)
        {
            try { var p=new Parser(json); root=p.Value(); p.End(); error=null; return true; }
            catch (Exception e) { root=null; error=e.Message; return false; }
        }

        public static string Canonicalize(object value)
        {
            var b=new StringBuilder(); Write(value,b); return b.ToString();
        }

        public static string Sha256(object value)
        {
            using (var sha=SHA256.Create())
            {
                byte[] hash=sha.ComputeHash(Encoding.UTF8.GetBytes(Canonicalize(value)));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static void Write(object v, StringBuilder b)
        {
            if (v==null) { b.Append("null"); return; }
            if (v is string s) { b.Append('"'); foreach(char c in s) { if(c=='"'||c=='\\') b.Append('\\').Append(c); else if(c=='\n') b.Append("\\n"); else if(c=='\r') b.Append("\\r"); else if(c=='\t') b.Append("\\t"); else if(c<32) b.Append("\\u").Append(((int)c).ToString("x4")); else b.Append(c); } b.Append('"'); return; }
            if (v is bool q) { b.Append(q ? "true" : "false"); return; }
            if (v is decimal n) { b.Append(n.ToString("G29",CultureInfo.InvariantCulture)); return; }
            if (v is ObjectNode o) { b.Append('{'); bool first=true; foreach(var e in o) { if(!first)b.Append(','); first=false; Write(e.Key,b); b.Append(':'); Write(e.Value,b); } b.Append('}'); return; }
            if (v is ArrayNode a) { b.Append('['); for(int i=0;i<a.Count;i++){if(i>0)b.Append(',');Write(a[i],b);} b.Append(']'); return; }
            throw new InvalidOperationException("unsupported JSON node");
        }

        private sealed class Parser
        {
            private readonly string s; private int i;
            public Parser(string value){s=value??throw new ArgumentNullException(nameof(value));}
            public object Value(){Ws(); if(i>=s.Length)throw Error("value expected"); char c=s[i]; if(c=='{')return Obj(); if(c=='[')return Arr(); if(c=='"')return Str(); if(c=='t'){Lit("true");return true;} if(c=='f'){Lit("false");return false;} if(c=='n'){Lit("null");return null;} return Num();}
            public void End(){Ws();if(i!=s.Length)throw Error("trailing JSON");}
            private ObjectNode Obj(){i++;var o=new ObjectNode();Ws();if(Take('}'))return o;while(true){Ws();string k=Str();Ws();Need(':');if(o.ContainsKey(k))throw Error("duplicate property: "+k);o.Add(k,Value());Ws();if(Take('}'))return o;Need(',');}}
            private ArrayNode Arr(){i++;var a=new ArrayNode();Ws();if(Take(']'))return a;while(true){a.Add(Value());Ws();if(Take(']'))return a;Need(',');}}
            private string Str(){Need('"');var b=new StringBuilder();while(i<s.Length){char c=s[i++];if(c=='"')return b.ToString();if(c!='\\'){b.Append(c);continue;}if(i>=s.Length)throw Error("bad escape");c=s[i++];if(c=='"'||c=='\\'||c=='/')b.Append(c);else if(c=='b')b.Append('\b');else if(c=='f')b.Append('\f');else if(c=='n')b.Append('\n');else if(c=='r')b.Append('\r');else if(c=='t')b.Append('\t');else if(c=='u'){if(i+4>s.Length)throw Error("bad unicode");b.Append((char)int.Parse(s.Substring(i,4),NumberStyles.HexNumber));i+=4;}else throw Error("bad escape");}throw Error("unterminated string");}
            private decimal Num(){int a=i;if(Take('-')){}while(i<s.Length&&char.IsDigit(s[i]))i++;if(Take('.'))while(i<s.Length&&char.IsDigit(s[i]))i++;if(i<s.Length&&(s[i]=='e'||s[i]=='E')){i++;if(i<s.Length&&(s[i]=='+'||s[i]=='-'))i++;while(i<s.Length&&char.IsDigit(s[i]))i++;}if(a==i||!decimal.TryParse(s.Substring(a,i-a),NumberStyles.Float,CultureInfo.InvariantCulture,out decimal n))throw Error("invalid number");return n;}
            private void Lit(string x){if(i+x.Length>s.Length||s.Substring(i,x.Length)!=x)throw Error("invalid literal");i+=x.Length;}
            private void Ws(){while(i<s.Length&&char.IsWhiteSpace(s[i]))i++;}
            private bool Take(char c){if(i<s.Length&&s[i]==c){i++;return true;}return false;}
            private void Need(char c){if(!Take(c))throw Error("expected "+c);}
            private Exception Error(string m)=>new FormatException(m+" at "+i);
        }
    }
}
