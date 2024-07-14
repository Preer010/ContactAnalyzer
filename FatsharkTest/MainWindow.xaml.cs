using System.Windows;
using FatsharkTest.ViewModel;

namespace FatsharkTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {    

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ContactDataAnalysetViewModel();
        }
    }
}