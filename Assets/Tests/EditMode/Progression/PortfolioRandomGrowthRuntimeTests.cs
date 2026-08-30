using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Progression.Portfolio;

namespace Progression.Tests
{
    public sealed class PortfolioRandomGrowthRuntimeTests
    {
        [TestCase("event.act1.random_event.21.breath_between_water_drops",
            "choice.act1.random_event.21.breath_between_water_drops.follow_silent_rhythm",
            RandomGrowthPayloadKind.Safe)]
        [TestCase("event.act1.random_event.22.sleeping_hawk_watch",
            "choice.act1.random_event.22.sleeping_hawk_watch.keep_night_watch",
            RandomGrowthPayloadKind.Risk)]
        [TestCase("event.act1.random_event.23.temple_hundred_eight_steps",
            "choice.act1.random_event.23.temple_hundred_eight_steps.carry_stone_to_summit",
            RandomGrowthPayloadKind.Risk)]
        public void B1RegistryAndTypedRuntimeIdentityAgree(string eventId,string choiceId,
            RandomGrowthPayloadKind kind)
        {
            Assert.That(RandomGrowthEventIdentityCatalog.TryResolve(eventId,choiceId,kind,out var identity),Is.True);
            Assert.That(Chapter1PortfolioManifestBuilder.PortfolioB1Registry.Any(x=>x.EventId==eventId),Is.True);
            Assert.That(identity.SourceId,Is.EqualTo(choiceId));
        }

        [Test]
        public void OwnershipIsEventKeyedExactlyOnceAndDoesNotAliasLegacySafe()
        {
            object ownership=NewOwnership();Invoke(ownership,"ResetForNewRun","run.b1");
            RandomGrowthEventIdentityCatalog.TryResolve("event.act1.random_event.22.sleeping_hawk_watch",
                "choice.act1.random_event.22.sleeping_hawk_watch.keep_night_watch",
                RandomGrowthPayloadKind.Risk,out var identity);
            object pending=Begin(ownership,identity,"node.instance.22",'a');
            Assert.That(InvokeBool(ownership,"TryApplying",pending),Is.True);
            Assert.That(Commit(ownership,pending,"Succeeded"),Is.True);
            Assert.That(InvokeBool(ownership,"IsTerminal",identity.EventId,"node.instance.22"),Is.True);
            Assert.That(TryBegin(ownership,identity,"node.instance.22",'a',out _),Is.False);
            Assert.That(SafeGrowthTransactionIds.EventId,Is.EqualTo("event.act1.random_growth.02.windworn_sword_marks"));
        }

        [Test]
        public void DifferentB1EventsMayOwnIndependentNodeTerminals()
        {
            object ownership=NewOwnership();Invoke(ownership,"ResetForNewRun","run.b1");
            var identities=RandomGrowthEventIdentityCatalog.Entries.Where(x=>x.PayloadKind==RandomGrowthPayloadKind.Risk
                &&x.EventId.StartsWith("event.act1.random_event.")).ToArray();
            foreach(var identity in identities)
            {object pending=Begin(ownership,identity,"instance."+identity.EventId,'b');
             Assert.That(InvokeBool(ownership,"TryApplying",pending),Is.True);Assert.That(Commit(ownership,pending,"Succeeded"),Is.True);}
            Assert.That(identities,Has.Length.EqualTo(2));
        }

        [Test]
        public void ExternalHealAndRiskShareOneEvent23Terminal()
        {
            object ownership=NewOwnership();Invoke(ownership,"ResetForNewRun","run.b1");
            Assert.That(InvokeBool(ownership,"TryCommitExternal",
                "event.act1.random_event.23.temple_hundred_eight_steps","node.instance.23"),Is.True);
            RandomGrowthEventIdentityCatalog.TryResolve(
                "event.act1.random_event.23.temple_hundred_eight_steps",
                "choice.act1.random_event.23.temple_hundred_eight_steps.carry_stone_to_summit",
                RandomGrowthPayloadKind.Risk,out var risk);
            Assert.That(TryBegin(ownership,risk,"node.instance.23",'c',out _),Is.False);
        }

        private static Type OwnershipType()=>Type.GetType(
            "Progression.PortfolioRandomGrowthInteractionOwnership, Assembly-CSharp",true);
        private static object NewOwnership()=>Activator.CreateInstance(OwnershipType());
        private static object Invoke(object target,string name,params object[] args)
        {
            MethodInfo method=target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.Public)
                ?? throw new AssertionException("Missing public runtime method: "+name);
            return method.Invoke(target,args);
        }
        private static bool InvokeBool(object target,string name,params object[] args)=>(bool)Invoke(target,name,args);
        private static bool TryBegin(object ownership,RandomGrowthEventIdentity identity,string node,char fingerprint,out object pending)
        {
            MethodInfo method=OwnershipType().GetMethod("TryBegin",BindingFlags.Instance|BindingFlags.Public)
                ?? throw new AssertionException("Missing public runtime method: TryBegin");
            object[] args={identity,node,"stage.b1",new string(fingerprint,64),null};
            bool result=(bool)method.Invoke(ownership,args);pending=args[4];return result;
        }
        private static object Begin(object ownership,RandomGrowthEventIdentity identity,string node,char fingerprint)
        {Assert.That(TryBegin(ownership,identity,node,fingerprint,out object pending),Is.True);return pending;}
        private static bool Commit(object ownership,object pending,string state)
        {
            Type enumType=Type.GetType("Progression.PortfolioRandomGrowthState, Assembly-CSharp",true);
            return InvokeBool(ownership,"TryCommit",pending,Enum.Parse(enumType,state));
        }
    }
}
