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

namespace TrexTestCmd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Globals.InitializeGlobals("TrexTest.log");
        }

        private void _btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = false;
                string result = TrexUtils.SendTrexCommand(_txtRemoteMachineName.Text, _txtCommand.Text, ref success);
                _txtResponse.Text = result;
            }
            catch(Exception ex)
            {
                BuildUtils.ShowErrorMessage("Failed to exec command\n" + ex.ToString());
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
