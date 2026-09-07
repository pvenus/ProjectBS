#!/bin/sh
set -eu
cd /Users/pvenus/ProjectBS
dash_mono=/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge
dash_out=$(mktemp -d /private/tmp/projectbs-dash.XXXXXX)
"$dash_mono/bin/mono" "$dash_mono/lib/mono/msbuild/Current/bin/Roslyn/csc.exe" -nologo -langversion:9.0 -out:"$dash_out/test.exe" AgentTools/MorpgDashHarness/ClockTests.cs Assets/Scripts/Battle/Morpg/MorpgTransitionClock.cs
"$dash_mono/bin/mono" "$dash_out/test.exe"
