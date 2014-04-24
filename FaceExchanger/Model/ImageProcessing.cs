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
    public class ImageProcessing
    {
        private static IplImage FaceDe(IplImage srcImg, IplImage putImg)
        {
            const double Scale = 1.04;
            //            const double ScaleFactor = 1.139;
            const double ScaleFactor = 1.3;
            const int MinNeighbors = 2;
            using (var smallImg = new IplImage(new CvSize(Cv.Round(srcImg.Width / Scale), Cv.Round(srcImg.Height / Scale)), BitDepth.U8, 1))
            {
                using (var gray = new IplImage(srcImg.Size, BitDepth.U8, 1))
                {
                    Cv.CvtColor(srcImg, gray, ColorConversion.BgrToGray);
                    Cv.Resize(gray, smallImg, Interpolation.Linear);
                    Cv.EqualizeHist(smallImg, smallImg);
                }

                using (var cascade = FileManager.GetFaceCascade())
                using (var storage = new CvMemStorage())
                {
                    storage.Clear();

                    var faces = Cv.HaarDetectObjects(smallImg, cascade, storage, ScaleFactor, MinNeighbors, 0, new CvSize(50, 50));

                    for (int d = 0; d < faces.Total; d++)
                    {
                        var r = faces[d].Value.Rect;
                        r.Y -= 10;
                        var size = new CvSize(r.Width + 30, r.Height + 30);
                        using (var img_laugh_resized = new IplImage(size, putImg.Depth, putImg.NChannels))
                        {
                            Cv.Resize(putImg, img_laugh_resized, Interpolation.NearestNeighbor);

                            int i_max = (((r.X + img_laugh_resized.Width) > srcImg.Width) ? srcImg.Width - r.X : img_laugh_resized.Width);
                            int j_max = (((r.Y + img_laugh_resized.Height) > srcImg.Height) ? srcImg.Height - r.Y : img_laugh_resized.Height);

                            for (int j = 0; j < img_laugh_resized.Width; ++j)
                            {
                                for (int i = 0; i < img_laugh_resized.Height; ++i)
                                {
                                    var color = img_laugh_resized[i, j];
                                    if (img_laugh_resized[i, j].Val1 != 0) srcImg[r.Y + i, r.X + j] = color;//img_laugh_resized[i, j];
                                }
                            }
                        }
                    }
                    return srcImg;
                }
            }
        }

        private static IplImage FaceRect(IplImage srcImg)
        {
            using (var cascade = App.FaceCascade)
            using (var storage = Cv.CreateMemStorage(0))
            using (var face = Cv.HaarDetectObjects(srcImg, cascade, storage, 1.139, 2))
            {
                for (int i = 0; i < face.Total; i++)
                {
                    var faceRect = Cv.GetSeqElem<CvRect>(face, i);
                    Cv.Rectangle(srcImg,
                        Cv.Point(faceRect.Value.X, faceRect.Value.Y),
                        //                    Cv.Point(faceRect.Value.X + faceRect.Value.Width, faceRect.Value.Y + faceRect.Value.Height),
                        Cv.Point(faceRect.Value.X + faceRect.Value.Width + 10, faceRect.Value.Y + faceRect.Value.Height + 20),
                        Cv.RGB(255, 0, 0),
                        3, Cv.AA);
                }
            }
            return srcImg;
        }

        public static WriteableBitmap FaceChenge(IplImage baseImg, IplImage putImg)
        {
            return WriteableBitmapConverter.ToWriteableBitmap(FaceDe(baseImg, putImg));
            
            /*try
            {
                return WriteableBitmapConverter.ToWriteableBitmap(FaceDe(baseImg, putImg));
            }
            catch (Exception e)
            {
                Utils.ShowErrorMessage(e);
                return null;
            }*/
        }

    }
}