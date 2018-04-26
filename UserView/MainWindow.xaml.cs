using Hardware;
using PrinterSimulator;
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
using System.Windows.Shapes;

namespace Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(HostHandler handler)
        {
            try
            {
                InitializeComponent();
                handler.execute(Command.GetFirmwareVersion, new float[0]);
                var testPage = new TesterPage(this, handler);
                var userPage = new UserPage(this, handler);
                testPage.SetUserPage(userPage);
                userPage.setTesterPage(testPage);
                MainView.Child = userPage;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }
    }
}
