using MHAchievManager.Forms;
using MHAchievManager.Locale;
using MHAchievManager.Models;
using MHAchievManager.Services;
using MHAchievManager.UI;
using OpenCalligraphy.Core.GameData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MHAchievManager
{
    public partial class MainForm : Form
    {
        private LayerListBox _infoLayersBox;
        private LayerListBox _stringLayersBox;
        private Panel _leftCategoryPanel;
        private Panel _rightInspectorPanel;
        private TreeView _categoryTreeView;
        private TreeView _achievementsTreeView;
        private PropertyGrid _achievementPropertyGrid;
        private SplitContainer _rightSplitContainer;
        private ToolStripMenuItem _localeMenu;
        private ToolStripMenuItem _saveChanges;
        private readonly List<(GameLocale.LocaleInfo Info, ToolStripMenuItem MenuItem)> _localeMenuItems = [];
        private string _folderPath;

        public MainForm()
        {
            AchievementRepository.Initialize();
            InitializeComponent();
            SetApplicationIcon();
            EnableDoubleBuffering(_categoryTreeView);
            EnableDoubleBuffering(_achievementsTreeView);
        }

        private void EnableDoubleBuffering(TreeView tv)
        {
            typeof(TreeView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, tv, new object[] { true });
        }

        private void SetApplicationIcon()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void InitializeComponent()
        {
            Text = "MH Achievement Manager";
            Size = new Size(1200, 930);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            Font = new("Segoe UI", 9f, FontStyle.Regular);

            var menuStrip = new MenuStrip();
            // --- File Menu ---
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Open Achievements...", null, OnOpenAchievementsClicked));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Open PakFile...", null, OnOpenPakFileClicked));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            _saveChanges = new ToolStripMenuItem("Save Changes...", null, OnSaveClicked)
            {
                Enabled = false
            };
            fileMenu.DropDownItems.Add(_saveChanges);

            // --- Locale Menu ---
            _localeMenu = new ToolStripMenuItem("Locale");

            foreach (var locale in GameLocale.Locales)
            {
                var item = new ToolStripMenuItem(locale.DisplayName)
                {
                    Tag = locale,
                    Enabled = false, // Disabled by default until language data is loaded
                    CheckOnClick = false
                };

                // Highlight English by default
                if (locale.Code == GameLocale.DefaultLocale)
                {
                    item.Checked = true;
                }

                item.Click += OnLocaleMenuItemClicked;

                _localeMenuItems.Add((locale, item));
                _localeMenu.DropDownItems.Add(item);
            }

            // --- Edit Menu ---
            var editMenu = new ToolStripMenuItem("Edit");

            var addMenu = new ToolStripMenuItem("Add New", null, OnAddNewAchievementClicked)
            {
                ShortcutKeys = Keys.Control | Keys.N
            };

            var searchMenu = new ToolStripMenuItem("Search...", null, OnSearchAchievementClicked)
            {
                ShortcutKeys = Keys.Control | Keys.F
            };

            editMenu.DropDownItems.Add(addMenu);
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add(searchMenu);

            // --- Help Menu ---
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("About...", null, OnAboutClicked));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(_localeMenu);
            menuStrip.Items.Add(helpMenu);

            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            var mainGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280f));
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 400f));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var layersSidebar = BuildLayersSidebar();

            _leftCategoryPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            _rightInspectorPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            _categoryTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                FullRowSelect = true,
                ShowLines = false,
                ItemHeight = 24,
                DrawMode = TreeViewDrawMode.OwnerDrawText
            };
            _categoryTreeView.DrawNode += (s, e) => LayerTreeView.DrawNode(_categoryTreeView, s, e);
            _categoryTreeView.AfterSelect += OnCategoryTreeNodeSelected;
            _leftCategoryPanel.Controls.Add(_categoryTreeView);

            _achievementsTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                FullRowSelect = true,
                ShowLines = false,
                ItemHeight = 24,
                DrawMode = TreeViewDrawMode.OwnerDrawText
            };
            _achievementsTreeView.DrawNode += (s, e) => LayerTreeView.DrawNode(_achievementsTreeView, s, e);
            InitializeRightInspectorPanel();

            mainGrid.Controls.Add(layersSidebar, 0, 0);
            mainGrid.Controls.Add(_leftCategoryPanel, 1, 0);
            mainGrid.Controls.Add(_rightInspectorPanel, 2, 0);

            Controls.Add(mainGrid);
            mainGrid.BringToFront();
        }

        private void OnAddNewAchievementClicked(object sender, EventArgs e)
        {
            if (_achievementsTreeView.SelectedNode?.Tag is AchievementInfo selectedAchievement)
            {
                int newId = AchievementRepository.Instance.AddNew(selectedAchievement);
                NavigateToAchievement(newId);
            }
        }

        private void OnSearchAchievementClicked(object sender, EventArgs e)
        {
            using var searchForm = new AchievementSearchForm(0);

            if (searchForm.ShowDialog(this) == DialogResult.OK && (int)searchForm.SelectedId != 0)
            {
                NavigateToAchievement((int)searchForm.SelectedId);
            }
        }

        private void OnAboutClicked(object sender, EventArgs e)
        {
            using var aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }

        private void OnOpenPakFileClicked(object sender, EventArgs e)
        {
            OpenPakFile();
            _achievementPropertyGrid.Refresh();
        }

        private void OnLocaleMenuItemClicked(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem clickedItem || clickedItem.Tag is not GameLocale.LocaleInfo locale)
                return;

            // Uncheck all items and check the selected one
            foreach (var (_, menuItem) in _localeMenuItems)
            {
                menuItem.Checked = (menuItem == clickedItem);
            }

            // Apply locale to repository
            ChangeLanguage(locale);
        }

        private void ChangeLanguage(GameLocale.LocaleInfo locale)
        {
            AchievementRepository.Instance.CurrentLanguage = locale.Code;
            RefreshTreesAndRestoreSelection();
        }

        public void NavigateToAchievement(int achievementId)
        {
            var achievement = AchievementRepository.Instance.GetAchievement(achievementId);
            if (achievement == null) return;

            UpdateCategoryList();

            var targetSubCategoryId = achievement.SubCategoryStr;
            TreeNode targetCategoryNode = FindCategoryNodeById(_categoryTreeView.Nodes, targetSubCategoryId);

            if (targetCategoryNode != null)
            {
                _categoryTreeView.SelectedNode = targetCategoryNode;
                targetCategoryNode.EnsureVisible();

                LoadAchievementsForSubCategory(targetSubCategoryId);

                TreeNode targetAchNode = FindAchievementNodeById(_achievementsTreeView.Nodes, achievementId);
                if (targetAchNode != null)
                {
                    _achievementsTreeView.SelectedNode = targetAchNode;
                    targetAchNode.EnsureVisible();
                    _achievementsTreeView.Focus();
                }
            }
        }

        private void RefreshTreesAndRestoreSelection()
        {
            if (_achievementsTreeView.SelectedNode?.Tag is AchievementInfo selectedAchievement)
            {
                NavigateToAchievement((int)selectedAchievement.Id);
                return;
            }

            LocaleStringId? targetSubCategoryId = (_categoryTreeView.SelectedNode?.Tag as CategoryNode)?.Id;

            UpdateCategoryList();

            if (targetSubCategoryId.HasValue)
            {
                TreeNode targetCategoryNode = FindCategoryNodeById(_categoryTreeView.Nodes, targetSubCategoryId.Value);
                if (targetCategoryNode != null)
                {
                    _categoryTreeView.SelectedNode = targetCategoryNode;
                    targetCategoryNode.EnsureVisible();
                    LoadAchievementsForSubCategory(targetSubCategoryId.Value);
                }
            }
        }

        // Helper to find a category node by Tag ID in tree
        private static TreeNode FindCategoryNodeById(TreeNodeCollection nodes, LocaleStringId categoryId)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is CategoryNode cat && cat.Id == categoryId)
                    return node;

                TreeNode child = FindCategoryNodeById(node.Nodes, categoryId);
                if (child != null)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// Recursively searches for an achievement node by ID across all tree levels.
        /// </summary>
        private static TreeNode FindAchievementNodeById(TreeNodeCollection nodes, int achievementId)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is AchievementInfo ach && ach.Id == achievementId)
                {
                    return node;
                }

                // Recursively check child nodes (for grouped/nested achievements)
                if (node.Nodes.Count > 0)
                {
                    TreeNode childMatch = FindAchievementNodeById(node.Nodes, achievementId);
                    if (childMatch != null)
                    {
                        return childMatch;
                    }
                }
            }

            return null;
        }

        private void OpenPakFile()
        {
            using OpenFileDialog dialog = new();
            dialog.FileName = "Calligraphy.sip";
            dialog.Filter = "Pak files (*.sip)|*.sip|All files (*.*)|*.*";
            dialog.Multiselect = false;

            DialogResult dialogResult = dialog.ShowDialog(this);
            if (dialogResult != DialogResult.OK)
                return;

            string filePath = dialog.FileName;

            InitializeGameDatabase(filePath);
        }

        private void InitializeGameDatabase(string filePath)
        {
            if (GameDatabase.Initialize(filePath) == false)
                return;
        }

        private TableLayoutPanel BuildLayersSidebar()
        {
            var layersSidebar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layersSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            layersSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            var infoGroup = new GroupBox
            {
                Text = "InfoMap",
                Dock = DockStyle.Left,
                Width = 165,
            };
            _infoLayersBox = new LayerListBox { Dock = DockStyle.Fill };
            _infoLayersBox.ItemCheckChanging += OnLayersChanging;
            _infoLayersBox.ItemCheckChanged += OnLayerCheckChanged;
            infoGroup.Controls.Add(_infoLayersBox);

            var stringGroup = new GroupBox
            {
                Text = "StringMap",
                Dock = DockStyle.Left,
                Width = 165,
            };
            _stringLayersBox = new LayerListBox { Dock = DockStyle.Fill };
            _stringLayersBox.ItemCheckChanged += OnLayerCheckChanged;
            _stringLayersBox.ItemCheckChanging += OnLayersChanging;
            stringGroup.Controls.Add(_stringLayersBox);

            layersSidebar.Controls.Add(infoGroup, 0, 0);
            layersSidebar.Controls.Add(stringGroup, 0, 1);

            return layersSidebar;
        }

        private void OnLayersChanging(object sender, LayerItemCheckEventArgs e)
        {
            if (!ConfirmDiscardUnsavedChanges())
            {
                e.Cancel = true;
            }
        }

        private static bool ConfirmDiscardUnsavedChanges()
        {
            bool isInfoDirty = AchievementRepository.Instance.IsInfoDirty;
            bool isStringDirty = AchievementRepository.Instance.IsStringDirty;

            // If nothing is modified, allow action immediately
            if (!isInfoDirty && !isStringDirty)
                return true;

            // Build clear detailed warning depending on what was edited
            string details = (isInfoDirty, isStringDirty) switch
            {
                (true, true) => "InfoMap and StringMap",
                (true, false) => "InfoMap",
                (false, true) => "StringMap",
                _ => string.Empty
            };

            string message = $"Unsaved changes in {details} will be lost.\n\n" +
                              "Do you want to proceed?";

            var result = MessageBox.Show(
                message,
                "Unsaved Changes Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            return result == DialogResult.Yes;
        }

        private void InitializeRightInspectorPanel()
        {
            _achievementPropertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                PropertySort = PropertySort.Categorized,
                ToolbarVisible = true,
                HelpVisible = true,
                ViewBackColor = SystemColors.Window,
                ViewForeColor = SystemColors.WindowText,
                DisabledItemForeColor = SystemColors.ControlText,
                HelpBackColor = Theme.ItemSelectedBg,
                HelpForeColor = Theme.TextSelected,
                HelpBorderColor = SystemColors.ActiveBorder,
                SelectedItemWithFocusBackColor = Theme.ItemSelectedBg,
                SelectedItemWithFocusForeColor = Theme.TextSelected,
            };

            _achievementPropertyGrid.PropertyValueChanged += OnAchievementPropertyValueChanged;

            _rightSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BorderStyle = BorderStyle.None,
                SplitterWidth = 4
            };

            _rightInspectorPanel.Controls.Clear();

            _achievementsTreeView.Dock = DockStyle.Fill;
            _rightSplitContainer.Panel1.Controls.Add(_achievementsTreeView);
            _rightSplitContainer.Panel2.Controls.Add(_achievementPropertyGrid);

            _rightInspectorPanel.Controls.Add(_rightSplitContainer);

            _rightSplitContainer.SplitterDistance = (int)(_rightInspectorPanel.Width * 0.4);

            _achievementsTreeView.AfterSelect += OnAchievementsTreeViewSelected;
        }

        /// <summary>
        /// When an achievement node is selected, passes its object to the PropertyGrid.
        /// </summary>
        private void OnAchievementsTreeViewSelected(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is AchievementInfo achInfo)
            {
                _achievementPropertyGrid.SelectedObject = null;
                _achievementPropertyGrid.SelectedObject = achInfo;
                _achievementPropertyGrid.ExpandAllGridItems();
            }
            else
            {
                _achievementPropertyGrid.SelectedObject = null;
            }
        }

        /// <summary>
        /// Triggered when a property value is changed in the PropertyGrid.
        /// Updates the corresponding tree node to reflect the change.
        /// </summary>
        private void OnAchievementPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string propertyName = e.ChangedItem.PropertyDescriptor.Name;

            if (propertyName == nameof(AchievementInfo.Name) && _achievementsTreeView.SelectedNode != null)
            {
                var achInfo = (AchievementInfo)_achievementsTreeView.SelectedNode.Tag;
                string displayName = AchievementRepository.Instance.GetLocale(achInfo.Name);

                _achievementsTreeView.SelectedNode.Text = string.IsNullOrEmpty(displayName)
                    ? $"[{achInfo.Id}]"
                    : $"{displayName} [{achInfo.Id}]";
            }

            if (!Equals(e.OldValue, e.ChangedItem.Value))
            {
                _saveChanges.Enabled = true;
                AchievementRepository.Instance.RebuildIndexes();
                AchievementRepository.Instance.IsInfoDirty = true;

                if (propertyName == nameof(AchievementInfo.CategoryStr) ||
                    propertyName == nameof(AchievementInfo.SubCategoryStr) ||
                    propertyName == nameof(AchievementInfo.ParentId))
                {
                    RefreshTreesAndRestoreSelection();
                    return;
                }                
            }

            _saveChanges.Enabled = AchievementRepository.Instance.IsInfoDirty || AchievementRepository.Instance.IsStringDirty;
            _achievementsTreeView.Invalidate();
        }

        private void OnCategoryTreeNodeSelected(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;

            var selectedId = e.Node.Tag;

            if (e.Node.Parent != null && selectedId is CategoryNode node)
            {
                LoadAchievementsForSubCategory(node.Id);
            }
        }

        private void LoadAchievementsForSubCategory(LocaleStringId id)
        {
            _achievementsTreeView.BeginUpdate();
            _achievementsTreeView.Nodes.Clear();

            var achievementNodes = AchievementRepository.Instance.GetAchievementsForSubCategory(id);

            foreach (var achNodeData in achievementNodes)
            {
                var treeNode = CreateTreeNodeRecursive(achNodeData);
                _achievementsTreeView.Nodes.Add(treeNode);
            }

            _achievementsTreeView.EndUpdate();
        }

        private TreeNode CreateTreeNodeRecursive(AchievementNode achNodeData)
        {
            var info = achNodeData.Info;

            string nodeText = string.IsNullOrEmpty(achNodeData.DisplayName)
                ? $"[{info.Id}]"
                : $"{achNodeData.DisplayName} [{info.Id}]";

            var treeNode = new TreeNode(nodeText)
            {
                Tag = info
            };

            foreach (var childData in achNodeData.Children)
            {
                var childTreeNode = CreateTreeNodeRecursive(childData);
                treeNode.Nodes.Add(childTreeNode);
            }

            return treeNode;
        }

        private void OnLayerCheckChanged(object sender, EventArgs e)
        {
            var activeInfoFiles = _infoLayersBox.Items.Cast<LayerItem>()
                .Where(i => i.IsChecked && !i.IsNew)
                .Select(i => i.FileName);

            var activeStringFiles = _stringLayersBox.Items.Cast<LayerItem>()
                .Where(i => i.IsChecked && !i.IsNew)
                .Select(i => i.FileName);

            AchievementRepository.Instance.ReloadLayers(activeInfoFiles, activeStringFiles);
            OnLocalesLoaded(AchievementRepository.Instance.AvailableLocales);
            RefreshTreesAndRestoreSelection();
            _saveChanges.Enabled = false;
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            string targetInfoPath = _infoLayersBox.ActiveLayer.FileName;
            string targetStringPath = _stringLayersBox.ActiveLayer.FileName;

            var activeInfoFiles = _infoLayersBox.Items.Cast<LayerItem>()
                .Where(i => i.IsChecked && !i.IsNew)
                .Select(i => i.FileName);

            var activeStringFiles = _stringLayersBox.Items.Cast<LayerItem>()
                .Where(i => i.IsChecked && !i.IsNew)
                .Select(i => i.FileName);

            // 1. Generate the report
            var report = AchievementRepository.Instance.GenerateSaveReport(
                targetInfoPath,
                targetStringPath,
                activeInfoFiles,
                activeStringFiles);

            // 2. Open confirmation window
            using var saveDlg = new SavePatchForm(report);
            if (saveDlg.ShowDialog(this) == DialogResult.OK)
            {
                // Update report paths in case user modified them in the textbox
                report.TargetInfoFilePath = saveDlg.FinalInfoPath;
                report.TargetStringFilePath = saveDlg.FinalStringPath;

                // 3. Execute save
                AchievementRepository.Instance.ExecuteSave(report);

                LoadFilesFromFolder(_folderPath);
                _saveChanges.Enabled = false;
            }
        }

        private void OnLocalesLoaded(IReadOnlySet<string> availableLocales)
        {
            foreach (var (info, menuItem) in _localeMenuItems)
            {
                menuItem.Enabled = availableLocales.Contains(info.Code);
            }
        }

        private void UpdateCategoryList()
        {
            _categoryTreeView.BeginUpdate();
            _categoryTreeView.Nodes.Clear();

            var categories = AchievementRepository.Instance.GetCategoriesWithSubcategories();

            foreach (var cat in categories)
            {
                var catNode = new TreeNode(cat.Name)
                {
                    Tag = cat
                };

                foreach (var sub in cat.SubCategories)
                {
                    var subNode = new TreeNode(sub.Name)
                    {
                        Tag = sub
                    };

                    catNode.Nodes.Add(subNode);
                }

                _categoryTreeView.Nodes.Add(catNode);
            }

            _categoryTreeView.EndUpdate();
        }

        private void OnOpenAchievementsClicked(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                FileName = "AchievementInfoMap.json",
                Filter = "Achievement Info Map|AchievementInfoMap.json|All files (*.*)|*.*",
                Multiselect = false,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialog.FileName;
                string folderPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(folderPath))
                {
                    _folderPath = folderPath;
                    LoadFilesFromFolder(folderPath);
                }
            }
        }

        private void LoadFilesFromFolder(string path)
        {
            _infoLayersBox.Items.Clear();
            _stringLayersBox.Items.Clear();

            var searchPaths = new[] { path, Path.Combine(path, "Off") };

            int numInfo = 0;
            int numString = 0;

            foreach (var currentPath in searchPaths)
            {
                if (!Directory.Exists(currentPath)) continue;

                bool isOffFolder = currentPath.EndsWith("Off", StringComparison.OrdinalIgnoreCase);
                var files = Directory.GetFiles(currentPath, "*.json", SearchOption.TopDirectoryOnly);

                foreach (var filePath in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    if (fileName.StartsWith("AchievementInfoMap", StringComparison.OrdinalIgnoreCase))
                    {
                        string cleanName = CleanLayerName(fileName, "AchievementInfoMap");
                        var item = new LayerItem
                        {
                            DisplayName = isOffFolder ? $"[Off] {cleanName}" : cleanName,
                            FileName = filePath,
                            IsChecked = !isOffFolder,
                            IsBase = cleanName == "[Base]"
                        };

                        _infoLayersBox.Items.Add(item);
                        numInfo++;
                    }
                    else if (fileName.StartsWith("AchievementStringMap", StringComparison.OrdinalIgnoreCase))
                    {
                        string cleanName = CleanLayerName(fileName, "AchievementStringMap");
                        var item = new LayerItem
                        {
                            DisplayName = isOffFolder ? $"[Off] {cleanName}" : cleanName,
                            FileName = filePath,
                            IsChecked = !isOffFolder,
                            IsBase = cleanName == "[Base]"
                        };

                        _stringLayersBox.Items.Add(item);
                        numString++;
                    }
                }
            }

            // Append virtual [New] layer placeholder to InfoLayersBox and make it active by default
            var newInfoItem = new LayerItem
            {
                DisplayName = "[New]",
                FileName = Path.Combine(_folderPath, $"AchievementInfoMap_{numInfo:D2}_New.json"),
                IsNew = true
            };
            _infoLayersBox.Items.Add(newInfoItem);
            _infoLayersBox.SetActiveLayer(newInfoItem);

            // Append virtual [New] layer placeholder to StringLayersBox and make it active by default
            var newStringItem = new LayerItem
            {
                DisplayName = "[New]",
                FileName = Path.Combine(_folderPath, $"AchievementStringMap_{numString:D2}_New.json"),
                IsNew = true
            };
            _stringLayersBox.Items.Add(newStringItem);
            _stringLayersBox.SetActiveLayer(newStringItem);

            OnLayerCheckChanged(this, EventArgs.Empty);
        }

        private static string CleanLayerName(string fileName, string prefix)
        {
            string name = fileName[prefix.Length..].TrimStart('_', ' ');
            return string.IsNullOrWhiteSpace(name) ? "[Base]" : name;
        }
    }
}