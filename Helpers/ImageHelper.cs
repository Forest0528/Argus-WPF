using System;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;

public static class ImageHelper
{
    public static BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
        using (var memory = new MemoryStream())
        {
            // Сохраняем System.Drawing.Bitmap в поток как PNG
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            // Создаем BitmapImage из потока
            var wpfBitmap = new BitmapImage();
            wpfBitmap.BeginInit();
            wpfBitmap.StreamSource = memory;
            wpfBitmap.CacheOption = BitmapCacheOption.OnLoad;
            wpfBitmap.EndInit();

            return wpfBitmap;
        }
    }
}
