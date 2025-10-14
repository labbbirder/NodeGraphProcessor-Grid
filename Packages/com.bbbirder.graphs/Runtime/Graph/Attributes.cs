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
		public readonly string Name;
		public readonly bool Hide;
		public readonly bool Unpack;

		/// <summary>
		/// Mark the field as an input port
		/// </summary>
		/// <param name="name">display name</param>
		/// <param name="unpack">unpack array to elements</param>
		public InputAttribute(string name = null, bool unpack = false, bool hide = false)
		{
			this.Name = name;
			this.Hide = hide;
			this.Unpack = unpack;
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

		/// <summary>
		/// Mark the field as an output port
		/// </summary>
		/// <param name="name">display name</param>
		/// <param name="unpack">unpack array to elements</param>
		public OutputAttribute(string name = null, bool hide = false)
		{
			this.name = name;
			this.hide = hide;
		}
	}

	/// <summary>
	/// Creates a vertical port instead of the default horizontal one
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class VerticalAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CompatibleWithGraphAttribute : Attribute
	{
		public readonly Type GraphType;
		public CompatibleWithGraphAttribute(Type graphType = null)
		{
			this.GraphType = graphType;
		}
	}

	/// <summary>
	/// Register the node in the NodeProvider class. The node will also be available in the node creation window.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class NodeMenuItemAttribute : Attribute
	{
		public string menuTitle;

		/// <summary>
		/// Register the node in the NodeProvider class. The node will also be available in the node creation window.
		/// </summary>
		/// <param name="menuTitle">Path in the menu, use / as folder separators</param>
		public NodeMenuItemAttribute(string menuTitle = null)
		{
			this.menuTitle = menuTitle;
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
}
