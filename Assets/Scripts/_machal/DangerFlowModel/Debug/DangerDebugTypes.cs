using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActivationValueDebugEntry
{
	public string key;
	[Range(0f, 1f)] public float finalValue;
	[Range(0f, 1f)] public float directValue;
	[TextArea(1, 3)] public string reason;
}

[Serializable]
public class ActivationLayerDebugData
{
	public List<ActivationValueDebugEntry> values = new List<ActivationValueDebugEntry>();

	public void Clear()
	{
		values.Clear();
	}

	public void CopyFrom(ActivationSet finalSet, ActivationSet directSet = null)
	{
		values.Clear();

		if (finalSet == null)
			return;

		for (int i = 0; i < finalSet.Values.Count; i++)
		{
			var finalItem = finalSet.Values[i];
			float directValue = directSet != null ? directSet.Get(finalItem.key) : 0f;

			values.Add(new ActivationValueDebugEntry
			{
				key = finalItem.key,
				finalValue = Mathf.Clamp01(finalItem.value),
				directValue = Mathf.Clamp01(directValue),
				reason = finalItem.reason ?? string.Empty
			});
		}
	}
}