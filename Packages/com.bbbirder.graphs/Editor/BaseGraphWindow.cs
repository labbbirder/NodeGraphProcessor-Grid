using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	public class BaseGraphWindow : EditorWindow, ISupportsOverlays
	{
		const string graphWindowStylePath = "res/Styles/BaseGraphWindowView.uss";

		Dictionary<string, OverlayView> overlayViews = new();
		protected VisualElement rootView;
		internal protected BaseGraphView graphView;

		[SerializeField]
		internal protected UnityEngine.Object graphOwner;

		bool reloadWorkaround = false;

		public event Action<BaseGraph> graphLoaded;
		public event Action<BaseGraph> graphUnloaded;

		public BaseGraph Graph => (graphOwner as IGraphOwner)?.Graph;

		public bool IsGraphLoaded
		{
			get { return graphView != null && graphView.graph != null; }
		}

		/// <summary>
		/// Called by Unity when the window is enabled / opened
		/// </summary>
		protected virtual void OnEnable()
		{
			InitializeRootView();

			if (graphOwner != null)
				LoadGraph();
			else
				reloadWorkaround = true;
		}

		internal void RegisterOverlayView(string id, OverlayView overlayView)
		{
			overlayViews.Add(id, overlayView);
		}

		protected virtual void Update()
		{
			// Workaround for the Refresh option of the editor window:
			// When Refresh is clicked, OnEnable is called before the serialized data in the
			// editor window is deserialized, causing the graph view to not be loaded
			if (reloadWorkaround && graphOwner != null)
			{
				LoadGraph();
				reloadWorkaround = false;
			}

			if (graphOwner == null)
			{
				rootView.Remove(graphView);
				graphView = null;
			}
		}

		void LoadGraph()
		{
			// We wait for the graph to be initialized
			if (Graph.IsInitialized)
				InitializeGraph(graphOwner);
			else
				Graph.onEnabled += () => InitializeGraph(graphOwner);
		}

		/// <summary>
		/// Called by Unity when the window is disabled (happens on domain reload)
		/// </summary>
		protected virtual void OnDisable()
		{
			if (Graph != null && graphView != null)
				graphView.SaveGraphToDisk();
		}

		/// <summary>
		/// Called by Unity when the window is closed
		/// </summary>
		protected virtual void OnDestroy()
		{
			graphView?.Dispose();
		}

		void InitializeRootView()
		{
			rootView = base.rootVisualElement;

			rootView.name = "graphRootView";

			rootView.RegisterCallback<AttachToPanelEvent>(e =>
			{
				e.destinationPanel.visualTree.styleSheets.Add(ResUtils.Load<StyleSheet>(graphWindowStylePath));
			});

			rootView.StretchToParentSize();
		}

		public void InitializeGraph(UnityEngine.Object graphOwner)
		{
			rootView = base.rootVisualElement;

			if (this.graphOwner != null && graphOwner != this.graphOwner)
			{
				// Save the graph to the disk
				EditorUtility.SetDirty(this.graphOwner as UnityEngine.Object);
				AssetDatabase.SaveAssets();

				// Unload the graph
				graphUnloaded?.Invoke(this.Graph);
			}

			this.graphOwner = graphOwner;
			graphLoaded?.Invoke(Graph);

			if (graphView != null)
				rootView.Remove(graphView);

			//Create graph view
			var graphViewType = EditorTypeCache.GetGraphViewType(Graph.GetType()) ?? typeof(BaseGraphView);
			graphView = Activator.CreateInstance(graphViewType, (object)this) as BaseGraphView;
			rootView.Add(graphView);

			var title = graphView.Title;
			var icon = graphView.Icon;
			titleContent = icon ? new(title, icon) : new(title);

			graphView.Initialize(Graph, overlayViews);

			// InitializeGraphView(graphView);

			// TOOD: onSceneLinked...

			if (Graph.IsLinkedToScene())
				LinkGraphWindowToScene(Graph.GetLinkedScene());
			else
				Graph.onSceneLinked += LinkGraphWindowToScene;
		}

		void LinkGraphWindowToScene(Scene scene)
		{
			EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

			void CloseWindowWhenSceneIsClosed(Scene closedScene)
			{
				if (scene == closedScene)
				{
					Close();
					EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
				}
			}
		}

		internal void SetOverlayDisplayState(string id, bool state)
		{
			if (overlayViews.TryGetValue(id, out var view))
			{
				view.displayed = state;
			}
		}

		internal bool GetOverlayDisplayState(string id)
		{
			if (overlayViews.TryGetValue(id, out var view))
			{
				return view.displayed;
			}

			return false;
		}

		public virtual void OnGraphDeleted()
		{
			if (Graph != null && graphView != null)
				rootView.Remove(graphView);

			graphView = null;
		}

		// protected virtual void InitializeGraphView(BaseGraphView view) { }
	}
}
