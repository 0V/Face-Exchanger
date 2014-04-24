using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using FaceExchanger.Model;

namespace FaceExchanger
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static IplImage PutImage { get; set; }
        public static CvHaarClassifierCascade AnimeFaceCascade { get; set; }
        public static CvHaarClassifierCascade FaceCascade { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (_sender, _e) => Utils.ShowErrorMessage(_e.ExceptionObject as Exception);
            this.DispatcherUnhandledException += (_sender, _e) =>
            {
                Utils.ShowErrorMessage(_e.Exception);
                _e.Handled = true;
            };
            try
            {
                PutImage = FileManager.GetFaceImage();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex,"顔データが読み込めませんでした。");
                Environment.Exit(0);
            }

            try
            {
               // AnimeFaceCascade = FileManager.GetAnimeFaceCascade();
                FaceCascade = FileManager.GetFaceCascade();
            }
            catch (Exception exe)
            {
                Utils.ShowErrorMessage(exe, "顔認識用のカスケードファイルが読み込めませんでした。");
                Environment.Exit(0);
            }
        }
    }
}
