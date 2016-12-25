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
        public const string FaceCascadeName = "Cascades/haarcascade_frontalface_alt.xml";
        public const string AnimeFaceCascadeName = "Cascades/lbpcascade_animeface.xml";
        public const string EyeCascadeName = "Cascades/haarcascade_eye.xml";
        public const string EyeGlassCascadeName = "Cascades/haarcascade_eye_tree_eyeglasses.xml";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (_sender, _e) => Utils.ShowErrorMessage(_e.ExceptionObject as Exception);
            this.DispatcherUnhandledException += (_sender, _e) =>
            {
                Utils.ShowErrorMessage(_e.Exception);
                _e.Handled = true;
            };
        }
    }
}
