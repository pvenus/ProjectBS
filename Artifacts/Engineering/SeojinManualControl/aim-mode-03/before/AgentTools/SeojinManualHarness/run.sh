#!/bin/sh
set -eu
cd /Users/pvenus/ProjectBS
manual_mono=/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge
manual_out=$(mktemp -d /private/tmp/projectbs-manual.XXXXXX)
"$manual_mono/bin/mono" "$manual_mono/lib/mono/msbuild/Current/bin/Roslyn/csc.exe" -nologo -nowarn:0649 -langversion:9.0 -out:"$manual_out/test.exe" AgentTools/SeojinManualHarness/Stub.cs AgentTools/SeojinManualHarness/Tests.cs Assets/Scripts/Actor/Character/Control/*.cs
"$manual_mono/bin/mono" "$manual_out/test.exe"
