using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Proteus
{
    public class WpfUtils
    {
        public static bool VerifyNumericText(string text)
        {
            if (text == "")
                return true;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\s*[0-9]+\s*$"); //regex that matches disallowed text
            return regex.IsMatch(text);
        }
        public static string GetComboItemText(System.Windows.Controls.ComboBox cbo, int index = -1)
        {
            object objValue = null;
 
            //if index is -1 then we use the selected value
            try
            {
                System.Windows.Controls.Button btn = (System.Windows.Controls.Button)cbo.SelectedValue;
                objValue = btn.Content;
            }
            catch (Exception)
            {
                System.Windows.Controls.ComboBoxItem itm = (System.Windows.Controls.ComboBoxItem)cbo.SelectedValue;
                objValue = itm.Content;
            }

            if (objValue == null)
                Globals.Logger.LogError("Failed to convert combobox item to button or comboboxitem type.", true);

            string cont = Convert.ToString(objValue);

            return cont;
        }

        public static Dictionary<Control, bool> EnableTabPage(
            string strTabName, 
            TabControl objTabControl,
            bool blnEnabled,
            Dictionary<Control, bool> previouslyEnabledOrDisabledControlStates = null,
            bool blnIgnoreRootParent = false
            )
        {
            TabItem found = FindTabItem(strTabName, objTabControl);
            if (found != null)
                return EnableControlHierarchy(found, blnEnabled, previouslyEnabledOrDisabledControlStates, blnIgnoreRootParent);
            return null;
        }
        public static TabItem FindTabItem(string strTabName, TabControl objTabControl)
        {
            foreach (TabItem ti in objTabControl.Items)
            {
                try
                {
                    string strHeader = (string)ti.Header;
                    if (strHeader.Equals(strTabName))
                    {
                        return ti;
                    }
                }
                catch (Exception ex)
                {
                    Globals.Logger.LogError("Error findni tab\n" + ex.ToString());
                }
            }

            return null;
        }
        public static Dictionary<Control, bool> EnableControlHierarchy(Control control,
                                                           bool blnEnabled,
                                                           Dictionary<Control, bool> previouslyEnabledOrDisabledControlStates = null,
                                                             bool blnIgnoreRootParent = false
                                                           )
        {
            //Returns a dictionary of control states.
            // pass in the dictionary in a subsequent call to re-enable.
            Dictionary<Control, bool> ret = new Dictionary<Control, bool>();
            List<Control> children;

            if (previouslyEnabledOrDisabledControlStates != null)
            {
                //enable / disable a previous set
                foreach (Control key in previouslyEnabledOrDisabledControlStates.Keys)
                {
                    bool value;
                    if (previouslyEnabledOrDisabledControlStates.TryGetValue(key, out value))
                        key.IsEnabled = value;
                }
                //**important we return null - this allows the method to be toggle-able
                return null;
            }

            try
            {
                if (blnIgnoreRootParent == false)
                {
                    ret.Add(control, control.IsEnabled);
                    control.IsEnabled = blnEnabled;
                }
                
                children = WpfUtils.GetChildControls(control);
                foreach (Control child in children)
                {
                    ret.Add(child, child.IsEnabled);
                    child.IsEnabled = blnEnabled;
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.LogError("Error enabling /disabling tab\n" + ex.ToString());
            }
            
            return ret;
        }
        public static List<Control> GetChildControls(Control parent)
        {
            List<Control> logicalCollection = new List<Control>();
            GetLogicalChildCollection(parent, logicalCollection);
            return logicalCollection;
        }
        private static void GetLogicalChildCollection(DependencyObject parent, List<Control> logicalCollection)
        {
            System.Collections.IEnumerable children = System.Windows.LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                
                if (child is DependencyObject)//System.Windows.DependencyObject)
                {
                //    DependencyObject depChild = child as DependencyObject;
                    if(child is Control)
                        logicalCollection.Add((Control)child);

                    GetLogicalChildCollection((DependencyObject)child, logicalCollection);
                }
            }
        }


    }
}
