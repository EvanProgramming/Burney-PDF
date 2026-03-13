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
using LiquidPDF.Rendering;

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
        
        // 液态玻璃渲染器
        private readonly LiquidGlassRenderer _glass = new();
        // 背景快照
        private SKImage? _backgroundSnapshot;

        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;
        }

        // 窗口关闭事件
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // 释放资源
            _backgroundSnapshot?.Dispose();
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

            // 1. 清空画布为深色背景
            canvas.Clear(new SKColor(30, 30, 36)); // #1E1E24

            // 2. 在画布中心绘制一个测试矩形（模拟 PDF 页面）
            float pdfWidth = 600;
            float pdfHeight = 800;
            float pdfX = (info.Width - pdfWidth) / 2;
            float pdfY = (info.Height - pdfHeight) / 2;
            var pdfRect = new SKRect(pdfX, pdfY, pdfX + pdfWidth, pdfY + pdfHeight);
            
            // 绘制阴影
            using (var shadowPaint = new SKPaint())
            {
                shadowPaint.Color = new SKColor(0, 0, 0, 60);
                shadowPaint.IsAntialias = true;
                shadowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8);
                canvas.DrawRoundRect(pdfRect, 4, 4, shadowPaint);
            }
            
            // 绘制白色背景
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.IsAntialias = true;
                canvas.DrawRoundRect(pdfRect, 4, 4, paint);
            }

            // 3. 截取当前画布快照
            _backgroundSnapshot?.Dispose();
            _backgroundSnapshot = e.Surface.Snapshot();

            // 4. 绘制顶部液态玻璃工具栏
            var toolbarRect = new SKRoundRect(new SKRect(0, 0, info.Width, 52), 0, 0);
            _glass.DrawGlassPanel(canvas, toolbarRect, _backgroundSnapshot, true);

            // 5. 绘制底部液态玻璃胶囊栏
            float capsuleWidth = 360;
            float capsuleHeight = 44;
            float capsuleX = (info.Width - capsuleWidth) / 2;
            float capsuleY = info.Height - 20 - capsuleHeight;
            var capsuleRect = new SKRect(capsuleX, capsuleY, capsuleX + capsuleWidth, capsuleY + capsuleHeight);
            _glass.DrawCapsule(canvas, capsuleRect, _backgroundSnapshot, true);

            // 6. 在工具栏和胶囊栏上绘制测试文字
            // 工具栏文字
            using (var textPaint = new SKPaint())
            {
                textPaint.Color = SKColors.White;
                textPaint.TextSize = 16;
                textPaint.IsAntialias = true;
                textPaint.TextAlign = SKTextAlign.Center;
                
                float toolbarTextY = 52 / 2f + textPaint.FontSpacing / 2f - textPaint.FontMetrics.Descent;
                canvas.DrawText("Liquid PDF", info.Width / 2f, toolbarTextY, textPaint);
            }
            
            // 胶囊栏文字
            using (var textPaint = new SKPaint())
            {
                textPaint.Color = SKColors.White;
                textPaint.TextSize = 14;
                textPaint.IsAntialias = true;
                textPaint.TextAlign = SKTextAlign.Center;
                
                float capsuleTextY = capsuleY + capsuleHeight / 2f + textPaint.FontSpacing / 2f - textPaint.FontMetrics.Descent;
                canvas.DrawText("第 1 页，共 1 页", info.Width / 2f, capsuleTextY, textPaint);
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