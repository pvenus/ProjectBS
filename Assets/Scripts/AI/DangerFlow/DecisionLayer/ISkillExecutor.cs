using UnityEngine;

public interface ISkillExecutor
{
	bool Execute(SkillBrainOutput output, Transform caster);
}