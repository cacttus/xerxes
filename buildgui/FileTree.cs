using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Proteus;
using Spartan;

namespace BuildGui
{
    public class FileTree
    {
        #region Private:Methods

        private BuildGuiController _objBuildGuiController = null;
        private System.Windows.Controls.TreeViewItem _objLastTvParent = null;
        private Brush _objLastTviChildBrush = null;
        private int _intTreeViewOutputFileNodeCount;
        private bool _blnOutputWindowUserScroll = false;
        private int _intCurrentError = 0;
        private int _intRecursiveError = 0;
        #endregion

        #region Public:Methods


        public FileTree(BuildGuiController bg)
        {
            _objBuildGuiController = bg;
        }
        public void Reset()
        {
            GetOutputWindowScrollView().MouseDown += OutputWindow_ScrollBar_MouseDown;
            GetTreeView().Items.Clear();
            _blnOutputWindowUserScroll = false;
            _intTreeViewOutputFileNodeCount = 0;
        }
        public void AddBottomScrollSpaceToTreeView()
        {
            System.Windows.Controls.TreeViewItem objTvi;

            if (GetTreeView().Items.Count == 0)
                return;

            objTvi = (System.Windows.Controls.TreeViewItem)GetTreeView().Items[0];

            if (objTvi.ActualHeight > 0)
            {
                int n = (int)(GetTreeView().ActualHeight / objTvi.ActualHeight) - 1; //-1 so we keep at least one item in view.
                for (int i = 0; i < n; ++i)
                {
                    objTvi = new System.Windows.Controls.TreeViewItem();
                    objTvi.Header = "";
                    GetTreeView().Items.Add(objTvi);
                }
            }

        }
        public void ScrollToBottomOfFileTree()
        {
            // ** If the user has scrolled away don't change the view.
            ScrollViewer sv = GetOutputWindowScrollView();
            if ((sv.VerticalOffset != sv.ScrollableHeight)
                && (_blnOutputWindowUserScroll == true))
                return;

            //** If we have updated then scroll to the bottom.
            if (GetTreeView().Items.Count > 0)
            {
                TreeViewItem tvi = null;

                // ** Get the first non empty string item.  The empty string items are just spacing.
                for (int nTvi = GetTreeView().Items.Count - 1; nTvi >= 0; nTvi--)
                {
                    tvi = (TreeViewItem)GetTreeView().Items.GetItemAt(nTvi);
                    string str = (string)tvi.Header;
                    if (!String.IsNullOrEmpty(str))
                        break;
                }

                //
                if (tvi != null)
                {
                    if (tvi.IsExpanded && tvi.Items.Count > 0)
                    {
                        TreeViewItem tviChild = (TreeViewItem)tvi.Items.GetItemAt(tvi.Items.Count - 1);
                        tviChild.BringIntoView();
                    }
                    else
                    {
                        tvi.BringIntoView();
                    }
                }
            }
        }
        public void UpdateTreeViewByLogLine(string line)
        {
            System.Text.RegularExpressions.MatchCollection matches;

            // ** Fucking MS. Yet again prove they have the dumb.  The command output here comes before the filename
            // so we have to strcmp it in order to not show it as a parent node.
            if (line.Contains("Skipping... (no relevant changes detected)"))
                return;

            matches = System.Text.RegularExpressions.Regex.Matches(line, "[a-zA-Z0-9_-~]+\\.(h|hpp|c|cpp|hxx|cxx)$");

            if (matches.Count == 1)
            {
                _objLastTvParent = CreateOutputTreeViewItem(line);
                GetTreeView().Items.Add(_objLastTvParent);
                _intTreeViewOutputFileNodeCount++;
                _objLastTviChildBrush = null;
                return;
            }


            // **We add the manual ##end## to signal the end of a compiled file.
            matches = System.Text.RegularExpressions.Regex.Matches(line, "##end##");

            if (matches.Count > 0)
            {
                _objLastTvParent = null;
                return;
            }

            if (_objLastTvParent != null)
                CreateOutputTreeViewItem(line, _objLastTvParent); // Create new item and add to parent.
            else
                GetTreeView().Items.Add(CreateTreeViewItemNonFile(line)); //Other - non-file.

        }

        public void JumpToNextError(int targetErrorId = -1)
        {
            _intRecursiveError = 0;
            try
            {
                foreach (TreeViewItem tviParent in GetTreeView().Items)
                {
                    // We only go one level deep. 
                    foreach (TreeViewItem tviChild in tviParent.Items)
                    {
                        if (TryJumpToError(tviChild, targetErrorId))
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError("Failed to jump to error.\n" + ex.ToString());
            }
        }

        #endregion

        #region Private:Methods


        private TreeView GetTreeView()
        {
            return _objBuildGuiController.GetGuiWindow()._tvwOutput;
        }
        private void OutputWindow_ScrollBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var myDel = new Action<object>(delegate(object param)
            {
                _blnOutputWindowUserScroll = true;
            });

            // ** Execute in the UI thread so we can access all the ui goodies.
            _objBuildGuiController.GetUxThreadControl().Dispatcher.BeginInvoke(myDel, new Object());

        }
        private ScrollViewer GetOutputWindowScrollView()
        {
            DependencyObject border = VisualTreeHelper.GetChild(GetTreeView(), 0);
            ScrollViewer sv = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            return sv;
        }
        private TreeViewItem CreateOutputTreeViewItem(string text, TreeViewItem objParent = null)
        {
            TreeViewItem objTvi = new System.Windows.Controls.TreeViewItem();
            objTvi.Padding = new Thickness(0, 0, 0, 0);
            //objTvi.BorderThickness = new Thickness(3, 3, 3, 3);
            //Border myBorder = new Border();
            //myBorder.Background = Brushes.SkyBlue;
            //myBorder.BorderBrush = Brushes.Black;
            //myBorder.BorderThickness = new Thickness(2);
            //myBorder.CornerRadius = new CornerRadius(5);
            //myBorder.Child = objTvi;
            // Border brd = objTvi.FindName("Border") as Border;
            // if(brd!=null)

            objTvi.Header = text;
            objTvi.Background = UiBrushes.Brush_OkColor_Output;
            objTvi.IsExpanded = false;
            //objTvi.HorizontalAlignment = HorizontalAlignment.Stretch;
            //Thickness th = objTvi.Margin;
            //objTvi.ItemContainerStyle.

            if (objParent != null)
            {
                objTvi.IsExpanded = true;
                objTvi.MouseDoubleClick += childText_MouseDoubleClick;
                HandleOutputLineErrorState(text, objTvi, objParent);
                objParent.Items.Add(objTvi);
            }

            return objTvi;
        }
        private TreeViewItem CreateTreeViewItemNonFile(string text)
        {
            System.Windows.Controls.TreeViewItem objTvi = new System.Windows.Controls.TreeViewItem();
            objTvi.Header = text;
            objTvi.IsExpanded = true;
            HandleOutputLineErrorState(text, objTvi);

            return objTvi;
        }

        private void HandleOutputLineErrorState(string line, TreeViewItem tviChild, TreeViewItem tviParent = null)
        {
            //Foreground
            tviChild.Foreground = UiBrushes.Brush_Foreground_Output;
            //_objLastTviChildBrush
            if (line.Contains("warning"))
            {
                if (tviParent != null)
                {
                    if (tviParent.Background == UiBrushes.Brush_OkColor_Output && tviParent.Background != UiBrushes.Brush_FailColor_Output)
                        tviParent.Background = UiBrushes.Brush_WarnColor_Output;
                }

                if (_objBuildGuiController.ParsedBuildStatus == ParsedBuildStatus.Ok)
                    _objBuildGuiController.ParsedBuildStatus = ParsedBuildStatus.Warning;

                tviChild.Background = UiBrushes.Brush_WarnColor_Output;

            }
            else if (line.Contains("error"))
            {
                if (tviParent != null)
                {
                    if (tviParent.Background == UiBrushes.Brush_OkColor_Output || tviParent.Background == UiBrushes.Brush_WarnColor_Output)
                        tviParent.Background = UiBrushes.Brush_FailColor_Output;
                }

                tviChild.Background = UiBrushes.Brush_FailColor_Output;

                if (tviParent != null)
                    tviParent.ExpandSubtree();

                if (_objBuildGuiController.ParsedBuildStatus == ParsedBuildStatus.Ok ||
                    _objBuildGuiController.ParsedBuildStatus == ParsedBuildStatus.Warning)
                    _objBuildGuiController.ParsedBuildStatus = ParsedBuildStatus.Error;
            }
            else
            {
                tviChild.Background = _objLastTviChildBrush;
            }

            _objLastTviChildBrush = tviChild.Background;
        }

        private void childText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TreeViewItem ti = (System.Windows.Controls.TreeViewItem)sender;
            TryJumpToError(ti);
        }

        private bool TryJumpToError(TreeViewItem ti, int targetErrorIndex=-1)
        {
            string line = (string)ti.Header;
            string strFile = "";
            int intLineNumber =0;

            //** becuase dumbfuck windows compiler (YEAH, THATS RIGHT DUMB FUCK)
            // lowercases filenames, we have to change the 
            // name of the file, or else when we save it in VS the filename will save in lowercase. annoying
            if (MsvcUtils.ParseClCompilerOutputFileLine(line, ref strFile, ref intLineNumber))
            {
                if (targetErrorIndex != -1)
                {
                    if (_intRecursiveError == targetErrorIndex)
                    {
                        MsvcUtils.VisualStudioOpenFileAtLine(strFile, intLineNumber, _objBuildGuiController.SolutionName);
                        _intCurrentError++;
                    }
                    else
                    {
                        //We didn't match next error.
                        _intRecursiveError++;
                        return false;
                    }
                }
                else
                {
                    MsvcUtils.VisualStudioOpenFileAtLine(strFile, intLineNumber, _objBuildGuiController.SolutionName);

                }
                return true;
            }
            return false;
        }

        #endregion

    }
}
