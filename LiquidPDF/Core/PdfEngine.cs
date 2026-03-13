using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LiquidPDF.Core
{
    /// <summary>
    /// PDF 引擎类，封装 PDF 渲染功能
    /// </summary>
    public class PdfEngine : IDisposable
    {
        // 字段
        private readonly Dictionary<(int page, int width), SKBitmap> _cache;
        private readonly Queue<(int page, int width)> _lruQueue;
        private const int MAX_CACHE_SIZE = 10;
        private readonly object _lock = new();

        /// <summary>
        /// 页面数量
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        /// 是否已加载 PDF 文件
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PdfEngine()
        {
            _cache = new Dictionary<(int page, int width), SKBitmap>();
            _lruQueue = new Queue<(int page, int width)>();
            PageCount = 0;
            IsLoaded = false;
        }

        /// <summary>
        /// 加载 PDF 文件
        /// </summary>
        /// <param name="filePath">PDF 文件路径</param>
        /// <exception cref="FileNotFoundException">文件不存在</exception>
        /// <exception cref="InvalidOperationException">PDF 格式无效</exception>
        public void LoadFile(string filePath)
        {
            lock (_lock)
            {
                // 清空缓存
                ClearCache();

                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("PDF 文件不存在", filePath);
                }

                try
                {
                    // 模拟加载 PDF 文件
                    // 实际应用中这里会使用 Docnet.Core 加载真实的 PDF
                    PageCount = 1; // 模拟只有一页
                    IsLoaded = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("加载 PDF 文件失败", ex);
                }
            }
        }

        /// <summary>
        /// 渲染指定页面
        /// </summary>
        /// <param name="pageIndex">页面索引（从 0 开始）</param>
        /// <param name="targetWidth">目标宽度</param>
        /// <returns>渲染后的 bitmap，失败返回 null</returns>
        public SKBitmap? RenderPage(int pageIndex, int targetWidth)
        {
            lock (_lock)
            {
                // 检查是否已加载 PDF
                if (!IsLoaded)
                {
                    return null;
                }

                // 检查页面索引是否有效
                if (pageIndex < 0 || pageIndex >= PageCount)
                {
                    return null;
                }

                // 检查缓存
                var cacheKey = (pageIndex, targetWidth);
                if (_cache.TryGetValue(cacheKey, out var cachedBitmap))
                {
                    // 更新 LRU 队列
                    UpdateLruQueue(cacheKey);
                    return cachedBitmap;
                }

                try
                {
                    // 模拟渲染页面
                    // 实际应用中这里会使用 Docnet.Core 渲染真实的 PDF 页面
                    int targetHeight = (int)(targetWidth * 1.414f); // A4 比例
                    var info = new SKImageInfo(targetWidth, targetHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
                    var bitmap = new SKBitmap(info);

                    // 填充白色背景
                    using var canvas = new SKCanvas(bitmap);
                    canvas.Clear(SKColors.White);

                    // 绘制一些测试内容
                    using var paint = new SKPaint();
                    paint.Color = SKColors.Black;
                    paint.TextSize = 24;
                    paint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText("PDF Page " + (pageIndex + 1), targetWidth / 2f, targetHeight / 2f, paint);

                    // 存入缓存
                    _cache[cacheKey] = bitmap;
                    _lruQueue.Enqueue(cacheKey);

                    // 检查缓存大小
                    if (_cache.Count > MAX_CACHE_SIZE)
                    {
                        var oldestKey = _lruQueue.Dequeue();
                        if (_cache.TryGetValue(oldestKey, out var oldestBitmap))
                        {
                            oldestBitmap.Dispose();
                            _cache.Remove(oldestKey);
                        }
                    }

                    return bitmap;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取页面宽高比
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <returns>宽高比（height / width），出错返回 1.414（A4 纸比例）</returns>
        public float GetPageAspectRatio(int pageIndex)
        {
            lock (_lock)
            {
                try
                {
                    if (!IsLoaded || pageIndex < 0 || pageIndex >= PageCount)
                    {
                        return 1.414f; // A4 纸比例
                    }

                    // 模拟返回 A4 比例
                    return 1.414f;
                }
                catch (Exception)
                {
                    return 1.414f;
                }
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void ClearCache()
        {
            lock (_lock)
            {
                foreach (var bitmap in _cache.Values)
                {
                    bitmap.Dispose();
                }
                _cache.Clear();
                _lruQueue.Clear();
            }
        }

        /// <summary>
        /// 更新 LRU 队列
        /// </summary>
        private void UpdateLruQueue((int page, int width) key)
        {
            // 移除旧位置
            var tempQueue = new Queue<(int page, int width)>();
            while (_lruQueue.Count > 0)
            {
                var item = _lruQueue.Dequeue();
                if (item != key)
                {
                    tempQueue.Enqueue(item);
                }
            }

            // 重新加入队列
            while (tempQueue.Count > 0)
            {
                _lruQueue.Enqueue(tempQueue.Dequeue());
            }

            _lruQueue.Enqueue(key);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                ClearCache();
                IsLoaded = false;
                PageCount = 0;
            }
            GC.SuppressFinalize(this);
        }
    }
}