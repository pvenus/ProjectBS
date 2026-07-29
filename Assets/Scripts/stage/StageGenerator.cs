using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// SVG SlotMap 기반 StageGraph 생성의 단일 진입점.
    /// </summary>
    public class StageGenerator
    {
        private readonly Dictionary<string, RoundNodeSO> lastAssignments =
            new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, RoundNodeSO> LastAssignments => lastAssignments;

        public StageGraph Generate(StageDefinitionSO definition)
        {
            lastAssignments.Clear();

            if (definition == null)
            {
                Debug.LogError("[StageGenerator] StageDefinitionSO is null.");
                return null;
            }

            var graph = new StageGraph(definition.stageId, definition.stageName);
            var generator = new StageSvgSlotMapGraphGenerator();
            if (!generator.Generate(definition, graph))
            {
                Debug.LogError("[StageGenerator] SVG SlotMap graph generation failed.");
                return null;
            }

            foreach (var assignment in generator.LastAssignments)
            {
                lastAssignments[assignment.Key] = assignment.Value;
            }

            graph.StartStage();
            return graph;
        }

    }
}
