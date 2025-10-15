using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BBBirder
{
    public class BrowsableWindow : EditorWindow
    {
        ListView lstView;
        VisualElement veNav, foot;
        Label txtNav, txtFootContent, txtFootTitle;
        VisualElement imgFoot;

        public event Action<object> onCommit;
        /// <summary>
        /// child element named `imgRight` will be rendered as a right arrow.
        /// </summary>
        public event Func<VisualElement> onMakeItem;
        public event Func<VisualElement, DescriptionInfo?> onGetDescription;
        /// <summary>
        /// passes (VisualElement item, int indexOfData), the data item is store in `item.userData`
        /// </summary>
        public event Action<VisualElement, int> onBindItem;

        ///<inheritdoc cref="DataFilter"/>
        public event DataFilter onFilterData;

        /// <summary>
        /// Whether the data item can be filtered through.
        /// </summary>
        /// <param name="data">the data in BrowsableList (without BrowsableList-type item)</param>
        /// <param name="filter">the searching text</param>
        /// <returns></returns>
        public delegate bool DataFilter(object data, string filter);

        void CreateGUI()
        {
            const int DelaySearchMS = 180;

            ResUtils.Load<VisualTreeAsset>("../Templates/BrowsableView.uxml").CloneTree(rootVisualElement);

            rootVisualElement.name = "InstructionBrowseView";

            var txtSearch = rootVisualElement.Q<TextField>("txtSearch");
            // var treeView = rootVisualElement.Q<TreeView>();
            lstView = rootVisualElement.Q<ListView>();
            veNav = rootVisualElement.Q<VisualElement>("nav");
            txtNav = rootVisualElement.Q<Label>("txtNav");
            foot = rootVisualElement.Q("Foot");
            imgFoot = rootVisualElement.Q("imgFoot");
            txtFootTitle = rootVisualElement.Q<Label>("txtFootTitle");
            txtFootContent = rootVisualElement.Q<Label>("txtFootContent");

            IVisualElementScheduledItem scheduledItem = null;
            txtSearch.RegisterValueChangedCallback(e =>
            {
                var ve = e.currentTarget as TextField;

                scheduledItem?.Pause();
                scheduledItem = null;

                scheduledItem = ve.schedule.Execute(() =>
                {
                    if (!string.IsNullOrEmpty(ve.value))
                    {
                        ShowSeachedPage(ve.value);
                    }
                    else
                    {
                        ShowPage(navigation[navigation.Count - 1]);
                    }
                });
                scheduledItem.ExecuteLater(DelaySearchMS);
            });

            lstView.makeItem += MakeItem;
            lstView.bindItem += BindItem;

            rootVisualElement.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    PopPage();
                    e.StopPropagation();
                }
            });

            SetDescriptionInfo(null);

            veNav.RegisterCallback<ClickEvent>(e =>
            {
                PopPage();
            });

            txtSearch.Focus();
        }


        private List<BrowsableList> navigation = new();
        private BrowsableList searchedItems = new();
        private VisualElement descriptingItem;

        void ShowPage(BrowsableList list)
        {
            lstView.itemsSource = list;
            lstView.RefreshItems();

            var isSearchedList = list == searchedItems;

            veNav.style.display = list == navigation[0] || isSearchedList
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            if (!isSearchedList)
            {
                txtNav.text = string.Join("/", navigation.Skip(1).Select(l => l.FolderName));
            }
        }

        void PushPage(BrowsableList list)
        {
            navigation.Add(list);
            ShowPage(list);
        }

        void PopPage()
        {
            if (navigation.Count > 1)
            {
                navigation.RemoveAt(navigation.Count - 1);
            }

            ShowPage(navigation[navigation.Count - 1]);
        }

        void ShowSeachedPage(string filter)
        {
            searchedItems.Clear();
            CollectTree(navigation[0], filter, searchedItems);
            ShowPage(searchedItems);
        }

        void CollectTree(BrowsableList list, string filter, BrowsableList results)
        {
            foreach (var ele in list)
            {
                if (ele is BrowsableList sub)
                {
                    CollectTree(sub, filter, results);
                }
                else
                {
                    if (onFilterData(ele, filter))
                    {
                        results.Add(ele);
                    }
                }
            }
        }

        void Commit(object result)
        {
            onCommit?.Invoke(result);
            this.Close();
        }

        VisualElement MakeItem()
        {
            var ve = onMakeItem();
            ve.AddToClassList("BrowsableItem");
            ve.RegisterCallback<ClickEvent>(e =>
            {
                lstView.SetSelection(Array.Empty<int>());
                var ve = e.currentTarget as VisualElement;
                if (ve.userData is BrowsableList list)
                {
                    PushPage(list);
                }
                else
                {
                    Commit(ve.userData);
                }

                e.StopPropagation();
            });

            ve.RegisterCallback<PointerEnterEvent>(e =>
            {
                if (onGetDescription == null) return;

                var desc = onGetDescription(descriptingItem = e.currentTarget as VisualElement);
                SetDescriptionInfo(desc);
            });

            ve.RegisterCallback<PointerLeaveEvent>(e =>
            {
                if (onGetDescription == null) return;
                if (ReferenceEquals(descriptingItem, e.currentTarget))
                {
                    SetDescriptionInfo(null);
                }
            });

            return ve;
        }

        void SetDescriptionInfo(DescriptionInfo? desc)
        {
            foot.style.display = desc == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (desc is { } d)
            {
                imgFoot.style.backgroundImage = Background.FromTexture2D(d.titleIcon);
                txtFootTitle.text = d.title;
                txtFootContent.text = d.description;
            }
        }

        void BindItem(VisualElement ve, int index)
        {
            var data = lstView.itemsSource[index];
            ve.userData = data;
            ve.EnableInClassList("folder", data is BrowsableList);
            onBindItem?.Invoke(ve, index);
        }

        public void SetSource(BrowsableList list)
        {
            navigation.Clear();
            PushPage(list);
        }

        public static BrowsableWindow Open(Rect btnRect, Action<object> onCommit)
        {
            var window = EditorWindow.CreateInstance<BrowsableWindow>();
            window.onCommit = onCommit;
            // window.Show();
            // return window;
            window.ShowAsDropDown(new(
                focusedWindow.position.x + btnRect.x,
                focusedWindow.position.y + btnRect.y,
                btnRect.width,
                btnRect.height
            ), new(MathF.Max(btnRect.width, 240), 420));
            return window;
        }


        public struct DescriptionInfo
        {
            public Texture2D titleIcon;
            public string title;
            public string description;
        }

        public class BrowsableList : List<object>
        {
            public string FolderName { get; internal set; }
        }
    }
}
