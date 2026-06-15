using System;
using System.Windows.Media;
using Translumo.MVVM.Common;
using Translumo.Resources;
using Translumo.Utils;

namespace Translumo.MVVM.ViewModels
{
    public sealed class SimpleDialogViewModel : BindableBase
    {
        public string TextContent
        {
            get => _textContent;
            set => SetProperty(ref _textContent, value);
        }

        public bool CancelAllowed
        {
            get => _cancelAllowed;
            set => SetProperty(ref _cancelAllowed, value);
        }

        public ImageSource IconSource
        {
            get => _iconSource;
            set => SetProperty(ref _iconSource, value);
        }

        public string Caption
        {
            get => _caption;
            set => SetProperty(ref _caption, value);
        }

        private string _textContent;
        private string _caption;
        private bool _cancelAllowed;
        private ImageSource _iconSource;

        public static SimpleDialogViewModel Create(string textContent, SimpleDialogTypes dialogType, string caption = null)
        {
            return new SimpleDialogViewModel()
            {
                TextContent = textContent,
                CancelAllowed = dialogType == SimpleDialogTypes.Question,
                IconSource = GetIconByDialogType(dialogType),
                Caption = caption ?? GetCaptionByDialogType(dialogType)
            };
        }

        private static ImageSource GetIconByDialogType(SimpleDialogTypes dialogType)
        {
            switch (dialogType)
            {
                case SimpleDialogTypes.Error:
                    return IconsResource.ErrorIcon.ToBitmapImage();
                case SimpleDialogTypes.Question:
                    return IconsResource.QuestionIcon.ToBitmapImage();
                case SimpleDialogTypes.Info:
                    return IconsResource.InformationIcon.ToBitmapImage();
                default:
                    throw new NotSupportedException();
            }
        }

        private static string GetCaptionByDialogType(SimpleDialogTypes dialogType)
        {
            switch (dialogType)
            {
                case SimpleDialogTypes.Error:
                    return "Error";
                case SimpleDialogTypes.Question:
                    return "Question";
                case SimpleDialogTypes.Info:
                    return "Information";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
