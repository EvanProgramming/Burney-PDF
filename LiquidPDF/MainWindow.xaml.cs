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
using SkiaSharp;
using SkiaSharp.Views.WPF;

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

        // 窗口双击事件 - 最大化/还原
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 检查点击位置是否在标题栏区域
            if (e.GetPosition(this).Y <= 52)
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

        // SKElement 绘制事件
        private void MainCanvas_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            // 获取画布和尺寸
            SKCanvas canvas = e.Surface.Canvas;
            SKImageInfo info = e.Info;

            // 清空画布为深色背景
            canvas.Clear(new SKColor(30, 30, 36)); // #1E1E24

            // 绘制中心文本
            string text = "拖放 PDF 文件到此处";
            using (SKPaint paint = new SKPaint())
            {
                // 设置文本属性
                paint.Color = new SKColor(255, 255, 255, 128); // 半透明白色
                paint.TextSize = 16;
                paint.IsAntialias = true;
                paint.TextAlign = SKTextAlign.Center;

                // 计算文本位置
                SKRect textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);
                float x = info.Width / 2f;
                float y = info.Height / 2f + textBounds.Height / 2f;

                // 绘制文本
                canvas.DrawText(text, x, y, paint);
            }
        }

        // 窗口大小改变事件
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 触发重绘
            MainCanvas.InvalidateVisual();
        }
    }
}