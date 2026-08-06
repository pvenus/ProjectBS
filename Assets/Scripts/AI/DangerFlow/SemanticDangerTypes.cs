using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SemanticDangerContext
{
	[Header("Self")]
	[Range(0f, 1f)] public float selfHp01;

	[Header("Allies")]
	[Range(0f, 1f)] public float lowestAllyHp01;
	public bool hasHealTarget;

	[Header("Enemies")]
	public int nearbyEnemyCount;
	[Range(0f, 1f)] public float enemyCluster01;
	public bool isEncircled;
	public bool hasBossNearby;
	[Range(0f, 1f)] public float averageEnemyHp01;
}

public enum SemanticSituationType
{
	None = 0,
	Swarm = 1,
	Weak = 2,
	Encircle = 3,
	AllyFragile = 4,
	SelfFragile = 5,
	Boss = 6
}

public enum SemanticDangerType
{
	None = 0,
	Immediate = 1,
	Linked = 2,
	Ambient = 3
}

public enum SemanticProblemType
{
	None = 0,
	Survival = 1,
	Rescue = 2,
	Relief = 3,
	Opportunity = 4
}

public enum SemanticSolutionType
{
	None = 0,
	Guard = 1,
	Control = 2,
	Support = 3,
	Burst = 4
}

public enum SemanticTacticType
{
	None = 0,
	Protect = 1,
	Recover = 2,
	Control = 3,
	Eliminate = 4,
	Retreat = 5
}

[Serializable]
public struct ActivationValue
{
	public string key;
	public float value;
	public string reason;

	public ActivationValue(string key, float value, string reason)
	{
		this.key = key;
		this.value = Mathf.Clamp01(value);
		this.reason = reason ?? string.Empty;
	}
}

[Serializable]
public class ActivationSet
{
	[SerializeField] private List<ActivationValue> values = new List<ActivationValue>();

	public IReadOnlyList<ActivationValue> Values => values;

	public void Clear()
	{
		values.Clear();
	}

	public void Set(string key, float value, string reason)
	{
		value = Mathf.Clamp01(value);

		for (int i = 0; i < values.Count; i++)
		{
			if (values[i].key == key)
			{
				values[i] = new ActivationValue(key, value, reason);
				return;
			}
		}

		values.Add(new ActivationValue(key, value, reason));
	}

	public float Get(string key)
	{
		for (int i = 0; i < values.Count; i++)
		{
			if (values[i].key == key)
				return values[i].value;
		}

		return 0f;
	}

	public string GetReason(string key)
	{
		for (int i = 0; i < values.Count; i++)
		{
			if (values[i].key == key)
				return values[i].reason;
		}

		return string.Empty;
	}

	public static ActivationSet CreateSituationSet()
	{
		var set = new ActivationSet();
		set.Set(SemanticSituationType.Swarm.ToString(), 0f, "");
		set.Set(SemanticSituationType.Weak.ToString(), 0f, "");
		set.Set(SemanticSituationType.Encircle.ToString(), 0f, "");
		set.Set(SemanticSituationType.AllyFragile.ToString(), 0f, "");
		set.Set(SemanticSituationType.SelfFragile.ToString(), 0f, "");
		set.Set(SemanticSituationType.Boss.ToString(), 0f, "");
		return set;
	}

	public static ActivationSet CreateDangerSet()
	{
		var set = new ActivationSet();
		set.Set(SemanticDangerType.Immediate.ToString(), 0f, "");
		set.Set(SemanticDangerType.Linked.ToString(), 0f, "");
		set.Set(SemanticDangerType.Ambient.ToString(), 0f, "");
		return set;
	}

	public static ActivationSet CreateProblemSet()
	{
		var set = new ActivationSet();
		set.Set(SemanticProblemType.Survival.ToString(), 0f, "");
		set.Set(SemanticProblemType.Rescue.ToString(), 0f, "");
		set.Set(SemanticProblemType.Relief.ToString(), 0f, "");
		set.Set(SemanticProblemType.Opportunity.ToString(), 0f, "");
		return set;
	}

	public static ActivationSet CreateSolutionSet()
	{
		var set = new ActivationSet();
		set.Set(SemanticSolutionType.Guard.ToString(), 0f, "");
		set.Set(SemanticSolutionType.Control.ToString(), 0f, "");
		set.Set(SemanticSolutionType.Support.ToString(), 0f, "");
		set.Set(SemanticSolutionType.Burst.ToString(), 0f, "");
		return set;
	}

	public static ActivationSet CreateTacticSet()
	{
		var set = new ActivationSet();
		set.Set(SemanticTacticType.Protect.ToString(), 0f, "");
		set.Set(SemanticTacticType.Recover.ToString(), 0f, "");
		set.Set(SemanticTacticType.Control.ToString(), 0f, "");
		set.Set(SemanticTacticType.Eliminate.ToString(), 0f, "");
		set.Set(SemanticTacticType.Retreat.ToString(), 0f, "");
		return set;
	}
}

[Serializable]
public class SemanticDangerEvaluationResult
{
	public SemanticDangerContext context;
	public bool useSituation;
	public ActivationSet situation;
	public ActivationSet directDanger;
	public ActivationSet finalDanger;
	public ActivationSet directProblem;
	public ActivationSet finalProblem;
	public ActivationSet directSolution;
	public ActivationSet finalSolution;
	public ActivationSet directTactic;
	public ActivationSet finalTactic;

	public SemanticDangerEvaluationResult()
	{
		situation = ActivationSet.CreateSituationSet();
		directDanger = ActivationSet.CreateDangerSet();
		finalDanger = ActivationSet.CreateDangerSet();
		directProblem = ActivationSet.CreateProblemSet();
		finalProblem = ActivationSet.CreateProblemSet();
		directSolution = ActivationSet.CreateSolutionSet();
		finalSolution = ActivationSet.CreateSolutionSet();
		directTactic = ActivationSet.CreateTacticSet();
		finalTactic = ActivationSet.CreateTacticSet();
	}
}