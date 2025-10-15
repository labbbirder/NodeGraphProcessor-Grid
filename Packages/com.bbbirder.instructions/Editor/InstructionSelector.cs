using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using static BBBirder.BrowsableWindow;

namespace BBBirder.Instructions
{
    public static class InstructionSelector
    {
        class InstructionInfo
        {
            public Type type;
            public string displayName;
            public string categoryName;
            public string description;
        }

        static BrowsableList s_instructionInfos;

        static void EnsureInstructionInfosBuilt()
        {
            if (s_instructionInfos != null) return;

            s_instructionInfos = new();

            foreach (var type in InstructionsRegistry.GetValidInstructions(typeof(void)))
            {
                var path = type.GetCustomAttribute<CategoryAttribute>()?.Path;
                var desc = type.GetCustomAttribute<DescriptionAttribute>()?.Description;
                var displayName = type.Name;
                var categoryName = "";
                if (string.IsNullOrEmpty(path))
                {
                    // do nothing...
                }
                else if (path.IndexOf("/") is int ichar and not ~0)
                {
                    categoryName = path[..ichar];
                    displayName = path[-~ichar..];
                }
                else
                {
                    categoryName = path;
                }

                BrowsableUtils.AppendItemSource(s_instructionInfos, categoryName, new InstructionInfo()
                {
                    type = type,
                    categoryName = categoryName,
                    displayName = displayName,
                    description = desc,
                });
            }

            BrowsableUtils.SortItemSource(s_instructionInfos, (lhs, rhs) =>
            {
                var lw = lhs is BrowsableList ? 0 : 1;
                var rw = rhs is BrowsableList ? 0 : 1;
                if (lw != rw)
                {
                    return lw - rw;
                }

                if (lhs is BrowsableList)
                {
                    return (lhs as BrowsableList).FolderName
                        .CompareTo((rhs as BrowsableList).FolderName);
                }
                else
                {
                    return (lhs as InstructionInfo).displayName
                        .CompareTo((rhs as InstructionInfo).displayName);
                }
            });
        }

        public static void ShowInstructionBrowsable(VisualElement btn, Action<Type> onCommit)
        {
            var window = BrowsableWindow.Open(btn.worldBound, result =>
            {
                var info = result as InstructionInfo;
                onCommit?.Invoke(info.type);
            });

            window.onMakeItem += () =>
            {
                var uiItem = new VisualElement();

                uiItem.Add(new Image() { style = { width = 21, height = 21 } });
                uiItem.Add(new Label() { style = { flexGrow = 1 } });
                uiItem.Add(new() { name = "imgRight", style = { width = 16, height = 16 } });

                return uiItem;
            };

            window.onBindItem += (ve, i) =>
            {
                var data = ve.userData;
                var lbl = ve.Q<Label>();
                var img = ve.Q<Image>();
                if (data is BrowsableList lst)
                {
                    img.image = ResUtils.Load<Texture2D>("./Textures/material-symbols--folder.png");
                    lbl.text = lst.FolderName;
                }
                else
                {
                    img.image = ResUtils.Load<Texture2D>("./Textures/mdi--division-box.png");
                    var info = data as InstructionInfo;
                    lbl.text = info.displayName;
                }
            };

            window.onFilterData += (data, filter) =>
            {
                var info = data as InstructionInfo;
                return info.displayName.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
            };

            window.onGetDescription += ve =>
            {
                var data = ve.userData;
                if (data is BrowsableList)
                {
                    return null;
                }
                else
                {
                    var info = data as InstructionInfo;
                    return new BrowsableWindow.DescriptionInfo()
                    {
                        titleIcon = ve.Q<Image>().image as Texture2D,
                        title = info.displayName,
                        description = info.description,
                    };
                }
            };

            EnsureInstructionInfosBuilt();

            window.SetSource(s_instructionInfos);

        }
    }

}
