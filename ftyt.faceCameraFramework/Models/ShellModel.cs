using ftyt.faceCameraFramework.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ftyt.faceCameraFramework.Models.FaceParameterModel;

namespace ftyt.faceCameraFramework.Models
{
    internal class ShellModel : BindableBase
    {
        Mode _mode;
        Direction _direction;
        string _datetime;
        string _message;
        string _status;
        byte[] _preview;
        string _previewButtonName;
        string _liveFaceButtonName;
        List<DisplayFaceQualityLevelItem> _faceQualityLevelItem;

        public Mode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        public Direction Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        public string DateTime
        {
            get => _datetime;
            set => SetProperty(ref _datetime, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public byte[] Preview
        {
            get => _preview;
            set => SetProperty(ref _preview, value);
        }
        public string PreviewButtonName
        {
            get => _previewButtonName;
            set => SetProperty(ref _previewButtonName, value);
        }
        public string LiveFaceButtonName
        {
            get => _liveFaceButtonName;
            set => SetProperty(ref _liveFaceButtonName, value);
        }
        public List<DisplayFaceQualityLevelItem> FaceQualityLevelItem
        {
            get => _faceQualityLevelItem;
            set => SetProperty(ref _faceQualityLevelItem, value);
        }
    }
}
