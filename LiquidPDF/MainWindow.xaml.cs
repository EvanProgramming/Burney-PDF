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

namespace LiquidPDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isMaximized = false;
        private double previousLeft;
        private double previousTop;
        private double previousWidth;
        private double previousHeight;

        public MainWindow()
        {
            InitializeComponent();
        }

        // 标题栏鼠标按下事件 - 移动窗口
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isMaximized)
                {
                    // 从最大化状态还原
                    isMaximized = false;
                    WindowState = WindowState.Normal;
                    
                    // 还原到之前的尺寸和位置
                    Left = previousLeft;
                    Top = previousTop;
                    Width = previousWidth;
                    Height = previousHeight;
                }
                
                DragMove();
            }
        }

        // 标题栏双击事件 - 最大化/还原
        private void TitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (isMaximized)
            {
                // 还原窗口
                isMaximized = false;
                WindowState = WindowState.Normal;
                
                // 还原到之前的尺寸和位置
                Left = previousLeft;
                Top = previousTop;
                Width = previousWidth;
                Height = previousHeight;
            }
            else
            {
                // 保存当前尺寸和位置
                previousLeft = Left;
                previousTop = Top;
                previousWidth = Width;
                previousHeight = Height;
                
                // 最大化窗口
                isMaximized = true;
                WindowState = WindowState.Maximized;
            }
        }

        // 窗口按钮鼠标进入事件
        private void WindowButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A32"));
            }
        }

        // 窗口按钮鼠标离开事件
        private void WindowButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.Transparent;
            }
        }

        // 关闭按钮鼠标进入事件
        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.Red;
            }
        }

        // 最小化按钮点击事件
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // 最大化按钮点击事件
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMaximized)
            {
                // 还原窗口
                isMaximized = false;
                WindowState = WindowState.Normal;
                
                // 还原到之前的尺寸和位置
                Left = previousLeft;
                Top = previousTop;
                Width = previousWidth;
                Height = previousHeight;
            }
            else
            {
                // 保存当前尺寸和位置
                previousLeft = Left;
                previousTop = Top;
                previousWidth = Width;
                previousHeight = Height;
                
                // 最大化窗口
                isMaximized = true;
                WindowState = WindowState.Maximized;
            }
        }

        // 关闭按钮点击事件
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}