using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace GM_CAD_First_Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Приватные переменные
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

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Приватные методы

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

        /// <summary>
        /// Конвертация BitmapSource в Bitmap
        /// </summary>
        /// <param name="bitmapImage">BitmapSource</param>
        /// <returns>Bitmap созданный из указанного BitmapSource</returns>
        private Bitmap BitmapImage2Bitmap(BitmapSource bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Конвертация BitmapSource в BitmapSource
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns> BitmapSource созданный из указанного Bitmap</returns>
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

        #endregion

        #region Обработчики нажатия кнопок

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
            UserImage.Source = Extensions.Invert(UserImage.Source as BitmapSource);
        }

        private void SharpnessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = BitmapImage2Bitmap(UserImage.Source as BitmapSource);
            UserImage.Source = Bitmap2BitmapImage(Extensions.Sharpen(bitmap));
        }

        private void ContrastMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var contrastDialog = new ContrastWindow();
            var result = contrastDialog.ShowDialog();
            if (!(bool)result)
            {
                return;
            }
            var bitmap = BitmapImage2Bitmap(UserImage.Source as BitmapSource);
            UserImage.Source = Bitmap2BitmapImage( Extensions.Contrast(bitmap,contrastDialog.ContrastValue));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog { Filter = "Image Files|*.bmp;*.jpeg;*.jpg;*.png", Title = "Load image" };
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
            _currentAngle -= ROTATION_DEGREES_OFFSET * _horizontalFlipOffset * _verticalFlipOffset;
            UpdateImage();
            if (_currentAngle == -360)
            {
                _currentAngle = 0;
            }
        }

        #endregion
    }
}
