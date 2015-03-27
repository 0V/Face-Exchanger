using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FaceExchanger.Model;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Threading.Tasks;
using System.Threading;
using OpenCvSharp.CPlusPlus;

namespace FaceExchanger.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private TargetDetector detector;
        private BackgroundVideoCapture backCapture;

        public MainWindowViewModel()
        {
//            BasePicture = FileManager.GetDefaultFaceImage().ToWriteableBitmap();
            detector = new TargetDetector(App.FaceCascadeName, FileManager.GetDefaultFaceImage());

            backCapture = new BackgroundVideoCapture();
            backCapture.PropertyChanged +=
                (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case "CaptureImage":
                            var b = (BackgroundVideoCapture)sender;
                            BasePicture = b.CaptureImage;
                            ResultPicture = detector.PutMaskOnFace(b.CaptureImageMat.Clone()).ToWriteableBitmap();
                            break;
                        default:
                            break;
                    }
                };

        }

        #region Properties
        private WriteableBitmap _BasePicture;
        public WriteableBitmap BasePicture
        {
            get { return _BasePicture; }
            set
            {
                if (BasePicture == value)
                    return;

                _BasePicture = value;
                OnPropertyChanged("BasePicture");
            }
        }

        private WriteableBitmap _ResultPicture;
        public WriteableBitmap ResultPicture
        {
            get { return _ResultPicture; }
            set
            {
                if (ResultPicture == value)
                    return;

                _ResultPicture = value;
                OnPropertyChanged("ResultPicture");
            }
        }

        private bool _IsCamera = false;
        public bool IsCamera
        {
            get { return _IsCamera; }
            set
            {
                _IsCamera = value;
                OnPropertyChanged("SwitchText");
            }
        }

        public string SwitchText
        {
            get
            {
                if (IsCamera)
                    return "Camera";
                else
                    return "File";
            }
        }

        private bool _IsMovie = false;
        public bool IsMovie
        {
            get { return _IsMovie; }
            set
            {
                _IsMovie = value;
                OnPropertyChanged("ImageText");
            }
        }

        public string ImageText
        {
            get
            {
                if (IsMovie)
                    return "Movie";
                else
                    return "Image";
            }
        }

        private RelayCommand _StartCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return _StartCommand ?? (_StartCommand = new RelayCommand(Start, x => !IsMovie || IsCamera));
                //                return _StartCommand ?? (_StartCommand = new RelayCommand(Start));
            }
            set { _StartCommand = value; }
        }

        private RelayCommand _SaveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return _SaveCommand ?? (_SaveCommand = new RelayCommand(Save, _ => ResultPicture != null));
            }
            set { _SaveCommand = value; }
        }

        private RelayCommand _SetFaceCommand;
        public RelayCommand SetFaceCommand
        {
            get
            {
                return _SetFaceCommand ?? (_SetFaceCommand = new RelayCommand(SetFaceImage));
            }
            set { _SetFaceCommand = value; }
        }

        private RelayCommand _SwitchCommand;
        public RelayCommand SwitchCommand
        {
            get
            {
                return _SwitchCommand ?? (_SwitchCommand = new RelayCommand(x => IsCamera = !IsCamera));
            }
            set { _SwitchCommand = value; }
        }

        private RelayCommand _ImageCommand;
        public RelayCommand ImageCommand
        {
            get
            {
                return _ImageCommand ?? (_ImageCommand = new RelayCommand(x => IsMovie = !IsMovie));
            }
            set { _ImageCommand = value; }
        }
        #endregion

        /// <summary>
        /// 画像処理を開始します
        /// </summary>
        /// <param name="paramater"></param>
        private void Start(object paramater)
        {

            // 画像一枚
            if (!IsMovie)
                StartFileImage();
            // カメラからの動画
            else if (IsCamera && IsMovie)
                StartCameraMovie();
            // 動画ファイル
            else
                StartFileMovie();
        }


        /// <summary>
        /// イメージファイルをもとに処理を開始します
        /// </summary>
        private void StartFileImage()
        {
            Mat img;
            try
            {
                if (IsCamera)
                    img = FileManager.GetCameraImage(0);
                else
                    img = FileManager.OpenImageFile();

                if (img == null)
                    return;
            }
            catch (Exception e)
            {
                Utils.ShowErrorMessage(e, "画像を取得できませんでした");
                return;
            }

            using (var srcImg = img.Clone())
            {
                try
                {
                    BasePicture = img.ToWriteableBitmap();
                    ResultPicture = detector.PutMaskOnFace(srcImg).ToWriteableBitmap();
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex, "画像を変換できませんでした。\n何度か試しても変換できない場合はその画像に対応していない可能性があります");
                }
            }
        }


        /// <summary>
        /// 動画ファイルをもとにを処理を開始します(未実装)
        /// </summary>
        private void StartFileMovie()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// カメラからのデータをもとに画像処理を開始します
        /// </summary>
        private void StartCameraMovie()
        {
            backCapture.Start(0);
        }

        private void Save(object paramater)
        {
            FileManager.SaveImageFile(ResultPicture.ToMat());
        }


        /// <summary>
        /// 顔画像をセットします
        /// </summary>
        private void SetFaceImage(object paramater)
        {
            try
            {
                var img = FileManager.OpenImageFile();
                if (img == null)
                    return;
                detector.SetMask(img);
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex, "顔データが読み込めませんでした。");
            }
        }
    }
}