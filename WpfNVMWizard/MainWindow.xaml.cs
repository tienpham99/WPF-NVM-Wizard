using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfNVMWizard.Helper;

namespace WpfNVMWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NvmHelper.IsNvmInstalled())
            {
                MessageBox.Show("NVM is not installed or not found.");
                return;
            }

            // Load NVM info
            txtNvmVersion.Text = NvmHelper.GetNvmVersion();
            txtNvmPath.Text = NvmHelper.GetNvmInstallPath();

            LoadVersions();
        }

        private void LoadVersions()
        {
            LoadInstalledVersions();
            LoadAvailableVersions();
        }

        private void LoadInstalledVersions()
        {
            lstInstalledVersions.Items.Clear();

            var installedVersions = NvmHelper.GetInstalledVersions();

            foreach (var (version, isCurrent) in installedVersions)
            {
                lstInstalledVersions.Items.Add(CreateVersionItem(version, isCurrent, null, "", false));
            }
        }

        private void LoadAvailableVersions()
        {
            lstAvailableVersions.Items.Clear();

            var available = NvmHelper.GetAvailableVersions();

            foreach (var (version, date, lts) in available.Take(30)) // chỉ lấy 10 bản đầu
            {
                lstAvailableVersions.Items.Add(CreateVersionItem(version, false, lts, date, true));
            }
        }

        private UIElement CreateVersionItem(string version, bool isCurrent, string lts, string date, bool isAvailable = false)
        {
            var panel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Margin = new Thickness(5), 
                VerticalAlignment = VerticalAlignment.Center 
            };

            var versionText = new TextBlock
            {
                Text = version + (isCurrent ? " (current)" : ""),
                FontWeight = isCurrent ? FontWeights.Bold : FontWeights.Normal,
                Width = 100,
                VerticalAlignment = VerticalAlignment.Center
            };
            panel.Children.Add(versionText);

            if (!string.IsNullOrEmpty(lts))
            {
                panel.Children.Add(new TextBlock 
                { 
                    Text = $"LTS ({lts})", 
                    Foreground = Brushes.SteelBlue, 
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 80
                });
            }

            panel.Children.Add(new TextBlock 
            { 
                Text = date, 
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Width = 100
            });

            var actionBtn = new Button
            {
                Content = isAvailable ? "Install" : (isCurrent ? "Uninstall" : "Use"),
                Tag = version,
                Margin = new Thickness(20, 0, 0, 0),
                Padding = new Thickness(5),
                Background = isAvailable ? Brushes.Green : (isCurrent ? Brushes.Red : Brushes.Blue),
                Foreground = Brushes.White,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 80
            };

            actionBtn.MouseEnter += (s, e) => actionBtn.Background = Brushes.DarkSlateGray;
            actionBtn.MouseLeave += (s, e) => actionBtn.Background = isAvailable ? Brushes.Green : (isCurrent ? Brushes.Red : Brushes.Blue);

            if (isAvailable)
                actionBtn.Click += Install_Click;
            else if (isCurrent)
                actionBtn.Click += Uninstall_Click;
            else
                actionBtn.Click += Use_Click;

            panel.Children.Add(actionBtn);

            // Thêm đường ngăn cách
            var separator = new Separator
            {
                Margin = new Thickness(0, 5, 0, 5),
                Background = Brushes.LightGray,
                Height = 1
            };

            var container = new StackPanel();
            container.Children.Add(panel);
            container.Children.Add(separator);

            return container;
        }

        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string version)
            {
                btn.IsEnabled = false;
                btn.Content = "Installing...";
                
                // Thêm log để kiểm tra
                Console.WriteLine($"Installing version: {version}");
                
                await Task.Run(() => NvmHelper.InstallVersion(version));
                
                // Thêm log để kiểm tra
                Console.WriteLine("Installation complete, refreshing list...");

                LoadVersions();
                
                // Thêm log để kiểm tra
                Console.WriteLine("List refreshed.");
                
                btn.IsEnabled = true;
                btn.Content = "Install";
            }
        }

        private void Use_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string version)
            {
                NvmHelper.UseVersion(version);

                System.Threading.Thread.Sleep(200);

                LoadVersions();
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string version)
            {
                NvmHelper.UninstallVersion(version);

                System.Threading.Thread.Sleep(200);

                LoadVersions();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            // Đặt lại ô tìm kiếm về trạng thái ban đầu
            txtSearchInstalled.Text = "Search Installed...";
            txtSearchInstalled.Foreground = Brushes.Gray;

            txtSearchAvailable.Text = "Search Available...";
            txtSearchAvailable.Foreground = Brushes.Gray;

            // Tải lại danh sách
            LoadVersions();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Search Installed..." || textBox.Text == "Search Available...")
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = textBox.Name == "txtSearchInstalled" ? "Search Installed..." : "Search Available...";
                    textBox.Foreground = Brushes.Gray;
                }
            }
        }

        private void SearchInstalled_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearchInstalled.Text.ToLower();

            if (!string.IsNullOrEmpty(searchText) && searchText != "search installed...")
            {
                lstInstalledVersions.Items.Clear();

                var installedVersions = NvmHelper.GetInstalledVersions()
                    .Where(v => v.version.ToLower().Contains(searchText));

                foreach (var (version, isCurrent) in installedVersions)
                {
                    lstInstalledVersions.Items.Add(CreateVersionItem(version, isCurrent, null, "", false));
                }
            }
        }

        private void SearchAvailable_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearchAvailable.Text.ToLower();

            if (!string.IsNullOrEmpty(searchText) && searchText != "search available...")
            {
                lstAvailableVersions.Items.Clear();

                var availableVersions = NvmHelper.GetAvailableVersions()
                    .Where(v => v.version.ToLower().Contains(searchText));

                foreach (var (version, date, lts) in availableVersions)
                {
                    lstAvailableVersions.Items.Add(CreateVersionItem(version, false, lts, date, true));
                }
            }
        }
    }
}