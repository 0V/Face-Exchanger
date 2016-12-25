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


        public Mat PutMaskOnFace(Mat srcMat)
        {
            return PutMaskOnFace(srcMat, Mask);
        }

        public Mat PutMaskOnFace__(Mat srcMat, Mat putMat)
        {
            var grayMat = new Mat();
            Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayMat, grayMat);

            var faces = Cascade.DetectMultiScale(grayMat);

            if (faces == null) return srcMat;

            var srcMat3 = new MatOfByte3(srcMat);
            var indexerSrc = srcMat3.GetIndexer();

            for (int i = 0; i < srcMat3.Height; i++)
            {
                for (int j = 0; j < srcMat3.Width; j++)
                {
                    var a = indexerSrc[i, j];
                }

            }
            var resizedMat = new Mat();

            for (int d = 0; d < faces.Count(); d++)
            {
                int faceX = faces[d].X;
                int faceY = faces[d].Y;
                var size = new Size(faces[d].Width, faces[d].Height);

                Cv2.Resize(putMat.Clone(), resizedMat, size);

                var mat3 = new MatOfByte3(resizedMat);
                var indexer = mat3.GetIndexer();

                int xMax = (((faceX + resizedMat.Width) > srcMat.Width) ? (faceX + resizedMat.Width) - srcMat.Width : resizedMat.Width);
                int yMax = (((faceY + resizedMat.Height) > srcMat.Height) ? (faceY + resizedMat.Height) - srcMat.Height : resizedMat.Height);

                for (int y = 0; y < xMax; ++y)
                {
                    for (int x = 0; x < xMax; ++x)
                    {
                        var color = indexer[y, x];
                        if (color[0] != 0)
                        {
                            int xx = faceX + x;
                            int yy = faceY + y;
                            indexerSrc[yy, xx] = color;
                        }
                    }
                }
            }
            return srcMat;
        }


        public Mat PutMaskOnFace/*Poisson*/(Mat srcMat, Mat putMat)
        {
            var grayMat = new Mat();
            Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayMat, grayMat);

            var faces = Cascade.DetectMultiScale(grayMat);

            if (faces == null) return srcMat;



            var polygons = new List<List<Point>>();

            for (int d = 0; d < faces.Count(); d++)
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

    }
}