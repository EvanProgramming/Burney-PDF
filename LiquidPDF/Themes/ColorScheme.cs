using SkiaSharp;

namespace LiquidPDF.Themes
{
    public static class ColorScheme
    {
        public static SKColor Background(bool isDark)
            => isDark ? new SKColor(30, 30, 36) : new SKColor(244, 244, 249);
        
        public static SKColor SidebarBg(bool isDark)
            => isDark ? new SKColor(24, 24, 30) : new SKColor(238, 238, 243);
        
        public static SKColor TextPrimary(bool isDark)
            => isDark ? new SKColor(255, 255, 255, 220) : new SKColor(0, 0, 0, 200);
        
        public static SKColor TextSecondary(bool isDark)
            => isDark ? new SKColor(255, 255, 255, 120) : new SKColor(0, 0, 0, 120);
        
        public static SKColor Border(bool isDark)
            => isDark ? new SKColor(255, 255, 255, 15) : new SKColor(0, 0, 0, 12);
        
        public static SKColor Accent => new SKColor(88, 130, 255);
        
        public static SKColor Shadow(bool isDark)
            => isDark ? new SKColor(0, 0, 0, 60) : new SKColor(0, 0, 0, 25);
    }
}