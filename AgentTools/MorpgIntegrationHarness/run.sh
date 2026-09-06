#!/bin/sh
set -eu
cd /Users/pvenus/ProjectBS
morpg_mono=/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge
morpg_out=$(mktemp -d /private/tmp/projectbs-morpg-live.XXXXXX)
"$morpg_mono/bin/mono" "$morpg_mono/lib/mono/msbuild/Current/bin/Roslyn/csc.exe" -nologo -langversion:9.0 -r:System.Web.Extensions.dll -out:"$morpg_out/live.exe" AgentTools/MorpgIntegrationHarness/Program.cs AgentTools/MorpgIntegrationHarness/RuntimeStub.cs AgentTools/MorpgP0Harness/UnityEngineStub.cs Assets/Scripts/Battle/Morpg/*.cs Assets/Scripts/Collection/Currency/CurrencyRutimeData.cs
"$morpg_mono/bin/mono" "$morpg_out/live.exe" /Users/pvenus/ProjectBS
