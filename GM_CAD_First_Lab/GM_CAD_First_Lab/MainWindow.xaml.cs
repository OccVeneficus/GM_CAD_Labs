using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace GM_CAD_First_Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Текущий масштаб
        /// </summary>
        private double _currentScale = 1.0;

        /// <summary>
        /// Флаг поворота по X
        /// </summary>
        private double _horizontalFlipOffset = 1.0;

        /// <summary>
        /// Флаг поворота по Y
        /// </summary>
        private double _verticalFlipOffset = 1.0;

        /// <summary>
        /// Текущий угол поворота
        /// </summary>
        private double _currentAngle = 0;

        /// <summary>
        /// Константа увеличения/уменьшения масштаба
        /// </summary>
        private const double SCALE_OFFSET = 0.1;

        /// <summary>
        /// Константа увеличения/уменьшения поворота изображения
        /// </summary>
        private const double ROTATION_DEGREES_OFFSET = 90.0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog {Filter= "Image Files|*.bmp;*.jpeg;*.jpg;*.png", Title="Load image"};
            if ((bool)fileDialog.ShowDialog())
            {
                var image = new TransformedBitmap();
                var bitmap = new BitmapImage(new Uri(fileDialog.FileName));
                image.BeginInit();
                image.Source = bitmap;
                image.EndInit();
                UserImage.Source = image;
            }
        }

        private void IncreaceSacleButton_Click(object sender, RoutedEventArgs e)
        {
            _currentScale += SCALE_OFFSET;
            UpdateImage();
        }

        private void DecreaseScaleButton_Click(object sender, RoutedEventArgs e)
        {
            _currentScale -= SCALE_OFFSET;
            UpdateImage();
        }

        private void HorizontalMirrorButton_Click(object sender, RoutedEventArgs e)
        {
            UserImage.RenderTransformOrigin = new Point(0.5, 0.5);
            _horizontalFlipOffset *= -1;
            UpdateImage();
        }

        private void VerticalMirrorButton_Click(object sender, RoutedEventArgs e)
        {
            UserImage.RenderTransformOrigin = new Point(0.5, 0.5);
            _verticalFlipOffset *= -1;
            UpdateImage();
        }

        private void RotateRightButton_Click(object sender, RoutedEventArgs e)
        {
            UserImage.RenderTransformOrigin = new Point(0.5, 0.5);
            _currentAngle += ROTATION_DEGREES_OFFSET * _horizontalFlipOffset * _verticalFlipOffset;
            UpdateImage();
            if (_currentAngle == 360)
            {
                _currentAngle = 0;
            }
        }

        private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            UserImage.RenderTransformOrigin = new Point(0.5, 0.5);
            _currentAngle -= ROTATION_DEGREES_OFFSET* _horizontalFlipOffset * _verticalFlipOffset;
            UpdateImage();
            if (_currentAngle == -360)
            {
                _currentAngle = 0;
            }
        }

        /// <summary>
        /// Обновить изображение с учетом пользовательских трансформаций
        /// </summary>
        private void UpdateImage()
        {
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform(_currentAngle));
            transformGroup.Children.Add(new ScaleTransform { ScaleX = _horizontalFlipOffset * _currentScale, ScaleY = _verticalFlipOffset * _currentScale });
            UserImage.LayoutTransform = transformGroup;
        }

        /// <summary>
        /// Обновляет изображение для сохранения
        /// </summary>
        /// <returns>Измененный битмап для сохранения</returns>
        private TransformedBitmap UpdateImageOnSave()
        {
            TransformedBitmap transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = UserImage.Source as BitmapSource;
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform(_currentAngle));
            transformGroup.Children.Add(new ScaleTransform { ScaleX = _horizontalFlipOffset * _currentScale, ScaleY = _verticalFlipOffset * _currentScale });
            transformedBitmap.Transform = transformGroup;
            transformedBitmap.EndInit();
            return transformedBitmap;
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog { Title = "Save Image" , DefaultExt=".jpg", Filter = "Image Files|*.bmp;*.jpg;*.png;*.jpeg" };
            if ((bool)fileDialog.ShowDialog())
            {
                BitmapEncoder encoder;
                var extenson = Path.GetExtension(fileDialog.FileName);
                switch (extenson)
                {
                    case ".png":
                        {
                            encoder = new PngBitmapEncoder();
                            break;
                        }
                    case ".bmp":
                        {
                            encoder = new BmpBitmapEncoder();
                            break;
                        }
                    case ".jpg":
                        {
                            encoder = new JpegBitmapEncoder();
                            break;
                        }
                    case ".jpeg":
                        {
                            encoder = new JpegBitmapEncoder();
                            break;
                        }
                    default:
                        {
                            MessageBox.Show("Wrong file extension, please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                }
                encoder.Frames.Add(BitmapFrame.Create(UpdateImageOnSave()));
                using (var stream = fileDialog.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }

        private void InvertColorMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UserImage.Source = Invert(UserImage.Source as BitmapSource);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Image isn't loaded",
                    "Color inversion error.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SharpnessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = BitmapImage2Bitmap(UserImage.Source as BitmapSource);
            var bluredImage = Contrast(bitmap);
            UserImage.Source = bluredImage;//Sharp(UserImage.Source as BitmapSource, bluredImage);
        }

        private void ContrastMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = BitmapImage2Bitmap(UserImage.Source as BitmapSource);
            UserImage.Source = Bitmap2BitmapImage( Extensions.Contrast(bitmap,100));
        }

        /// <summary>
        /// Инверсия цветов картинки
        /// </summary>
        /// <param name="source">Картинка</param>
        /// <returns>Картинка с инвертированными цветами</returns>
        public BitmapSource Sharp(BitmapSource source, BitmapSource bluredImage)
        {
            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
            byte[] newPixels = new byte[length];
            byte[] oldPixels = new byte[length];

            source.CopyPixels(oldPixels, stride, 0);

            // Copy source image pixels to the data array
            bluredImage.CopyPixels(newPixels, stride, 0);

            // Change this loop for other formats
            for (int i = 0; i < length; i += 4)
            {
                for(var j = i; j< i+3; j++)
                {
                    var difference = oldPixels[j] - newPixels[j];
                    difference *= 3;
                    if (newPixels[j] + difference < 0)
                    {
                        newPixels[j] = 0;
                    }
                    else if (newPixels[j] + difference > 255)
                    {
                        newPixels[j] = 255;
                    }
                    else
                    {
                        newPixels[j] = (byte)(newPixels[j] + difference);
                    }
                }
            }
            
            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, newPixels, stride);
        }

        public static BitmapSource Invert(BitmapSource source)
        {
            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
          
            byte[] data = new byte[length];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            // Change this loop for other formats
            for (int i = 0; i < length; i += 4)
            {
                data[i] = (byte)(255 - data[i]);         //R
                data[i + 1] = (byte)(255 - data[i + 1]); //G
                data[i + 2] = (byte)(255 - data[i + 2]); //B
                //data[i + 3] = (byte)(255 - data[i + 3]); //A
            }

            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, data, stride);
        }

        public BitmapSource Contrast(Bitmap source)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(source.Width, source.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            // create the negative color matrix
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] { 2.0f, 0, 0, 0, 0},
                new float[] { 0, 2.0f, 0, 0, 0},
                new float[] { 0, 0, 2.0f, 0, 0},
                new float[] { 0, 0, 0, 1.0f, 0},
                new float[] { 0, 0, 0, 0, 1},
            });

            // create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            attributes.SetGamma(1.0f, ColorAdjustType.Bitmap);

            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();

            return Bitmap2BitmapImage(newBitmap);
        }

        private Bitmap BitmapImage2Bitmap(BitmapSource bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval;

            try
            {
                retval = (BitmapSource)Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }


    }
}
