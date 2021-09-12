using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        /// <summary>
        /// Оригинальное загруженное изображение
        /// </summary>
        private BitmapImage _originalImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog {Filter= "Image Files|*.bmp;*.jpeg;*.png", Title="Load image"};
            if ((bool)fileDialog.ShowDialog())
            {
                UserImage.Source = new BitmapImage(new Uri(fileDialog.FileName));
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
            UserImage.RenderTransform = transformGroup;
        }
    }
}
