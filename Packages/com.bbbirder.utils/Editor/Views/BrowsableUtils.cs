using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static BBBirder.BrowsableWindow;

namespace BBBirder
{
    public static class BrowsableUtils
    {
        public static void SortItemSource(BrowsableList root, Comparison<object> comparison)
        {
            root.Sort(comparison);

            foreach (var ele in root)
            {
                if (ele is BrowsableList lst)
                {
                    SortItemSource(lst, comparison);
                }
            }
        }

        public static void AppendItemSource(BrowsableList root, string folderName, object item)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                root.Add(item);
            }
            else
            {
                var folder = root.Find(e => e is BrowsableList l && l.FolderName == folderName) as BrowsableList;
                if (folder != null)
                {
                    folder.Add(item);
                }
                else
                {
                    var list = new BrowsableList()
                    {
                        FolderName = folderName,
                    };
                    list.Add(item);
                    root.Add(list);
                }
            }
        }

    }
}
