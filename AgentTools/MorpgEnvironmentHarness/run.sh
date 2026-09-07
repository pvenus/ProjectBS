#!/bin/sh
set -eu
cd /Users/pvenus/ProjectBS
env_mono=/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge
env_out=$(mktemp -d /private/tmp/projectbs-environment.XXXXXX)
"$env_mono/bin/mono" "$env_mono/lib/mono/msbuild/Current/bin/Roslyn/csc.exe" -nologo -nowarn:0649 -langversion:9.0 -r:System.Web.Extensions.dll -out:"$env_out/test.exe" AgentTools/MorpgEnvironmentHarness/GeometryTests.cs AgentTools/MorpgP0Harness/UnityEngineStub.cs Assets/Scripts/Battle/Morpg/MorpgEnvironmentGeometry.cs Assets/Scripts/Battle/Morpg/BattleMorpgZoneRewardContracts.cs Assets/Scripts/Battle/Morpg/BattleMorpgP0AddendumCodec.cs Assets/Scripts/Battle/Morpg/StrictCanonicalJson.cs
"$env_mono/bin/mono" "$env_out/test.exe" /Users/pvenus/ProjectBS
