using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceExchanger.Model
{
    /// <summary>
    /// 抽出する色
    /// </summary>
    public enum ColorVariation
    {
        White,
        //        Black,
        //        DarkRed,
        Green,
        //        Orange,
        Red,
        Skin,
    }

    public struct ColorTable
    {
        public ColorTable(int ch1Lower, int ch1Upper, int ch2Lower, int ch2Upper, int ch3Lower, int ch3Upper)
        {
            this.ch1Lower = ch1Lower;
            this.ch1Upper = ch1Upper;
            this.ch2Lower = ch2Lower;
            this.ch2Upper = ch2Upper;
            this.ch3Lower = ch3Lower;
            this.ch3Upper = ch3Upper;
        }
        public int ch1Lower, ch1Upper, ch2Lower, ch2Upper, ch3Lower, ch3Upper;
    }

    //******************************************************************************
    //
    //
    // 新しい定義済み抽出ルールを追加したいときは ColorVariation に色名を追加し
    // Extract メソッド内で分岐して ColorTable のコンストラクタに引数を指定してやればOK
    // 詳しくは実装を参照
    // 
    //******************************************************************************

    /// <summary>
    /// ユーザー指定あるいは定義済みのルールに基づき色領域を抽出します
    /// </summary>
    public class ColorExtractor
    {

        public static Mat Extract(Mat srcMat, ColorConversionCodes code, ColorVariation color)
        {
            //            int a = Enum.GetNames(typeof(ColorConversion)).Length;
            ColorTable table;
            if (code == ColorConversionCodes.BGR2HSV)
            {
                switch (color)
                {
                    //                    case ColorVariation.Black:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    //                    case ColorVariation.DarkRed:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    case ColorVariation.Green:
                        table = new ColorTable(50, 70, 80, 255, 0, 255);
                        break;
                    //                    case ColorVariation.Orange:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    case ColorVariation.Red:
                        table = new ColorTable(170, 10, 80, 255, 0, 255);
                        break;
                    case ColorVariation.Skin:
                        table = new ColorTable(0, 10, 80, 255, 0, 255);
                        break;
                    //                    case ColorVariation.White:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    default:
                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                        break;
                }

                return Extract(
                    srcMat,
                    code,
                    table.ch1Lower, table.ch1Upper,
                    table.ch2Lower, table.ch2Upper,
                    table.ch3Lower, table.ch3Upper
                    );
            }
            else
            {
                throw new NotImplementedException("HSV 値以外を用いた抽出は実装されていません");
            }



        }


        public static Mat Extract(Mat srcMat, ColorConversionCodes code, ColorTable table)
        {
            return Extract(
                srcMat,
                code,
                table.ch1Lower, table.ch1Upper,
                table.ch2Lower, table.ch2Upper,
                table.ch3Lower, table.ch3Upper
                );
        }

        public static Mat ExtractRegularFormat(Mat srcMat, ColorConversionCodes code,
        int ch1LowerAngle, int ch1UpperAngle,
        int ch2LowerPer, int ch2UpperPer,
        int ch3LowerPer, int ch3UpperPer)
        {
            /*
            Console.WriteLine("{0} : {1} : {2} : {3} : {4} : {5}",
                ch1LowerAngle / 2,
                ch1UpperAngle / 2,
                ch2LowerPer * 255 / 100,
                ch2UpperPer * 255 / 100,
                ch3LowerPer * 255 / 100,
                ch3UpperPer * 255 / 100);*/
            return Extract(
                 srcMat,
                 code,
                 ch1LowerAngle / 2,
                 ch1UpperAngle / 2,
                 ch2LowerPer * 255 / 100,
                 ch2UpperPer * 255 / 100,
                 ch3LowerPer * 255 / 100,
                 ch3UpperPer * 255 / 100
                 );
        }

        public static Mat ExtractRegularFormat(Mat srcMat, ColorConversionCodes code,
        ColorTable table)
        {
            /*
            Console.WriteLine("{0} : {1} : {2} : {3} : {4} : {5}",
                ch1LowerAngle / 2,
                ch1UpperAngle / 2,
                ch2LowerPer * 255 / 100,
                ch2UpperPer * 255 / 100,
                ch3LowerPer * 255 / 100,
                ch3UpperPer * 255 / 100);*/

            return ExtractRegularFormat(
                srcMat,
                code,
                table.ch1Lower, table.ch1Upper,
                table.ch2Lower, table.ch2Upper,
                table.ch3Lower, table.ch3Upper
                );
        }

        public static Mat Extract(Mat srcMat, ColorConversionCodes code,
        int ch1Lower, int ch1Upper,
        int ch2Lower, int ch2Upper,
        int ch3Lower, int ch3Upper)
        {
            var maskMat = ExtractMask(srcMat,
                code,
                ch1Lower, ch1Upper,
                ch2Lower, ch2Upper,
                ch3Lower, ch3Upper
                );
            srcMat.CopyTo(maskMat, maskMat);

            return maskMat;
        }


        public static Mat ExtractMask(Mat srcMat, ColorConversionCodes code, ColorVariation color)
        {
            //            int a = Enum.GetNames(typeof(ColorConversion)).Length;
            ColorTable table;
            if (code == ColorConversionCodes.BGR2HSV)
            {
                switch (color)
                {
                    //                    case ColorVariation.Black:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    //                    case ColorVariation.DarkRed:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    case ColorVariation.Green:
                        table = new ColorTable(50, 70, 80, 255, 0, 255);
                        break;
                    //                    case ColorVariation.Orange:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    case ColorVariation.Red:
                        table = new ColorTable(170, 10, 80, 255, 0, 255);
                        break;
                    case ColorVariation.Skin:
                        table = new ColorTable(0, 10, 80, 255, 0, 255);
                        break;
                    //                    case ColorVariation.White:
                    //                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                    //                        break;
                    default:
                        table = new ColorTable(0, 0, 0, 0, 0, 0);
                        break;
                }

                return ExtractMask(
                    srcMat,
                    code,
                    table.ch1Lower, table.ch1Upper,
                    table.ch2Lower, table.ch2Upper,
                    table.ch3Lower, table.ch3Upper
                    );
            }
            else
            {
                throw new NotImplementedException("HSV 値以外を用いた抽出は実装されていません");
            }



        }


        public static Mat ExtractMask(Mat srcMat, ColorConversionCodes code, ColorTable table)
        {
            return ExtractMask(
                srcMat,
                code,
                table.ch1Lower, table.ch1Upper,
                table.ch2Lower, table.ch2Upper,
                table.ch3Lower, table.ch3Upper
                );
        }
        public static Mat ExtractMask(Mat srcMat, ColorConversionCodes code,
        int ch1Lower, int ch1Upper,
        int ch2Lower, int ch2Upper,
        int ch3Lower, int ch3Upper)
        {
            if (srcMat == null)
                throw new ArgumentNullException("srcMat");

            var colorMat = srcMat.CvtColor(code);

            var lut = new Mat(256, 1, MatType.CV_8UC3);

            var lower = new int[3] { ch1Lower, ch2Lower, ch3Lower };
            var upper = new int[3] { ch1Upper, ch2Upper, ch3Upper };

            // cv::Mat_<cv::Vec3b>
            var mat3 = new MatOfByte3(lut);

            var indexer = mat3.GetIndexer();

            for (int i = 0; i < 256; i++)
            {
                var color = indexer[i];
                byte temp;

                for (int k = 0; k < 3; k++)
                {

                    if (lower[k] <= upper[k])
                    {
                        if ((lower[k] <= i) && (i <= upper[k]))
                        {
                            temp = 255;
                        }
                        else
                        {
                            temp = 0;
                        }
                    }
                    else
                    {
                        if ((i <= upper[k]) || (lower[k] <= i))
                        {
                            temp = 255;
                        }
                        else
                        {
                            temp = 0;
                        }
                    }

                    color[k] = temp;
                }

                indexer[i] = color;
            }

            Cv2.LUT(colorMat, lut, colorMat);

            var channelMat = colorMat.Split();

            var maskMat = new Mat();

            Cv2.BitwiseAnd(channelMat[0], channelMat[1], maskMat);
            Cv2.BitwiseAnd(maskMat, channelMat[2], maskMat);
            return maskMat;
        }
    }
}