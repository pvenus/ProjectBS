#!/bin/sh
set -eu
cd /Users/pvenus/ProjectBS
p2_mono=/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge
p2_out=$(mktemp -d /private/tmp/projectbs-reward-delivery.XXXXXX)
"$p2_mono/bin/mono" "$p2_mono/lib/mono/msbuild/Current/bin/Roslyn/csc.exe" -nologo -langversion:9.0 -r:System.Web.Extensions.dll -out:"$p2_out/tests.exe" AgentTools/MorpgIntegrationHarness/reward_delivery_tests.cs AgentTools/MorpgP0Harness/UnityEngineStub.cs Assets/Scripts/Battle/Morpg/MorpgRewardDeliveryQueue.cs Assets/Scripts/Battle/Morpg/BattleMorpgZoneRewardContracts.cs Assets/Scripts/Battle/Morpg/StrictCanonicalJson.cs Assets/Scripts/Battle/Morpg/BattleMorpgP0AddendumCodec.cs
"$p2_mono/bin/mono" "$p2_out/tests.exe"
