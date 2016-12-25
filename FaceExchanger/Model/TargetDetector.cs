using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Media.Imaging;


namespace FaceExchanger.Model
{
    public class TargetDetector
    {
        public CascadeClassifier Cascade { get; set; }
        public Mat Mask { get; private set; }

        public TargetDetector(string fileName)
        {
            Cascade = new CascadeClassifier(fileName);

            Scale = 1.04;
            ScaleFactor = 1.3;
            MinNeighbors = 2;
        }
        public TargetDetector(string fileName, Mat mask)
        {
            Cascade = new CascadeClassifier(fileName);
            SetMask(mask);

            Scale = 1.04;
            ScaleFactor = 1.3;
            MinNeighbors = 2;
        }

        public void SetMask(Mat mask)
        {
            Mask = mask;
            /*
            var mat4 = new MatOfInt4(mask);
            var indexer = mat4.GetIndexer();

            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    var color = indexer[y, x];

                    // color[3]: alpha channel
                    if ((color[0] == 0 && color[1] == 0 && color[2] == 0) || color[3] == 255)
                    {
                        color[0] = 0;
                        color[1] = 0;
                        color[2] = 0;
                        indexer[y, x] = color;
                    }
                }
            }

            Mask = mat4;*/
        }

        public double Scale { get; set; }
        public double ScaleFactor { get; set; }
        public int MinNeighbors { get; set; }


        /// <summary>
        /// Poisson Image Editing
        /// </summary>
        /// <param name="srcMat">顔がある方</param>
        /// <returns></returns>
        public Mat PutMaskOnFace(Mat srcMat)
        {
            return PutMaskOnFace(srcMat, Mask);
        }

        /// <summary>
        /// Poisson Image Editing
        /// </summary>
        /// <param name="srcMat">顔がある方</param>
        /// <param name="putMat">重ねる顔</param>
        /// <returns></returns>
        public Mat PutMaskOnFace(Mat srcMat, Mat putMat)
        {
            var grayMat = new Mat();
            Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayMat, grayMat);

            var faces = Cascade.DetectMultiScale(grayMat);

            if (faces == null) return srcMat;


            var polygons = new List<List<Point>>();

            var faceCount = faces.Count(); // O(n)

            for (int d = 0; d < faceCount; d++)
            {
                polygons = new List<List<Point>>();

                int x1 = faces[d].X;
                int y1 = faces[d].Y;
                int width = faces[d].Width;
                int heigh = faces[d].Height;
                int x2 = x1 + width;
                int y2 = y1 + heigh;

                polygons.Add(new List<Point>() {
                new Point(x1,y1),
                new Point(x2,y1),
                new Point(x2,y2),
                new Point(x1,y2),
                });

                var pwidth = putMat.Width;
                var pheight = putMat.Height;

                //重ねるファイルは少し拡大したほうが良いかな？
                /*                Mat put0 = putMat[(int)(pwidth * 0.1) ,
                                    (int)(pwidth * 0.9), 
                                    (int)(pheight * 0.1),
                                    (int)(pheight * 0.9)]
                                    .Resize(new Size(width, heigh), 0, 0, InterpolationFlags.Lanczos4);
                */
                Mat put0 = putMat.Resize(new Size(width, heigh), 0, 0, InterpolationFlags.Lanczos4);

                //真ん中編の色を適当に抽出
                // 改良の余地あり（肌色領域の平均取ったり？）
                MatOfByte3 mat3 = new MatOfByte3(put0); // cv::Mat_<cv::Vec3b>
                var indexer = mat3.GetIndexer();
                Vec3b color = indexer[(int)(put0.Width * 0.5), (int)(put0.Height * 0.5)];

                //抽出した色で埋める
                Mat put1 = new Mat(srcMat.Size(), MatType.CV_8UC3, new Scalar(color.Item0, color.Item1, color.Item2));

                //重ねる範囲にコピー
                put1[y1, y2, x1, x2] = put0;

                Mat mask = Mat.Zeros(srcMat.Size(), MatType.CV_8UC3);
                Cv2.FillPoly(mask, polygons, new Scalar(255, 255, 255));

                //中心はここ
                var center = new Point(faces[d].X + faces[d].Width * 0.5, faces[d].Y + faces[d].Height * 0.5);
                Cv2.SeamlessClone(put1, srcMat, mask, center, srcMat, SeamlessCloneMethods.NormalClone);
            }
            return srcMat;
        }

        /// <summary>
        /// 楕円フィッティングして傾き取ろうと思ったけど失敗
        /// </summary>
        /// <param name="srcMat"></param>
        /// <param name="putMat"></param>
        /// <returns></returns>
        [Obsolete]
        public Mat PutEllipseMaskOnFace(Mat srcMat, Mat putMat)
        {
            var grayMat = new Mat();
            Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayMat, grayMat);

            var faces = Cascade.DetectMultiScale(grayMat);

            if (faces == null) return srcMat;

            var binaryMat = new Mat();
            int blockSize = 7;
            double k = 0.15;
            double R = 32;
            Binarizer.Sauvola(grayMat, binaryMat, blockSize, k,R);
            Cv2.BitwiseNot(binaryMat,binaryMat);
            //            return binaryMat;

            var polygons = new List<List<Point>>();

            var faceCount = faces.Count(); // O(n)


            for (int d = 0; d < faceCount; d++)
            {
                polygons = new List<List<Point>>();

                int x1 = faces[d].X;
                int y1 = faces[d].Y;
                int width = faces[d].Width;
                int height = faces[d].Height;
                int x2 = x1 + width;
                int y2 = y1 + height;
                int pwidth = putMat.Width;
                int pheight = putMat.Height;
                int srcWidth = srcMat.Width;
                int srcHeight = srcMat.Height;

                polygons.Add(new List<Point>() {
                new Point(x1,y1),
                new Point(x2,y1),
                new Point(x2,y2),
                new Point(x1,y2),
                });

                // f = fixed
                int fx1 = (int)(x1 - width * 0.01);
                fx1 = fx1 > 0 ? fx1 : 0;

                int fx2 = (int)(x2 + width * 0.01);
                fx2 = fx2 < srcWidth ? fx2 : srcWidth;

                int fy1 = (int)(y1 - height * 0.01);
                fy1 = fy1 > 0 ? fy1 : 0;

                int fy2 = (int)(y2 + height * 0.01);
                fy2 = fy2 < srcHeight ? fy2 : srcHeight;

                int fwidth = x2 - x1;
                int fheight = y2 - y1;

                /*
                                var detectedContours = contours.Where(c =>
                                {
                                    var cc = c.Count();
                                    return cc > 150 && cc < 1000;
                                });

                                foreach(var con in detectedContours)
                                {
                                  var rotateRect =   Cv2.FitEllipse(con);
                                }*/

                var faceSize = new Size(fwidth, fheight);

                //重ねるファイルは少し拡大したほうが良いかな？
                /*                Mat put0 = putMat[(int)(pwidth * 0.1) ,
                                    (int)(pwidth * 0.9), 
                                    (int)(pheight * 0.1),
                                    (int)(pheight * 0.9)]
                                    .Resize(new Size(width, heigh), 0, 0, InterpolationFlags.Lanczos4);
                */
                Mat put0 = putMat.Resize(faceSize, 0, 0, InterpolationFlags.Lanczos4);

                //真ん中編の色を適当に抽出
                // 改良の余地あり（肌色領域の平均取ったり？）
                MatOfByte3 mat3 = new MatOfByte3(put0); // cv::Mat_<cv::Vec3b>
                var indexer = mat3.GetIndexer();
                Vec3b color = indexer[(int)(put0.Width * 0.5), (int)(put0.Height * 0.5)];

                //抽出した色で埋める
                Mat put1 = new Mat(srcMat.Size(), MatType.CV_8UC3, new Scalar(color.Item0, color.Item1, color.Item2));

                //重ねる範囲にコピー
                put1[y1, y2, x1, x2] = put0;

                Mat mask = Mat.Zeros(srcMat.Size(), MatType.CV_8UC3);

                //中心はここ
                var center = new Point(faces[d].X + faces[d].Width * 0.5, faces[d].Y + faces[d].Height * 0.5);



                Mat faceAroundMat = Mat.Zeros(srcMat.Size(), MatType.CV_8UC1);


                faceAroundMat[fy1, fy2, fx1, fx2] = binaryMat[fy1, fy2, fx1, fx2];


                //                faceAroundMat[y1, y2, x1, x2] = binaryMat[y1, y2, x1, x2];
                //var countours = new 
                // 単純な輪郭抽出のみでは、傾きがわからない
                // 元のAPIが破壊的な関数なので clone http://opencv.jp/opencv-2svn/cpp/imgproc_structural_analysis_and_shape_descriptors.html#cv-findcontours
                var contours = faceAroundMat.Clone().FindContoursAsArray(RetrievalModes.List, ContourApproximationModes.ApproxNone);
                

                var detectedContour = contours.FindMax(c => c.Count());

                var rotateRect = new RotatedRect(); 
                if (detectedContour.Count() > 200)
                {
                    rotateRect = Cv2.FitEllipse(detectedContour);
                }
                else
                {
                    rotateRect = new RotatedRect(center, new Size2f(faceSize.Width, faceSize.Height), 0);
                }

                Cv2.Ellipse(mask, rotateRect, new Scalar(255, 255, 255), -1, LineTypes.AntiAlias);
                //                Cv2.FillPoly(mask, polygons, new Scalar(255, 255, 255));

                Cv2.SeamlessClone(put1, srcMat, mask, center, srcMat, SeamlessCloneMethods.NormalClone);
            }


            return srcMat;
        }

    }
}