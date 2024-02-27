using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using WebCam.Model;
using System.Windows;
using GalaSoft.MvvmLight.Command;

namespace WebCam.ViewModel
{
    internal class MainViewModel : ViewModelBase, IDisposable
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private ImageModel _imageModel;
        private bool _isProcessing;

        public MainViewModel()
        {
            _imageModel = new ImageModel();
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Vérifie s'il y a au moins un périphérique vidéo disponible
            if (_videoDevices.Count > 0)
            {
                // Utilisation du premier périphérique vidéo trouvé
                _videoSource = new VideoCaptureDevice(_videoDevices[0].MonikerString);

                // Gestion de l'événement NewFrame pour la capture d'image
                _videoSource.NewFrame += VideoSource_NewFrame;
            }
            else
            {
                Console.WriteLine("Aucun périphérique vidéo trouvé.");
            }
        }

        public BitmapSource RawImageSource => _imageModel.RawImageSource;
        public BitmapSource ProcessedImageSource => _imageModel.ProcessedImageSource;

        public Visibility RawImageVisibility => !_isProcessing ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProcessedImageVisibility => _isProcessing ? Visibility.Visible : Visibility.Collapsed;

        public ICommand StartAcquisitionCommand => new RelayCommand(StartAcquisition);
        public ICommand ApplyTreatmentCommand => new RelayCommand(ApplyTreatment);

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Convertit le cadre en BitmapImage
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

            // Utilise Dispatcher.Invoke pour mettre à jour l'interface utilisateur depuis le thread approprié
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BitmapImage bitmapImage = ConvertBitmapToBitmapImage(bitmap);

                    // Affiche l'image capturée sans conversion en niveaux de gris
                    _imageModel.RawImageSource = bitmapImage;
                    OnPropertyChanged(nameof(RawImageSource));
                });
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);

                return new Bitmap(stream);
            }
        }

        private void StartAcquisition()
        {
            // La capture vidéo est déjà traitée dans le constructeur 
            _videoSource.Start();
        }

        private void ApplyTreatment()
        {
            // Convertit la source d'image en Bitmap
            Bitmap rawBitmap = BitmapSourceToBitmap(_imageModel.RawImageSource);

            // Applique le traitement ( conversion en niveaux de gris)
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            rawBitmap = grayscaleFilter.Apply(rawBitmap);

            // Convertit le Bitmap résultant en BitmapImage
            _imageModel.ProcessedImageSource = ConvertBitmapToBitmapImage(rawBitmap);
            OnPropertyChanged(nameof(ProcessedImageSource));

            // Désactive l'état de traitement
            _isProcessing = true;
            OnPropertyChanged(nameof(RawImageVisibility));
            OnPropertyChanged(nameof(ProcessedImageVisibility));
        }

        // on arrete la capture vidéo à la fin
        public void Dispose()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
            }
        }
    }
}
