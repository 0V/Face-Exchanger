using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
namespace FaceExchanger
{
    public class Utils
    {
        /// <summary>
        /// ログを残しアプリケーションを通常終了
        /// </summary>
        /// <param name="comment">option コメント</param>
        public static void Exit(string message = "No message")
        {
            try
            {
                WriteNormalLog(message);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 例外オブジェクトの情報をファイルに出力しアプリケーションを終了
        /// </summary>
        /// <param name="ex">Exeption型のオブジェクト</param>
        /// <param name="comment">option 例外のコメント</param>
        public static void Exit(Exception ex, string comment = "例外が発生しました")
        {
            try
            {
                WriteErrorLog(ex, comment);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// ファイルに出力
        /// </summary>
        /// <param name="comment">出力内容</param>
        /// <param name="directoryName">option ディレクトリ名</param>
        public static void WriteNormalLog(string comment,
          string directoryName = "Log")
        {
            string fileName = string.Format("Log{0}.txt", GetTimeString());
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            string contents = comment + "\n";

            File.WriteAllText(directoryName + "/" + fileName, contents);
        }

        /// <summary>
        /// ファイルに出力
        /// </summary>
        /// <param name="comment">出力内容</param>
        /// <param name="directoryName">option ディレクトリ名</param>
        public static void WriteExitLog(string comment,
          string directoryName = "ExitLog")
        {
            string fileName = string.Format("ExitLog{0}.txt", GetTimeString());
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            string contents = comment + "\n\n" + "プログラムを通常終了しました";

            File.WriteAllText(directoryName + "/" + fileName, contents);
        }

        /// <summary>
        /// 例外オブジェクトの情報をファイルに出力
        /// </summary>
        /// <param name="ex">例外オブジェクト</param>
        /// <param name="comment">option 例外のコメント</param>
        /// <param name="directoryName">option ディレクトリ名</param>
        public static void WriteExitLog(Exception ex, string comment = "例外が発生しました",
          string directoryName = "ErrorExitLog")
        {
            string fileName = string.Format("ErrorExitLog{0}.txt", GetTimeString());
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            string contents = comment + "\n" + GetExceptionInfoString(ex) + "\n\n" + "プログラムを例外終了しました";

            File.WriteAllText(@directoryName + "/" + fileName, contents);
        }

        /// <summary>
        /// 例外オブジェクトの情報をファイルに出力
        /// </summary>
        /// <param name="ex">例外オブジェクト</param>
        /// <param name="comment">option 例外のコメント</param>
        /// <param name="directoryName">option ディレクトリ名</param>
        public static void WriteErrorLog(Exception ex, string comment = "例外が発生しました",
          string directoryName = "ErrorLog")
        {
            string fileName = string.Format("ErrorLog{0}.txt", GetTimeString());
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            string contents = comment + "\n" + GetExceptionInfoString(ex) + "\n";

            File.WriteAllText(@directoryName + "/" + fileName, contents);
        }

        /// <summary>
        /// 例外オブジェクトからデバッグに必要な情報を文字列で返す
        /// </summary>
        /// <param name="ex">例外オブジェクト</param>
        /// <returns>例外情報の文字列</returns>
        public static string GetExceptionInfoString(Exception ex)
        {
            var str = new StringBuilder();
            str.AppendFormat("Message:{0}\n", ex.Message);
            str.AppendFormat("Source:{0}\n", ex.Source);
            str.AppendFormat("HelpLink:{0}\n", ex.HelpLink);
            str.AppendFormat("TargetSite:{0}\n", ex.TargetSite.ToString());
            str.AppendFormat("StackTrace:{0}\n", ex.StackTrace);
            return str.ToString();
        }

        /// <summary>
        /// 呼び出された時間を年から秒までの文字列で返す
        /// </summary>
        /// <returns>時間の文字列</returns>
        public static string GetTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        }



        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show("エラーが発生しました\n" + message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowErrorMessage(Exception e)
        {
            MessageBox.Show("エラーが発生しました\n" + e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowErrorMessage(Exception e, string message)
        {
            MessageBox.Show("エラーが発生しました\n" + message + "\n" + e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowMessage(string message, string window = "メッセージ")
        {
            MessageBox.Show(message, window, MessageBoxButton.OK);
        }
    }


    public static class IEnumerableExtensions
    {
        /// <summary>
        /// 最大値を持つ要素を返します
        /// </summary>
        public static TSource FindMax<TSource, TResult>(
        this IEnumerable<TSource> self,
        Func<TSource, TResult> selector)
        {
            return self.First(c => selector(c).Equals(self.Max(selector)));
        }
    }
}