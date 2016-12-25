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
        public CascadeClassifier EyeCascade { get; set; }
        public Mat Mask { get; private set; }

        /*        public TargetDetector(string fileName)
                {
                    Cascade = new CascadeClassifier(fileName);

                    Scale = 1.04;
                    ScaleFactor = 1.3;
                    MinNeighbors = 2;
                }*/
        public TargetDetector(Mat mask)
        {
            Cascade = new CascadeClassifier(App.FaceCascadeName);
            EyeCascade = new CascadeClassifier(App.EyeCascadeName);
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
            return PutEllipseEyeMaskOnFace(srcMat, Mask);
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

            var binaryMat = new Mat();
            int blockSize = 7;
            double k = 0.15;
            double R = 32;
            Binarizer.Sauvola(grayMat, binaryMat, blockSize, k, R);
            Cv2.BitwiseNot(binaryMat, binaryMat);


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
            Binarizer.Sauvola(grayMat, binaryMat, blockSize, k, R);
            Cv2.BitwiseNot(binaryMat, binaryMat);
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
                int fx1 = (int)(x1 - width * 0.1);
                fx1 = fx1 > 0 ? fx1 : 0;

                int fx2 = (int)(x2 + width * 0.1);
                fx2 = fx2 < srcWidth ? fx2 : srcWidth;

                int fy1 = (int)(y1 - height * 0.1);
                fy1 = fy1 > 0 ? fy1 : 0;

                int fy2 = (int)(y2 + height * 0.1);
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
                //var contours = faceAroundMat.Clone().FindContoursAsArray(RetrievalModes.List, ContourApproximationModes.ApproxNone);
                var contours = binaryMat.Clone().FindContoursAsArray(RetrievalModes.List, ContourApproximationModes.ApproxNone);

                /*
                Mat mat = Mat.Zeros(srcMat.Size(), MatType.CV_8UC3);
                
                Cv2.DrawContours(mat, contours.Where(c => Cv2.ContourArea(c) > 100),-1, new Scalar(0, 255, 0));
                */

                var detectedContour = contours.FindMax(c => Cv2.ContourArea(c));
                //                var rotateRect = new RotatedRect();
                /*
                if (Cv2.ContourArea(detectedContour) > Cv2.ContourArea(polygons[0]) * 0.3)
                {
                    rotateRect = Cv2.FitEllipse(detectedContour);
                }
                else
                {
                    rotateRect = new RotatedRect(center, new Size2f(faceSize.Width, faceSize.Height), 0);
                }
                rotateRect = Cv2.FitEllipse(detectedContour);

                Debug.WriteLine(rotateRect.Angle);*/


                var rotateRect = Cv2.FitEllipse(detectedContour);
                rotateRect.Size = new Size2f(faceSize.Width, faceSize.Height);

                float angle = Math.Abs(rotateRect.Angle) > 20 ? -rotateRect.Angle % 20 : -rotateRect.Angle;
                float scale = 1.0f;
                // 回転
                Mat matrix = Cv2.GetRotationMatrix2D(center, angle, scale);

                //画像を回転させる
                Cv2.WarpAffine(put1, put1, matrix, put1.Size());

                Cv2.Ellipse(mask, rotateRect, new Scalar(255, 255, 255), -1, LineTypes.AntiAlias);
                //                Cv2.FillPoly(mask, polygons, new Scalar(255, 255, 255));

                Cv2.SeamlessClone(put1, srcMat, mask, center, srcMat, SeamlessCloneMethods.NormalClone);
            }


            return srcMat;
        }


        /// <summary>
        /// 楕円フィッティングして傾き取ろうと思ったけど
        /// </summary>
        /// <param name="srcMat"></param>
        /// <param name="putMat"></param>
        /// <returns></returns>
        [Obsolete]
        public Mat PutEllipseMaskOnFace2(Mat srcMat, Mat putMat)
        {
            var grayMat = new Mat();
            Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayMat, grayMat);

            var faces = Cascade.DetectMultiScale(grayMat);

            if (faces == null) return srcMat;


            var binaryMat = new Mat();
            //            binaryMat = ColorExtractor.ExtractMask(srcMat,ColorConversionCodes.BGR2HSV,ColorVariation.Skin);
            //            return binaryMat;

            int blockSize = 7;
            double k = 1.5;
            double R = 100;
            Binarizer.Sauvola(grayMat, binaryMat, blockSize, k, R);

            Cv2.BitwiseNot(binaryMat, binaryMat);
            return binaryMat;

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
                /*                int fx1 = (int)(x1 - width * 0.01);
                                fx1 = fx1 > 0 ? fx1 : 0;

                                int fx2 = (int)(x2 + width * 0.01);
                                fx2 = fx2 < srcWidth ? fx2 : srcWidth;

                                int fy1 = (int)(y1 - height * 0.01);
                                fy1 = fy1 > 0 ? fy1 : 0;

                                int fy2 = (int)(y2 + height * 0.01);
                                fy2 = fy2 < srcHeight ? fy2 : srcHeight;
                                */


                int fx1 = (int)(x1 + width * 0.1);

                int fx2 = (int)(x2 - width * 0.1);

                int fy1 = (int)(y1 + height * 0.1);

                int fy2 = (int)(y2 - height * 0.1);

                int fwidth = x2 - x1;
                int fheight = y2 - y1;

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


                //要素数が大きい輪郭だけ
                var detectedContours = contours.Where(c =>
                /*Cv2.ContourArea(c) > Cv2.ContourArea(polygons[0]) * 0.05 &&*/ Cv2.ContourArea(c) < Cv2.ContourArea(polygons[0]) * 0.1);


                Mat conMat = Mat.Zeros(srcMat.Size(), MatType.CV_8UC1);
                Cv2.DrawContours(conMat, detectedContours, -1, new Scalar(255, 255, 255));

                return conMat;

                var points = new List<Point>();
                foreach (var dc in detectedContours)
                {
                    points.Union(dc);
                }
                var detectedRotateRect = Cv2.MinAreaRect(points);

                float angle =
                detectedRotateRect.Angle =
                    Math.Abs(detectedRotateRect.Angle) > 20 ?
                    detectedRotateRect.Angle % 20 :
                    detectedRotateRect.Angle;
                float scale = 1.0f;
                // 回転
                Mat matrix = Cv2.GetRotationMatrix2D(center, angle, scale);

                Debug.WriteLine(detectedRotateRect.Angle);
                //画像を回転させる
                Cv2.WarpAffine(put1, put1, matrix, put1.Size());
                var rotateRect = new RotatedRect(center, new Size2f(faceSize.Width, faceSize.Height), detectedRotateRect.Angle);

                continue;


                Cv2.Ellipse(mask, detectedRotateRect, new Scalar(255, 255, 255), -1, LineTypes.AntiAlias);
                //                Cv2.FillPoly(mask, polygons, new Scalar(255, 255, 255));

                Cv2.SeamlessClone(put1, srcMat, mask, center, srcMat, SeamlessCloneMethods.NormalClone);
            }


            return srcMat;
        }





        /// <summary>
        /// Eye見て傾き検出
        /// </summary>
        /// <param name="srcMat"></param>
        /// <param name="putMat"></param>
        /// <returns></returns>
        public Mat PutEllipseEyeMaskOnFace(Mat srcMat, Mat putMat)
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

                var faceSize = new Size(width, height);

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

                Mat put1gray = Mat.Zeros(srcMat.Size(), MatType.CV_8UC1);
                put1gray[y1, y2, x1, x2] = grayMat[y1, y2, x1, x2];
                var eyes = EyeCascade.DetectMultiScale(put1gray);
                /*
                                Debug.WriteLine(eyes.Count());

                                var cccc = new Point(eyes[0].X + eyes[0].Width * 0.5, eyes[0].Y + eyes[0].Height * 0.5);
                                put1gray.Circle(cccc,(int)(eyes[0].Width * 0.5), new Scalar(0, 255, 255));
                                return put1gray;*/
                var eyeCount = eyes.Count();
                if (eyeCount >= 2)
                {
                    var eyePpints = new List<Point>();

                    var orderedEyes = eyes.OrderByDescending(x => x.Width * x.Height).ToArray();
                    

                    while (true)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            eyePpints.Add(new Point(eyes[i].X + eyes[i].Width * 0.5, eyes[i].Y + eyes[i].Height * 0.5));
                        }
                        var wrapRect = Cv2.MinAreaRect(eyePpints);
                        if (Math.Abs(wrapRect.Angle % 180) < 20)
                        {
                            var scale = 1.0;
                            var angle = -wrapRect.Angle % 180;

                            var eyedx = (eyePpints[0].X + eyePpints[1].X) * 0.5 - wrapRect.Center.X;
                            var eyedy = (eyePpints[0].Y + eyePpints[1].Y) * 0.5 - wrapRect.Center.Y;

                            //中心はここ
                            var center = new Point(
                                (faces[d].X + faces[d].Width * 0.5) + eyedx,
                                (faces[d].Y + faces[d].Height * 0.5) + eyedy);

                            Mat matrix = Cv2.GetRotationMatrix2D(center, angle, scale);

                            //画像を回転させる
                            Cv2.WarpAffine(put1, put1, matrix, put1.Size());
                            var faceAvgWidth = (int)((wrapRect.Size.Width + faceSize.Width) * 0.6);
                            var rotateRect = new RotatedRect(center, new Size2f(faceAvgWidth, faceSize.Height * 0.9), angle);
                            Mat mask = Mat.Zeros(srcMat.Size(), MatType.CV_8UC3);
                            Cv2.Ellipse(mask, rotateRect, new Scalar(255, 255, 255), -1, LineTypes.AntiAlias);
                            //                Cv2.FillPoly(mask, polygons, new Scalar(255, 255, 255));

                            Cv2.SeamlessClone(put1, srcMat, mask, center, srcMat, SeamlessCloneMethods.NormalClone);
                            break;
                        }
                        else
                        {
                            if (orderedEyes.Count() > 2)
                            {
                                orderedEyes = orderedEyes.Skip(1).ToArray();
                            }
                            else
                            {
                                var angle = 0;

                                //中心はここ
                                var center = new Point(faces[d].X + faces[d].Width * 0.5, faces[d].Y + faces[d].Height * 0.5);
                                var rotateRect = new RotatedRect(center, new Size2f(faceSize.Width * 0.8, faceSize.Height * 0.9), angle);
                                Mat mask = Mat.Zeros(srcMat.Size(), MatType.CV_8UC3);
                                Cv2.Ellipse(mask, rotateRect, new Scalar(255, 255, 255), -1, LineTypes.AntiAlias);
                                //                Cv2.FillPoly(mask, polygons, new Scalar(255, 255, 255));

                                Cv2.SeamlessClone(put1, srcMat, mask, center, srcMat, SeamlessCloneMethods.NormalClone);

                                break;
                            }
                        }
                    }
}
                else
                {
                    var angle = 0;
                    //中心はここ
                    var center = new Point(faces[d].X + faces[d].Width * 0.5, faces[d].Y + faces[d].Height * 0.5);
                    var rotateRect = new RotatedRect(center, new Size2f(faceSize.Width * 0.8, faceSize.Height * 0.9), angle);
                    Mat mask = Mat.Zeros(srcMat.Size(), MatType.CV_8UC3);
                    Cv2.Ellipse(mask, rotateRect, new Scalar(255, 255, 255), -1, LineTypes.AntiAlias);
                    //                Cv2.FillPoly(mask, polygons, new Scalar(255, 255, 255));
                    Cv2.SeamlessClone(put1, srcMat, mask, center, srcMat, SeamlessCloneMethods.NormalClone);
                }

            }


            return srcMat;
        }


    }
}