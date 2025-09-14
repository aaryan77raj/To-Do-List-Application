using System.Windows;
using ToDoAppClient.ViewModels;

namespace ToDoAppClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
