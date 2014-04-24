using FaceExchanger.Properties;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
namespace FaceExchanger.Model
{
    public class FileManager
    {
        /// <summary>
        /// リソースファイルから例のシェフの顔画像を取得します
        /// </summary>
        /// <returns></returns>
        public static IplImage GetFaceImage()
        {
            var res = Resources.kawagoe503b01;
            return BitmapConverter.ToIplImage(res);
        }


        /// <summary>
        /// カメラから画像を取得します
        /// </summary>
        /// <param name="cameraIndex"></param>
        /// <returns></returns>
        public static IplImage GetCameraImage(int cameraIndex = 0)
        {
            return Cv.CreateCameraCapture(cameraIndex).QueryFrame();
        }


        /// <summary>
        /// 顔認識用カスケード型分類器の取得します
        /// </summary>
        /// <returns></returns>
        public static CvHaarClassifierCascade GetFaceCascade()
        {
            //            return CvHaarClassifierCascade.FromFile("haarcascade_frontalface_default.xml");
            return CvHaarClassifierCascade.FromFile("haarcascade_frontalface_alt.xml");
        }


        /// <summary>
        /// アニメ顔認識用カスケード型分類器の取得します
        /// </summary>
        /// <returns></returns>
        public static CvHaarClassifierCascade GetAnimeFaceCascade()
        {
            return CvHaarClassifierCascade.FromFile("lbpcascade_animeface.xml");
        }


        /// <summary>
        /// 指定したイメージファイルを取得します
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IplImage OpenImageFile(LoadMode flags = LoadMode.AnyColor)
        {
            var openfile = new OpenFileDialog();
            openfile.Filter =
                "イメージファイル(*.bmp;*.png;*.jpg;*.jpeg)|*.bmp;*.png;*.jpg;*.jpeg|すべてのファイル(*.*)|*.*";
            if (openfile.ShowDialog() != DialogResult.OK)
                return null;

            return new IplImage(openfile.FileName, flags);
            
            /*if (openfile.ShowDialog() != DialogResult.OK)
                return null;
            try
            {
                return new IplImage(openfile.FileName, flags);
            }
            catch
            {
                return null;
            }*/
        }


        private bool _IsAlive = false;
        public bool IsAlive
        {
            get { return _IsAlive; }
            set { _IsAlive = value; }
        }

        /// <summary>
        /// カメラから映像を取得します
        /// </summary>
        /// <param name="write"></param>
        /// <param name="interval">option</param>
        /// <param name="cameraIndex">option</param>
        /// <returns></returns>
        public Task GetCameraMovie(Action<IplImage> write,int interval = 0,int cameraIndex = 0)
        {
            IsAlive = true;
            return Task.Factory.StartNew(() =>
            {
                using (var capture = Cv.CreateCameraCapture(cameraIndex))
                {
                    var wb = new WriteableBitmap(capture.FrameWidth, capture.FrameHeight, 96, 96, PixelFormats.Bgr24, null);
                    while (IsAlive)
                    {
                        write(capture.QueryFrame());
                        System.Threading.Thread.Sleep(interval); 
                    }
                }
            });
        }
    }
}