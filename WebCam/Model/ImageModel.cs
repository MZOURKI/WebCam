using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WebCam.Model
{
    // Classe interne représentant les sources d'image
    internal class ImageModel : INotifyPropertyChanged
    {
        // Source d'image brute (non traitée)
        private BitmapSource _rawImageSource;

        // Source d'image traitée
        private BitmapSource _processedImageSource;

        // Propriété publique pour la source d'image brute
        public BitmapSource RawImageSource
        {
            get { return _rawImageSource; }
            set
            {
                // Vérifier si la nouvelle valeur est différente de l'ancienne
                if (_rawImageSource != value)
                {
                    _rawImageSource = value;

                    // Déclencher l'événement de modification de propriété
                    OnPropertyChanged(nameof(RawImageSource));
                }
            }
        }

        // Propriété publique pour la source d'image traitée
        public BitmapSource ProcessedImageSource
        {
            get { return _processedImageSource; }
            set
            {
                // Vérifier si la nouvelle valeur est différente de l'ancienne
                if (_processedImageSource != value)
                {
                    _processedImageSource = value;

                    // Déclencher l'événement de modification de propriété
                    OnPropertyChanged(nameof(ProcessedImageSource));
                }
            }
        }

        // Événement déclenché lorsqu'une propriété est modifiée
        public event PropertyChangedEventHandler PropertyChanged;

        // Méthode protégée pour déclencher l'événement de modification de propriété
        protected virtual void OnPropertyChanged(string propertyName)
        {
            // Vérifier si des gestionnaires d'événements sont inscrits
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
