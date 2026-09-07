#!/bin/sh
set -eu
cd /Users/pvenus/ProjectBS
p1_output=$(mktemp -d /private/tmp/projectbs-p1-reproduce.XXXXXX)
p1_mono=/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge
"$p1_mono/bin/mono" "$p1_mono/lib/mono/msbuild/Current/bin/Roslyn/csc.exe" -nologo -langversion:9.0 -r:System.Web.Extensions.dll -out:"$p1_output/MorpgP1Harness.exe" AgentTools/MorpgP0Harness/Program.cs AgentTools/MorpgP0Harness/P1RuntimeTests.cs AgentTools/MorpgP0Harness/UnityEngineStub.cs Assets/Scripts/Battle/Morpg/BattleMorpgZoneRewardContracts.cs Assets/Scripts/Battle/Morpg/StrictCanonicalJson.cs Assets/Scripts/Battle/Morpg/BattleMorpgP0AddendumCodec.cs Assets/Scripts/Battle/Morpg/BattleMorpgZoneTransitionCoordinator.cs Assets/Scripts/Battle/Morpg/MorpgZoneRuntimeSlice.cs Assets/Scripts/Battle/Morpg/MorpgTransitionClock.cs
"$p1_mono/bin/mono" "$p1_output/MorpgP1Harness.exe" /Users/pvenus/ProjectBS > "$p1_output/run1.log"
"$p1_mono/bin/mono" "$p1_output/MorpgP1Harness.exe" /Users/pvenus/ProjectBS > "$p1_output/run2.log"
diff -u "$p1_output/run1.log" "$p1_output/run2.log"
cat "$p1_output/run2.log"
