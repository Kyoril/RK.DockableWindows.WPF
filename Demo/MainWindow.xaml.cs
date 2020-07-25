using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> log = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.logList.ItemsSource = log;
        }

        private void DockWindow_Closed(object sender, RoutedEventArgs e)
        {
            this.log.Add($"DockWindow \"{(sender as Docker.DockWindow)?.Title}\" was closed");
        }

        private void DockWindow_Closing(object sender, CancelEventArgs e)
        {
            this.log.Add($"DockWindow \"{(sender as Docker.DockWindow)?.Title}\" will be closed...");
        }
    }
}
