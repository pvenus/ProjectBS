using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Stage;
using UnityEditor;

public sealed class Events21To46FunctionalQaTests
{
    private sealed class Case
    {
        public string JsonPath;
        public string EventId;
        public string NodeId;
        public string[] Tokens;
    }

    private static readonly Case[] Cases =
    {
        C(21, "breath_between_water_drops", "RandomGrowthSafe", "RandomGrowthDecline"),
        C(22, "sleeping_hawk_watch", "RandomGrowthRisk", "RandomGrowthDecline"),
        C(25, "hot_spring_beneath_ice", "BeginBattle", "InventoryGrant"),
        C(28, "rockfall_scouts", "battle.act1.event18.water_theft_guard",
            "CommitImmediateSuccessorRoute", "LongestRemainingToSectionExit"),
        C(29, "chain_bridge_tollkeepers", "GoldSpend", "\"amount\": 50",
            "battle.act1.event11.restore_waterway"),
        C(34, "half_vein_map", "NextEvent", "GoldGrant", "\"amount\": 75",
            "event.act1.random_event.34.half_vein_map.followup.unstable_vein"),
        C(39, "self_knotting_rope", "RelicRouteTrade", "item.relic.blunt_gear",
            "ShortestRemainingToSectionExit"),
        C(40, "jihan_empty_medicine_folio", "VitalDelta", "SetRunFlagTrue",
            "character.jihan"),
        C(45, "false_wildfire_boundary_stones", "SetRunFlagTrue",
            "BattlePurposeThenShortestRemainingToSectionExit", "CompleteEvent")
    };

    [Test]
    public void RepresentativeJsonAndPopupSoIdentity_AreProductionComplete()
    {
        foreach (Case item in Cases)
        {
            Assert.That(File.Exists(item.JsonPath), Is.True, item.JsonPath);
            string json = File.ReadAllText(item.JsonPath);
            string compact = Compact(json);
            Assert.That(compact, Does.Contain($"\"eventId\":\"{item.EventId}\""), item.JsonPath);
            Assert.That(compact, Does.Contain($"\"nodeId\":\"{item.NodeId}\""), item.JsonPath);
            foreach (string token in item.Tokens)
                Assert.That(compact, Does.Contain(Compact(token)), $"{item.EventId}: {token}");

            string soPath = $"Assets/Contents/Stage/so/{item.NodeId}.asset";
            PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(soPath);
            Assert.That(popup, Is.Not.Null, soPath);
            Assert.That(popup.eventId, Is.EqualTo(item.NodeId), soPath);
            Assert.That(popup.mainImage, Is.Not.Null, soPath);
        }
    }

    [Test]
    public void Event34ParentAndChild_AreDistinctAndImageBound()
    {
        const string parentPath =
            "Assets/Contents/Stage/so/node.act1.random_event.34.half_vein_map.intro.asset";
        const string childPath =
            "Assets/Contents/Stage/so/node.act1.random_event.34.half_vein_map.followup.unstable_vein.intro.asset";
        PopupEventSO parent = AssetDatabase.LoadAssetAtPath<PopupEventSO>(parentPath);
        PopupEventSO child = AssetDatabase.LoadAssetAtPath<PopupEventSO>(childPath);
        Assert.That(parent, Is.Not.Null);
        Assert.That(child, Is.Not.Null);
        Assert.That(parent, Is.Not.SameAs(child));
        Assert.That(parent.eventId,
            Is.EqualTo("node.act1.random_event.34.half_vein_map.intro"));
        Assert.That(child.eventId,
            Is.EqualTo("node.act1.random_event.34.half_vein_map.followup.unstable_vein.intro"));
        Assert.That(parent.mainImage, Is.Not.Null);
        Assert.That(child.mainImage, Is.Not.Null);
        Assert.That(parent.mainImage, Is.Not.SameAs(child.mainImage));
    }

    private static Case C(int number, string slug, params string[] tokens)
    {
        string eventId = $"event.act1.random_event.{number}.{slug}";
        return new Case
        {
            JsonPath = number <= 26
                ? $"Assets/Contents/Stage/json/event/act01/event.act1.{number}.{slug}.json"
                : $"Assets/Contents/Stage/json/event/act01/{eventId}.json",
            EventId = eventId,
            NodeId = $"node.act1.random_event.{number}.{slug}.intro",
            Tokens = tokens
        };
    }

    private static string Compact(string value) =>
        string.Concat(value.Where(character => !char.IsWhiteSpace(character)));
}
