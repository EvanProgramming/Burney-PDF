using SkiaSharp;

namespace LiquidPDF.Rendering
{
    /// <summary>
    /// 液态玻璃效果渲染器
    /// 参考 macOS 26 的液态玻璃视觉效果
    /// </summary>
    public class LiquidGlassRenderer
    {
        // 可调参数（字段）
        private float _blurRadius = 40f;              // 模糊半径
        private float _chromaticAberration = 2.5f;    // 色散强度（px）
        private float _innerOpacity = 0.12f;          // 内部填充透明度
        private float _borderHighlight = 0.6f;        // 边缘高光强度

        /// <summary>
        /// 绘制玻璃面板
        /// </summary>
        /// <param name="canvas">Skia 画布</param>
        /// <param name="bounds">玻璃区域（圆角矩形）</param>
        /// <param name="backgroundSnapshot">背景快照</param>
        /// <param name="isDarkMode">是否为深色模式</param>
        public void DrawGlassPanel(
            SKCanvas canvas,
            SKRoundRect bounds,           // 玻璃区域（圆角矩形）
            SKImage? backgroundSnapshot,  // 背景快照
            bool isDarkMode = true
        )
        {
            // 保存当前绘图状态
            canvas.Save();

            try
            {
                // 1. 第一层：背景模糊
                if (backgroundSnapshot != null)
                {
                    // 创建模糊滤镜
                    using (var blurFilter = SKImageFilter.CreateBlur(_blurRadius, _blurRadius))
                    using (var paint = new SKPaint())
                    {
                        paint.ImageFilter = blurFilter;
                        paint.IsAntialias = true;

                        // 裁剪到玻璃区域
                        canvas.ClipRoundRect(bounds);

                        // 绘制模糊后的背景
                        canvas.DrawImage(backgroundSnapshot, 0, 0, paint);
                    }
                }

                // 2. 第二层：色散效果
                if (backgroundSnapshot != null && _chromaticAberration > 0)
                {
                    // 分离 RGB 通道并偏移
                    DrawChromaticAberration(canvas, bounds, backgroundSnapshot);
                }

                // 3. 第三层：半透明白色填充
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    // 根据模式设置填充颜色
                    float opacity = isDarkMode ? _innerOpacity : _innerOpacity + 0.15f;
                    paint.Color = new SKColor(255, 255, 255, (byte)(255 * opacity));
                    paint.Style = SKPaintStyle.Fill;

                    canvas.DrawRoundRect(bounds, paint);
                }

                // 4. 第四层：边缘高光
                DrawBorderHighlight(canvas, bounds, isDarkMode);

                // 5. 第五层：内部渐变（从上到下）
                DrawInnerGradient(canvas, bounds, isDarkMode);

                // 6. 第六层：1px 边框
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    // 根据模式设置边框颜色
                    paint.Color = isDarkMode 
                        ? new SKColor(255, 255, 255, 30) 
                        : new SKColor(0, 0, 0, 20);
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = 0.5f;

                    canvas.DrawRoundRect(bounds, paint);
                }
            }
            finally
            {
                // 恢复绘图状态
                canvas.Restore();
            }
        }

        /// <summary>
        /// 绘制胶囊形玻璃（用于底部栏）
        /// </summary>
        /// <param name="canvas">Skia 画布</param>
        /// <param name="rect">胶囊区域</param>
        /// <param name="backgroundSnapshot">背景快照</param>
        /// <param name="isDarkMode">是否为深色模式</param>
        public void DrawCapsule(
            SKCanvas canvas,
            SKRect rect,
            SKImage? backgroundSnapshot,
            bool isDarkMode = true
        )
        {
            // 计算胶囊的圆角半径 = rect.Height / 2
            float radius = rect.Height / 2;

            // 创建胶囊形的 RoundRect
            var bounds = new SKRoundRect(rect, radius, radius);

            // 调用 DrawGlassPanel
            DrawGlassPanel(canvas, bounds, backgroundSnapshot, isDarkMode);
        }

        /// <summary>
        /// 绘制色散效果
        /// </summary>
        private void DrawChromaticAberration(SKCanvas canvas, SKRoundRect bounds, SKImage backgroundSnapshot)
        {
            // 保存当前状态
            canvas.Save();

            try
            {
                // 裁剪到玻璃区域
                canvas.ClipRoundRect(bounds);

                // 红色通道 - 左上偏移
                using (var redMatrix = SKColorFilter.CreateColorMatrix(new float[]
                {
                    1, 0, 0, 0, 0,  // R
                    0, 0, 0, 0, 0,  // G
                    0, 0, 0, 0, 0,  // B
                    0, 0, 0, 1, 0   // A
                }))
                using (var paint = new SKPaint())
                {
                    paint.ColorFilter = redMatrix;
                    paint.BlendMode = SKBlendMode.Plus;
                    paint.IsAntialias = true;

                    canvas.DrawImage(backgroundSnapshot, -_chromaticAberration, -_chromaticAberration, paint);
                }

                // 绿色通道 - 不偏移
                using (var greenMatrix = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0, 0, 0, 0, 0,  // R
                    0, 1, 0, 0, 0,  // G
                    0, 0, 0, 0, 0,  // B
                    0, 0, 0, 1, 0   // A
                }))
                using (var paint = new SKPaint())
                {
                    paint.ColorFilter = greenMatrix;
                    paint.BlendMode = SKBlendMode.Plus;
                    paint.IsAntialias = true;

                    canvas.DrawImage(backgroundSnapshot, 0, 0, paint);
                }

                // 蓝色通道 - 右下偏移
                using (var blueMatrix = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0, 0, 0, 0, 0,  // R
                    0, 0, 0, 0, 0,  // G
                    0, 0, 1, 0, 0,  // B
                    0, 0, 0, 1, 0   // A
                }))
                using (var paint = new SKPaint())
                {
                    paint.ColorFilter = blueMatrix;
                    paint.BlendMode = SKBlendMode.Plus;
                    paint.IsAntialias = true;

                    canvas.DrawImage(backgroundSnapshot, _chromaticAberration, _chromaticAberration, paint);
                }
            }
            finally
            {
                canvas.Restore();
            }
        }

        /// <summary>
        /// 绘制边缘高光
        /// </summary>
        private void DrawBorderHighlight(SKCanvas canvas, SKRoundRect bounds, bool isDarkMode)
        {
            // 顶部边缘高光
            var highlightRect = new SKRect(
                bounds.Rect.Left,
                bounds.Rect.Top,
                bounds.Rect.Right,
                bounds.Rect.Top + 2
            );

            // 创建渐变
            using (var gradient = SKShader.CreateLinearGradient(
                new SKPoint(highlightRect.Left, highlightRect.Top),
                new SKPoint(highlightRect.Right, highlightRect.Top),
                new SKColor[] { 
                    new SKColor(255, 255, 255, 0),
                    new SKColor(255, 255, 255, (byte)(255 * _borderHighlight)),
                    new SKColor(255, 255, 255, 0)
                },
                new float[] { 0, 0.5f, 1 },
                SKShaderTileMode.Clamp
            ))
            using (var maskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3))
            using (var paint = new SKPaint())
            {
                paint.Shader = gradient;
                paint.MaskFilter = maskFilter;
                paint.IsAntialias = true;

                canvas.DrawRect(highlightRect, paint);
            }
        }

        /// <summary>
        /// 绘制内部渐变
        /// </summary>
        private void DrawInnerGradient(SKCanvas canvas, SKRoundRect bounds, bool isDarkMode)
        {
            // 创建垂直渐变
            using (var gradient = SKShader.CreateLinearGradient(
                new SKPoint(bounds.Rect.MidX, bounds.Rect.Top),
                new SKPoint(bounds.Rect.MidX, bounds.Rect.Bottom),
                new SKColor[] { 
                    isDarkMode 
                        ? new SKColor(200, 210, 255, 18)  // 深色模式顶部
                        : new SKColor(255, 255, 255, 40), // 浅色模式顶部
                    isDarkMode 
                        ? new SKColor(150, 160, 200, 5)   // 深色模式底部
                        : new SKColor(220, 225, 240, 10)  // 浅色模式底部
                },
                new float[] { 0, 1 },
                SKShaderTileMode.Clamp
            ))
            using (var paint = new SKPaint())
            {
                paint.Shader = gradient;
                paint.IsAntialias = true;

                canvas.DrawRoundRect(bounds, paint);
            }
        }
    }
}