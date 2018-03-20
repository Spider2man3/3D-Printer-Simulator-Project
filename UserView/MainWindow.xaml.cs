using Hardware;
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

namespace UserView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(HostController controller)
        {
            InitializeComponent();
            var testPage = new TesterPage(this, controller);
            var userPage = new UserPage(this, controller);
            testPage.SetUserPage(userPage);
            userPage.setTesterPage(testPage);
            MainView.Child = userPage;
        }
    }
}
