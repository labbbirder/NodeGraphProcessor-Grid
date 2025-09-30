using System;
using GraphProcessor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;

[Overlay(
    id = Id,
    displayName = "Blackboard",
    editorWindowType = typeof(BaseGraphWindow),
    defaultDisplay = true,
    defaultDockZone = DockZone.LeftToolbar,
    defaultDockPosition = DockPosition.Top,
    defaultDockIndex = 1,
    defaultLayout = Layout.Panel
)]
public class ExecutionBlackboardView : BlackboardView
{

}


[Overlay(
    id = Id,
    displayName = "Toolbar",
    editorWindowType = typeof(BaseGraphWindow),
    defaultDisplay = true,
    defaultDockZone = DockZone.TopToolbar,
    defaultDockPosition = DockPosition.Top,
    defaultDockIndex = 1,
    defaultLayout = Layout.HorizontalToolbar
)]
public class ExecutionToolbarView : ToolbarView
{

}
