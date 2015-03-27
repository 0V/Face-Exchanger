using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using System.ComponentModel;
using System.Threading;

namespace FaceExchanger.Model
{

    public class BackgroundVideoCapture : ViewModel.ViewModelBase, IDisposable
    {
        #region CaptureImage変更通知プロパティ
        private WriteableBitmap _CaptureImage;

        public WriteableBitmap CaptureImage
        {
            get
            { return _CaptureImage; }
            set
            {
                if (_CaptureImage == value)
                    return;
                _CaptureImage = value;
                OnPropertyChanged("CaptureImage");
            }
        }
        #endregion

        #region CaptureImageMat変更通知プロパティ
        private Mat _CaptureImageMat;

        public Mat CaptureImageMat
        {
            get
            { return _CaptureImageMat; }
            set
            {
                if (_CaptureImageMat == value)
                    return;
                _CaptureImageMat = value;
                OnPropertyChanged("CaptureImageMat");
            }
        }
        #endregion

        public int CameraNumber { get; private set; }
        public string FileName { get; private set; }

        private BackgroundWorker captureWorker;

        public BackgroundVideoCapture()
        {
            CreateBackgroundWorker();
        }

        private void CreateBackgroundWorker()
        {
            captureWorker = new BackgroundWorker();
            captureWorker.WorkerReportsProgress = true;
            captureWorker.WorkerSupportsCancellation = true;

            // Completed イベントハンドラ―で例外を捕捉し inline 例外にして上に投げる
            captureWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {

                }

                if (e.Error != null)
                {
                    throw new Exception("画像取得中に例外が発生しました。", e.Error);
                }
                else
                {

                }
            };

            captureWorker.ProgressChanged += (sender, e) =>
            {
                try
                {
                    CaptureImageMat = (Mat)e.UserState;
                    CaptureImage = CaptureImageMat.ToWriteableBitmap();
                }
                catch (ArgumentException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            };
        }

        public void Start(int cameraNumber, int interval = 1000)
        {
            if (captureWorker.IsBusy)
                return;

            CameraNumber = cameraNumber;

            captureWorker.DoWork += (sender, e) =>
            {
                var bw = (BackgroundWorker)sender;
                using (var capture = new VideoCapture(CameraNumber))
                {
                    capture.FrameHeight = 640;
                    var image = new Mat();
                    while (true)
                    {
                        if (bw.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        capture.Read(image);
                        if (image == null)
                            throw new Exception("カメラから画像が読み取れませんでした。");

                        bw.ReportProgress(0, image);
                        Thread.Sleep(interval);
                    }
                }
            };
            captureWorker.RunWorkerAsync();
        }


        public void Start(string fileName)
        {
            if (captureWorker.IsBusy)
                throw new InvalidOperationException("すでに Capture スレッドが実行中です。");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            FileName = fileName;

            captureWorker.DoWork += (sender, e) =>
            {
                var bw = (BackgroundWorker)sender;
                using (var capture = new VideoCapture(FileName))
                {
                    int interval = (int)(1000 / capture.Fps);
                    var image = new Mat();
                    while (true)
                    {
                        if (bw.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        capture.Read(image);
                        if (image == null) // 動画終了
                                return;

                        bw.ReportProgress(0, image);
                        Thread.Sleep(interval);
                    }
                    e.Cancel = true;
                }
            };
            captureWorker.RunWorkerAsync();
        }

        public void Start(string fileName, int interval)
        {
            if (captureWorker.IsBusy)
                throw new InvalidOperationException("すでに Capture スレッドが実行中です。"); //{ Data = { { "GetValue.Arguments.fileName", fileName } } };

            FileName = fileName;

            captureWorker.DoWork += (sender, e) =>
            {
                var bw = (BackgroundWorker)sender;
                using (var capture = new VideoCapture(FileName))
                {
                    var image = new Mat();
                    while (true)
                    {
                        if (bw.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        capture.Read(image);
                        if (image == null) // 動画終了
                                return;

                        bw.ReportProgress(0, image);
                        Thread.Sleep(interval);
                    }
                    e.Cancel = true;
                }
            };
            captureWorker.RunWorkerAsync();
        }

        /// <summary>
        /// スレッドを解放しキャプチャを停止
        /// </summary>
        public void Stop()
        {
            captureWorker.CancelAsync();
            captureWorker.Dispose();
            CreateBackgroundWorker();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (captureWorker != null)
                {
                    captureWorker.Dispose();
                }
            }
        }

        ~BackgroundVideoCapture()
        {
            Dispose(false);
        }


    }
}
