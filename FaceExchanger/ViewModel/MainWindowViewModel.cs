using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FaceExchanger.Model;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Threading.Tasks;
using System.Threading;
namespace FaceExchanger.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties
        private WriteableBitmap _BasePicture;
        public WriteableBitmap BasePicture
        {
            get { return _BasePicture; }
            set
            {
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



        //        private CancellationTokenSource cameraTask = new CancellationTokenSource();
        private FileManager movieTask = new FileManager();
        private bool IsFirst = true;

        /// <summary>
        /// 画像処理を開始します
        /// </summary>
        /// <param name="paramater"></param>
        private void Start(object paramater)
        {
            if (movieTask.IsAlive)
            {
                movieTask.IsAlive = false;
            }
            // 画像一枚
            if (!IsMovie)
                FileImage();
            // カメラからの動画
            else if (IsCamera && IsMovie)
                CameraMovie();
            // 動画ファイル
            else
                FileMovie();
        }


        /// <summary>
        /// イメージファイルをもとに処理を開始します
        /// </summary>
        private void FileImage()
        {
            IplImage img;
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

                    // 最初に一二回処理させないとちゃんと動かない
                    if (IsFirst)
                    {
                        IsFirst = false;
                        ImageProcessing.FaceChenge(srcImg.Clone(), App.PutImage);
                        ImageProcessing.FaceChenge(srcImg.Clone(), App.PutImage);
                    }
                    ResultPicture = ImageProcessing.FaceChenge(srcImg, App.PutImage);
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(ex, "画像を変換できませんでした。\n何度か試しても変換できない場合はこのソフトがその画像に対応していない可能性があります");
                }
            }
        }


        /// <summary>
        /// 動画ファイルをもとにを処理を開始します(未実装)
        /// </summary>
        private void FileMovie()
        {
        }


        /// <summary>
        /// カメラからのデータをもとに画像処理を開始します
        /// </summary>
        private void CameraMovie()
        {
            try
            {
                movieTask.GetCameraMovie((img) =>
                {
                    try
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                BasePicture = img.ToWriteableBitmap();
                            }
                            catch { }
                        });
                        App.Current.Dispatcher.Invoke(() =>
                        {

                            try
                            {
                                ResultPicture = ImageProcessing.FaceChenge(img, App.PutImage); 
                            }
                            catch { }
                        });
                    }
                    catch { }
                });
            }
            catch (AggregateException e)
            {
                Utils.ShowErrorMessage(e);
            }
        }


        /// <summary>
        /// 顔画像をセットします
        /// </summary>
        private void SetFaceImage(object paramater)
        {
            try
            {
                using (var img = FileManager.OpenImageFile())
                {
                    if (img == null)
                        return;
                    App.PutImage = img;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex, "顔データが読み込めませんでした。");
            }
        }
    }
}