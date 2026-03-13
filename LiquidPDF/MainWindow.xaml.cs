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
using LiquidPDF.Core;

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
        // PDF 引擎
        private readonly PdfEngine _pdf = new();
        // 当前页码
        private int _currentPage = 0;
        // 缩放比例
        private float _zoom = 1.0f;
        // 当前文件名
        private string? _currentFileName;
        // 侧边栏状态
        private bool _sidebarVisible = true;
        private const float SIDEBAR_WIDTH = 200f;
        // 深色模式
        private bool _isDarkMode = true;

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
            _pdf.Dispose();
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

            // 1. 清空画布，根据模式设置背景颜色
            if (_isDarkMode)
            {
                canvas.Clear(new SKColor(30, 30, 36)); // #1E1E24
            }
            else
            {
                canvas.Clear(new SKColor(244, 244, 249)); // #F4F4F9
            }

            // 2. 绘制侧边栏背景
            if (_sidebarVisible)
            {
                // 侧边栏背景矩形
                using (var sidebarPaint = new SKPaint())
                {
                    sidebarPaint.Color = _isDarkMode ? new SKColor(24, 24, 30) : new SKColor(238, 238, 239); // 深色模式 #18181E，浅色模式 #EEEEEF
                    sidebarPaint.IsAntialias = true;
                    var sidebarRect = new SKRect(0, 52, SIDEBAR_WIDTH, info.Height);
                    canvas.DrawRect(sidebarRect, sidebarPaint);
                }

                // 右边框
                using (var borderPaint = new SKPaint())
                {
                    borderPaint.Color = _isDarkMode ? new SKColor(255, 255, 255, 30) : new SKColor(0, 0, 0, 20);
                    borderPaint.IsAntialias = true;
                    borderPaint.StrokeWidth = 1;
                    var startPoint = new SKPoint(SIDEBAR_WIDTH, 52);
                    var endPoint = new SKPoint(SIDEBAR_WIDTH, info.Height);
                    canvas.DrawLine(startPoint, endPoint, borderPaint);
                }
            }

            // 2. 检查是否已加载 PDF
            if (!_pdf.IsLoaded)
            {
                // 绘制空状态
                using (var textPaint = new SKPaint())
                {
                    textPaint.Color = _isDarkMode ? new SKColor(255, 255, 255, 128) : new SKColor(0, 0, 0, 128); // 半透明文字
                    textPaint.TextSize = 16;
                    textPaint.IsAntialias = true;
                    textPaint.TextAlign = SKTextAlign.Center;

                    // 计算文本位置
                    SKRect textBounds = new SKRect();
                    string text = "拖放 PDF 文件到此处";
                    textPaint.MeasureText(text, ref textBounds);
                    float x = info.Width / 2f;
                    float y = info.Height / 2f + textBounds.Height / 2f;

                    // 绘制文本
                    canvas.DrawText(text, x, y, textPaint);
                }
            }
            else
            {
                // 定义常量
                const float PAGE_MARGIN_TOP = 20;
                const float PAGE_CORNER_RADIUS = 4;

                // 1. 计算内容区域
                float contentLeft = _sidebarVisible ? SIDEBAR_WIDTH : 0;
                var contentRect = new SKRect(
                    contentLeft,
                    52, // TOOLBAR_HEIGHT
                    info.Width,
                    info.Height - 52 - 44 - 20 // TOOLBAR_HEIGHT - CAPSULE_HEIGHT - CAPSULE_MARGIN
                );

                // 2. 计算页面尺寸
                float pageWidth = contentRect.Width * 0.7f * _zoom;
                float aspectRatio = _pdf.GetPageAspectRatio(_currentPage);
                float pageHeight = pageWidth * aspectRatio;

                // 3. 计算页面位置（居中，顶部留 20px 边距）
                float pdfX = (contentRect.Width - pageWidth) / 2f + contentRect.Left;
                float pdfY = PAGE_MARGIN_TOP + contentRect.Top;
                var pdfRect = new SKRect(pdfX, pdfY, pdfX + pageWidth, pdfY + pageHeight);
                var roundRect = new SKRoundRect(pdfRect, PAGE_CORNER_RADIUS, PAGE_CORNER_RADIUS);

                // 4. 渲染 PDF 页面
                int renderWidth = (int)(pageWidth * _zoom);
                var bitmap = _pdf.RenderPage(_currentPage, renderWidth);
                if (bitmap != null)
                {
                    // 5. 绘制 macOS 风格的多层柔和阴影
                    // 第一层：偏移 (0, 2)，模糊 8，透明度 15
                    using (var shadowPaint = new SKPaint())
                    {
                        shadowPaint.Color = new SKColor(0, 0, 0, 15);
                        shadowPaint.IsAntialias = true;
                        shadowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8);
                        var shadowRect1 = new SKRect(pdfRect.Left, pdfRect.Top + 2, pdfRect.Right, pdfRect.Bottom + 2);
                        canvas.DrawRoundRect(shadowRect1, PAGE_CORNER_RADIUS, PAGE_CORNER_RADIUS, shadowPaint);

                        // 第二层：偏移 (0, 8)，模糊 24，透明度 20
                        shadowPaint.Color = new SKColor(0, 0, 0, 20);
                        shadowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 24);
                        var shadowRect2 = new SKRect(pdfRect.Left, pdfRect.Top + 8, pdfRect.Right, pdfRect.Bottom + 8);
                        canvas.DrawRoundRect(shadowRect2, PAGE_CORNER_RADIUS, PAGE_CORNER_RADIUS, shadowPaint);

                        // 第三层：偏移 (0, 16)，模糊 48，透明度 12
                        shadowPaint.Color = new SKColor(0, 0, 0, 12);
                        shadowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 48);
                        var shadowRect3 = new SKRect(pdfRect.Left, pdfRect.Top + 16, pdfRect.Right, pdfRect.Bottom + 16);
                        canvas.DrawRoundRect(shadowRect3, PAGE_CORNER_RADIUS, PAGE_CORNER_RADIUS, shadowPaint);
                    }
                    
                    // 6. 绘制页面
                    using (var paint = new SKPaint())
                    {
                        paint.IsAntialias = true;
                        paint.FilterQuality = SKFilterQuality.High; // 高质量缩放

                        // 裁剪为圆角矩形
                        canvas.ClipRoundRect(roundRect);

                        // 绘制 PDF 位图，填充整个页面区域
                        canvas.DrawBitmap(bitmap, pdfRect, paint);
                    }
                }
            }

            // 7. 截取当前画布快照
            _backgroundSnapshot?.Dispose();
            _backgroundSnapshot = e.Surface.Snapshot();

            // 定义常量
            const float TOOLBAR_HEIGHT = 52;
            const float CAPSULE_HEIGHT = 44;
            const float CAPSULE_MARGIN = 20;
            const float TOOLBAR_PADDING = 16;

            // 8. 绘制顶部液态玻璃工具栏
            var toolbarRect = new SKRoundRect(new SKRect(0, 0, info.Width, TOOLBAR_HEIGHT), 0, 0);
            _glass.DrawGlassPanel(canvas, toolbarRect, _backgroundSnapshot, _isDarkMode);

            // 9. 绘制工具栏 UI 元素
            // 创建文字绘制 SKPaint
            using (var textPaint = new SKPaint())
            {
                textPaint.IsAntialias = true;
                textPaint.Color = _isDarkMode ? new SKColor(255, 255, 255, 220) : new SKColor(0, 0, 0, 220); // 深色/浅色模式文字颜色
                textPaint.TextSize = 13;
                textPaint.TextAlign = SKTextAlign.Center;
                // 设置 Segoe UI Semibold 字体
                textPaint.Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

                // 绘制文件名/标题
                float toolbarTextY = TOOLBAR_HEIGHT / 2f + 5;
                string toolbarText = _currentFileName ?? "Liquid PDF";
                canvas.DrawText(toolbarText, info.Width / 2f, toolbarTextY, textPaint);
            }

            // 绘制图标
            using (var iconPaint = new SKPaint())
            {
                iconPaint.IsAntialias = true;
                iconPaint.Color = _isDarkMode ? new SKColor(255, 255, 255, 160) : new SKColor(0, 0, 0, 160); // 深色/浅色模式图标颜色
                iconPaint.TextSize = 16;
                iconPaint.TextAlign = SKTextAlign.Center;
                // 设置 Segoe UI Symbol 字体
                iconPaint.Typeface = SKTypeface.FromFamilyName("Segoe UI Symbol", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

                // 左侧：侧边栏切换按钮图标 "☰"
                float sidebarIconX = TOOLBAR_PADDING;
                float iconY = TOOLBAR_HEIGHT / 2f + 4;
                canvas.DrawText("☰", sidebarIconX, iconY, iconPaint);

                // 右侧：搜索图标 "🔍"
                float searchIconX = info.Width - TOOLBAR_PADDING - 40;
                canvas.DrawText("🔍", searchIconX, iconY, iconPaint);

                // 右侧：更多选项图标 "⋯"
                float moreIconX = info.Width - TOOLBAR_PADDING;
                canvas.DrawText("⋯", moreIconX, iconY, iconPaint);
            }

            // 10. 绘制底部液态玻璃胶囊栏
            float capsuleW = 360;
            float capsuleH = 44;
            float capsuleX = (info.Width - capsuleW) / 2;
            float capsuleY = info.Height - capsuleH - 20;
            var capsuleRect = new SKRect(capsuleX, capsuleY, capsuleX + capsuleW, capsuleY + capsuleH);
            _glass.DrawCapsule(canvas, capsuleRect, _backgroundSnapshot, _isDarkMode);

            // 11. 在胶囊栏上绘制页码和控制元素
            if (_pdf.IsLoaded)
            {
                // 计算垂直居中位置
                float centerY = capsuleY + capsuleH / 2f + 4;

                // 绘制页码文字
                using (var textPaint = new SKPaint())
                {
                    textPaint.IsAntialias = true;
                    textPaint.Color = _isDarkMode ? new SKColor(255, 255, 255, 200) : new SKColor(0, 0, 0, 200); // 半透明文字
                    textPaint.TextSize = 12.5f;
                    textPaint.TextAlign = SKTextAlign.Center;
                    // 设置 Segoe UI Regular 字体
                    textPaint.Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

                    string pageInfo = $"第 {_currentPage + 1} 页，共 {_pdf.PageCount} 页";
                    canvas.DrawText(pageInfo, info.Width / 2f, centerY, textPaint);
                }

                // 绘制箭头
                using (var arrowPaint = new SKPaint())
                {
                    arrowPaint.IsAntialias = true;
                    arrowPaint.TextSize = 16;
                    arrowPaint.TextAlign = SKTextAlign.Center;
                    // 设置 Segoe UI Symbol 字体
                    arrowPaint.Typeface = SKTypeface.FromFamilyName("Segoe UI Symbol", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

                    // 左侧：上一页箭头
                    float leftArrowX = capsuleX + 28;
                    arrowPaint.Color = _isDarkMode 
                        ? (_currentPage > 0 ? new SKColor(255, 255, 255, 120) : new SKColor(255, 255, 255, 60)) 
                        : (_currentPage > 0 ? new SKColor(0, 0, 0, 120) : new SKColor(0, 0, 0, 60)); // 第一页时半透明
                    canvas.DrawText("◀", leftArrowX, centerY, arrowPaint);

                    // 右侧：下一页箭头
                    float rightArrowX = capsuleX + capsuleW - 28;
                    arrowPaint.Color = _isDarkMode 
                        ? (_currentPage < _pdf.PageCount - 1 ? new SKColor(255, 255, 255, 120) : new SKColor(255, 255, 255, 60)) 
                        : (_currentPage < _pdf.PageCount - 1 ? new SKColor(0, 0, 0, 120) : new SKColor(0, 0, 0, 60)); // 最后一页时半透明
                    canvas.DrawText("▶", rightArrowX, centerY, arrowPaint);
                }
            }
            else
            {
                // 未加载 PDF 时，绘制空状态
                using (var textPaint = new SKPaint())
                {
                    textPaint.Color = _isDarkMode ? SKColors.White : SKColors.Black;
                    textPaint.TextSize = 14;
                    textPaint.IsAntialias = true;
                    textPaint.TextAlign = SKTextAlign.Center;
                    
                    float capsuleTextY = capsuleY + capsuleH / 2f + textPaint.FontSpacing / 2f - textPaint.FontMetrics.Descent;
                    canvas.DrawText("", info.Width / 2f, capsuleTextY, textPaint);
                }
            }
        }

        // 窗口大小改变事件
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 触发重绘
            MainCanvas.InvalidateVisual();
        }

        // 键盘按键事件
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // 调整模糊半径
                case Key.D1:
                    _glass.SetBlurRadius(_glass.GetParameters().BlurRadius + 5);
                    MainCanvas.InvalidateVisual();
                    PrintParameters();
                    break;
                case Key.D2:
                    _glass.SetBlurRadius(_glass.GetParameters().BlurRadius - 5);
                    MainCanvas.InvalidateVisual();
                    PrintParameters();
                    break;
                
                // 调整色散强度
                case Key.D3:
                    _glass.SetChromaticAberration(_glass.GetParameters().ChromaticAberration + 0.5f);
                    MainCanvas.InvalidateVisual();
                    PrintParameters();
                    break;
                case Key.D4:
                    _glass.SetChromaticAberration(_glass.GetParameters().ChromaticAberration - 0.5f);
                    MainCanvas.InvalidateVisual();
                    PrintParameters();
                    break;
                
                // 调整透明度
                case Key.D5:
                    _glass.SetInnerOpacity(_glass.GetParameters().InnerOpacity + 0.02f);
                    MainCanvas.InvalidateVisual();
                    PrintParameters();
                    break;
                case Key.D6:
                    _glass.SetInnerOpacity(_glass.GetParameters().InnerOpacity - 0.02f);
                    MainCanvas.InvalidateVisual();
                    PrintParameters();
                    break;
                
                // 打印参数
                case Key.D:
                    PrintParameters();
                    break;
                
                // 缩放快捷键
                case Key.D0:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        // Ctrl + 0：重置缩放
                        _zoom = 1.0f;
                        _pdf.ClearCache();
                        MainCanvas.InvalidateVisual();
                    }
                    break;
                case Key.OemPlus:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        // Ctrl + =：放大
                        _zoom += 0.1f;
                        if (_zoom > 5.0f) _zoom = 5.0f;
                        _pdf.ClearCache();
                        MainCanvas.InvalidateVisual();
                    }
                    break;
                case Key.OemMinus:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        // Ctrl + -：缩小
                        _zoom -= 0.1f;
                        if (_zoom < 0.3f) _zoom = 0.3f;
                        _pdf.ClearCache();
                        MainCanvas.InvalidateVisual();
                    }
                    break;
            }
        }

        // 打印当前参数值
        private void PrintParameters()
        {
            var (blur, aberration, opacity) = _glass.GetParameters();
            Console.WriteLine($"Blur: {blur} | Aberration: {aberration} | Opacity: {opacity:F2}");
        }

        // 拖放进入事件
        private void OnDragOver(object sender, DragEventArgs e)
        {
            // 检查拖入的是否为文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // 获取文件路径
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    // 检查文件扩展名
                    var file = files[0];
                    if (System.IO.Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Effects = DragDropEffects.Copy;
                        return;
                    }
                }
            }
            e.Effects = DragDropEffects.None;
        }

        // 文件拖放事件
        private void OnFileDrop(object sender, DragEventArgs e)
        {
            // 获取文件路径
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                // 过滤出第一个 PDF 文件
                var pdfFile = files.FirstOrDefault(f => System.IO.Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(pdfFile))
                {
                    try
                    {
                        // 加载 PDF 文件
                        _pdf.LoadFile(pdfFile);
                        // 提取文件名
                        _currentFileName = System.IO.Path.GetFileName(pdfFile);
                        // 重置状态
                        _currentPage = 0;
                        _zoom = 1.0f;
                        // 重绘
                        MainCanvas.InvalidateVisual();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载 PDF 文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // 鼠标滚轮事件
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_pdf.IsLoaded) return;

            // 如果按住 Ctrl 键，实现缩放功能
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // 缩放模式
                if (e.Delta > 0)
                {
                    // 放大
                    _zoom += 0.1f;
                    if (_zoom > 5.0f) _zoom = 5.0f;
                }
                else
                {
                    // 缩小
                    _zoom -= 0.1f;
                    if (_zoom < 0.3f) _zoom = 0.3f;
                }

                // 清空 PDF 缓存（因为渲染尺寸变了）
                _pdf.ClearCache();
                // 重绘
                MainCanvas.InvalidateVisual();
            }
            else
            {
                // 翻页模式
                // 向上滚：上一页，向下滚：下一页
                if (e.Delta > 0 && _currentPage > 0)
                {
                    _currentPage--;
                }
                else if (e.Delta < 0 && _currentPage < _pdf.PageCount - 1)
                {
                    _currentPage++;
                }

                // 重绘
                MainCanvas.InvalidateVisual();
            }
        }

        // 鼠标点击事件
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 获取点击位置（考虑 DPI 缩放）
            var point = e.GetPosition(MainCanvas);
            float clickX = (float)point.X;
            float clickY = (float)point.Y;

            // 检查是否点击工具栏左侧的侧边栏按钮（左侧 56px）
            if (clickY <= 52 && clickX <= 56)
            {
                // 切换侧边栏显示/隐藏
                _sidebarVisible = !_sidebarVisible;
                MainCanvas.InvalidateVisual();
                return;
            }

            if (!_pdf.IsLoaded) return;

            // 计算胶囊栏区域
            float capsuleW = 360;
            float capsuleH = 44;
            float capsuleX = (float)((MainCanvas.ActualWidth - capsuleW) / 2);
            float capsuleY = (float)(MainCanvas.ActualHeight - capsuleH - 20);

            // 检查点击是否在胶囊栏内
            if (clickX >= capsuleX && clickX <= capsuleX + capsuleW &&
                clickY >= capsuleY && clickY <= capsuleY + capsuleH)
            {
                // 检查是否点击左箭头区域（左侧 56px）
                if (clickX <= capsuleX + 56 && _currentPage > 0)
                {
                    _currentPage--;
                    MainCanvas.InvalidateVisual();
                }
                // 检查是否点击右箭头区域（右侧 56px）
                else if (clickX >= capsuleX + capsuleW - 56 && _currentPage < _pdf.PageCount - 1)
                {
                    _currentPage++;
                    MainCanvas.InvalidateVisual();
                }
            }
        }

        // 键盘按键事件
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S:
                    // 按 S 键切换侧边栏显示/隐藏
                    _sidebarVisible = !_sidebarVisible;
                    MainCanvas.InvalidateVisual();
                    break;
                case Key.F2:
                    // 按 F2 键切换深色/浅色模式
                    _isDarkMode = !_isDarkMode;
                    MainCanvas.InvalidateVisual();
                    break;
            }
        }
    }
}