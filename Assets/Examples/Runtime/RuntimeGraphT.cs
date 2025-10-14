using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace BBBirder.Graphs
{
	public class RuntimeGraph<T> : MonoBehaviour, IGraphOwner<T> where T : BaseGraph
	{
		public bool useTemplate;
		public GraphTemplate<T> template;
		public T graph;
		// [NonSerialized] public BaseGraphProcessor processor;

		public GameObject assignedGameObject;

		public T Graph => graph;

		[NonSerialized] bool isInited;
		private void EnsureInited()
		{
			if (isInited) return;
			isInited = true;

			if (useTemplate)
			{
				graph = template != null
					? JsonUtility.FromJson<T>(JsonUtility.ToJson(template.Graph))
					: null;
			}
		}

		private void Start()
		{
			EnsureInited();
			if (graph != null)
			{
				// var g = ScriptableObject.Instantiate(graph);
				// processor = g.CreateProcessor();
			}
		}

		// int i = 0;

		async void Update()
		{
			if (graph != null)
			{
				// graph.SetParameterValue("Input", (float)i++);
				// graph.SetParameterValue("GameObject", assignedGameObject);
				// processor.Run();
				// Debug.Log("Output: " + graph.GetParameterValue("Output"));
			}

		}

		public void CreateSerialized(out SerializedObject serializedObject, out SerializedProperty graphProperty)
		{
			if (useTemplate && !Application.isPlaying)
			{
				template.CreateSerialized(out serializedObject, out graphProperty);
			}
			else
			{
				serializedObject = new(this);
				graphProperty = serializedObject.FindProperty(nameof(graph));
			}
		}
	}
}
