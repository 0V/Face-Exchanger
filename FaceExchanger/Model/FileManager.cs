using FaceExchanger.Properties;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using OpenCvSharp.CPlusPlus;

namespace FaceExchanger.Model
{
    public static class FileManager
    {
        /// <summary>
        /// リソースファイルから例のシェフの顔画像を取得します
        /// </summary>
        /// <returns></returns>
        public static Mat GetDefaultFaceImage()
        {
            var res = Resources.kawagoe503b01;
            return res.ToMat();
        }

        /// <summary>
        /// カメラから画像を取得します
        /// </summary>
        /// <param name="cameraIndex"></param>
        /// <returns></returns>
        public static Mat GetCameraImage(int cameraIndex = 0)
        {
            var frame = new Mat();
            using (var capture = new VideoCapture(0))
                capture.Read(frame);

            return frame;
        }

        /// <summary>
        /// 指定したイメージファイルを取得します
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Mat OpenImageFile(LoadMode flags = LoadMode.AnyColor)
        {
            using (var openfile = new OpenFileDialog())
            {
                openfile.Filter =
                    "イメージファイル(*.bmp;*.png;*.jpg;*.jpeg)|*.bmp;*.png;*.jpg;*.jpeg|すべてのファイル(*.*)|*.*";

                if (openfile.ShowDialog() != DialogResult.OK)
                    return null;

                return Cv2.ImRead(openfile.FileName, flags);
            }
        }

        public static void SaveImageFile(Mat saveMat)
        {
            using (var sfd = new SaveFileDialog()
            {
                FileName = "新しいファイル.html",
                Filter = "JPEG ファイル(*.jpg;*.png)|*.jpg;*.png|すべてのファイル(*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true,
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (!saveMat.SaveImage(sfd.FileName))
                    {
                        Utils.ShowErrorMessage("ファイルを保存できませんでした。");
                    }
                }
            }
        }
    }
}