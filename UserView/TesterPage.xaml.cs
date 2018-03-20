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

namespace UserView
{
    /// <summary>
    /// Interaction logic for TesterPage.xaml
    /// </summary>
    public partial class TesterPage : Canvas
    {
        private MainWindow mainWindow;
        private UserPage userPage;
        private HostController controller;
        public TesterPage(MainWindow mainWindow, HostController controller)
        {
            InitializeComponent();
            this.controller = controller;
            this.mainWindow = mainWindow;
        }
        public void SetUserPage(UserPage page)
        {
            this.userPage = page;
        }

        private void Switch_To_User(object sender, RoutedEventArgs e)
        {
            (this.Parent as Viewbox).Child = userPage;
            //(this.Parent as MainWindow).Content = userPage;
        }
    }
}
