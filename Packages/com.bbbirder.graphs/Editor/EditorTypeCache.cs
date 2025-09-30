using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace GraphProcessor
{
    public static class EditorTypeCache
    {
        public static Dictionary<Type, Type> s_graph2graphView;
        public static Type GetGraphViewType(Type graphType)
        {
            if (s_graph2graphView == null)
            {
                s_graph2graphView = new();
                var graphViewTypes = TypeCache.GetTypesDerivedFrom<BaseGraphView>();
                foreach (var graphViewType in graphViewTypes)
                {
                    var attrs = graphViewType.GetCustomAttributes<CustomGraphView>();
                    foreach (var attr in attrs)
                    {
                        s_graph2graphView[attr.GraphType] = graphViewType;
                    }
                }
            }

            return s_graph2graphView.GetValueOrDefault(graphType);
        }
    }
}