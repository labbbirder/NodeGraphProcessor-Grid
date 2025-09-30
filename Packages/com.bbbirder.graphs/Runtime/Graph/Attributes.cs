using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	public class CustomGraphView : Attribute
	{
		public readonly Type GraphType;
		public CustomGraphView(Type graphType)
		{
			this.GraphType = graphType;
		}
	}

	/// <summary>
	/// Tell that this field is will generate an input port
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InputAttribute : Attribute
	{
		public readonly string name;
		public readonly bool hide;
		public readonly bool allowMultiple = false;

		/// <summary>
		/// Mark the field as an input port
		/// </summary>
		/// <param name="name">display name</param>
		/// <param name="allowMultiple">is connecting multiple edges allowed</param>
		public InputAttribute(string name = null, bool allowMultiple = false, bool hide = false)
		{
			this.name = name;
			this.hide = hide;
			this.allowMultiple = allowMultiple;
		}
	}

	/// <summary>
	/// Tell that this field is will generate an output port
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class OutputAttribute : Attribute
	{
		public readonly string name;
		public readonly bool hide;
		public readonly bool allowMultiple = true;

		/// <summary>
		/// Mark the field as an output port
		/// </summary>
		/// <param name="name">display name</param>
		/// <param name="allowMultiple">is connecting multiple edges allowed</param>
		public OutputAttribute(string name = null, bool allowMultiple = true, bool hide = false)
		{
			this.name = name;
			this.hide = hide;
			this.allowMultiple = allowMultiple;
		}
	}

	/// <summary>
	/// Creates a vertical port instead of the default horizontal one
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class VerticalAttribute : Attribute
	{
	}

	/// <summary>
	/// Register the node in the NodeProvider class. The node will also be available in the node creation window.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class NodeMenuItemAttribute : Attribute
	{
		public string menuTitle;
		public Type onlyCompatibleWithGraph;

		/// <summary>
		/// Register the node in the NodeProvider class. The node will also be available in the node creation window.
		/// </summary>
		/// <param name="menuTitle">Path in the menu, use / as folder separators</param>
		public NodeMenuItemAttribute(string menuTitle = null, Type onlyCompatibleWithGraph = null)
		{
			this.menuTitle = menuTitle;
			this.onlyCompatibleWithGraph = onlyCompatibleWithGraph;
		}
	}

	/// <summary>
	/// Allow you to have a custom view for your stack nodes
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class CustomStackNodeView : Attribute
	{
		public Type stackNodeType;

		/// <summary>
		/// Allow you to have a custom view for your stack nodes
		/// </summary>
		/// <param name="stackNodeType">The type of the stack node you target</param>
		public CustomStackNodeView(Type stackNodeType)
		{
			this.stackNodeType = stackNodeType;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class VisibleIf : Attribute
	{
		public string fieldName;
		public object value;

		public VisibleIf(string fieldName, object value)
		{
			this.fieldName = fieldName;
			this.value = value;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class ShowInInspector : Attribute
	{
		public bool showInNode;

		public ShowInInspector(bool showInNode = false)
		{
			this.showInNode = showInNode;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class ShowAsDrawer : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class SettingAttribute : Attribute
	{
		public string name;

		public SettingAttribute(string name = null)
		{
			this.name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class IsCompatibleWithGraph : Attribute { }
}
