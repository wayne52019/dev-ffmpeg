
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace AR2VR
{
    public class FFmpegManager : InstanceBase<FFmpegManager>
    {
        //--------------------------------------------------事件--------------------------------------------------

        /// <summary>
        /// 開始轉檔事件
        /// </summary>
        public Action StartEncodeEvent;

        /// <summary>
        /// 轉檔進度更新事件
        /// </summary>
        public Action<string> EncodeProgressUpdateEvent;

        /// <summary>
        /// 轉檔成功事件
        /// </summary>
        public Action<string> SuccessEvent;

        /// <summary>
        /// 轉檔失敗事件
        /// </summary>
        public Action FailEvent;

        /// <summary>
        /// 完成事件，不論成功或者取消與否
        /// </summary>
        public Action CompleteEvent;

        /// <summary>
        /// 取消轉檔事件
        /// </summary>
        public Action CancelEvent;

        //-------------------------------------------------------------------------------------------------------

        string OutputPath;

        /// <summary>
        /// 是否在轉檔
        /// </summary>
        bool isEncoding;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void FFmpegKit_Execute(string gameObjectName, string completeMethodName, string progressMethodName, string cmd,string duration);
    [DllImport("__Internal")]
    private static extern void Cancel();
#elif UNITY_STANDALONE || UNITY_EDITOR
        HashSet<Action> CompleteCallBackActions = new HashSet<Action>();

        Process process;

        /// <summary>
        /// 是否取消
        /// </summary>
        bool isCancel;

        /// <summary>
        /// 回傳結果
        /// </summary>
        string PC_returnCode;

        /// <summary>
        /// 要轉檔影片的總長度
        /// </summary>
        int totalDuration = 1;

        /// <summary>
        /// 當前轉檔影片長度
        /// </summary>
        int nowDuration = 0;

        /// <summary>
        /// 複製檔案的路徑
        /// 備註:如果原檔路徑有含空白 就複製到該路徑 確保沒空白
        /// </summary>
        string copyFolderPath = Application.streamingAssetsPath + "/CopyFile/";

#if UNITY_STANDALONE_OSX || UNITY_EDITOR
        /// <summary>
        /// ffmpeg路徑
        /// </summary>
        string ffmpegExePath = Application.streamingAssetsPath + "/ffmpeg 3";
#elif UNITY_STANDALONE_WIN
    /// <summary>
    /// ffmpeg路徑
    /// </summary>
    string ffmpegExePath = Application.streamingAssetsPath + "/ffmpeg.exe";
#endif
#endif

        void Start()
        {
            Loom.Initialize();
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        void Update()
        {
            if (isEncoding)
            {
                //監聽完成的回調
                if (CompleteCallBackActions.Count != 0)
                {
                    LogManager.WriteLogFile("CompleteCallBackActions.count:" + CompleteCallBackActions.Count);
                    foreach (var action in CompleteCallBackActions)
                    {
                        LogManager.WriteLogFile("有執行complete");
                        print("有執行complete");
                        action();
                    }

                    LogManager.WriteLogFile("清理complete");
                    CompleteCallBackActions.Clear();
                }

                //影片才需要更新進度條
                EncodeProgressUpdate(((int)((float)nowDuration / (float)totalDuration * 100)).ToString());
            }
        }

        /// <summary>
        /// 影片時間轉換成影片長度
        /// </summary>
        /// <param name="timeStr"></param>
        /// <returns></returns>
        public int ConvertToDuration(string timeStr)
        {
            var time = timeStr.Split(':');

            print("time[0]:" + time[0] + "_time[1]:" + time[1] + "_time[2]:" + time[2]);

            double hh = double.Parse(time[0]) * 3600;

            double mm = double.Parse(time[1]) * 60;

            double ss = double.Parse(time[2]);

            /*print("hh:" + hh + "_mm:" + mm + "_ss:" + ss);

            print("(hh + mm + ss)*100:" + (hh + mm + ss) * 100);

            print("(int)((hh + mm + ss) * 100):" + (int)((hh + mm + ss) * 100));

            print("((hh + mm + ss) * 100).ToString():" + ((hh + mm + ss) * 100).ToString());

            print("int.Parse(((hh + mm + ss) * 100).ToString()):" + int.Parse(((hh + mm + ss) * 100).ToString()));*/

            int duration = int.Parse(((hh + mm + ss) * 100).ToString());

            print("duration:" + duration);

            return duration;
        }

        /// <summary>
        /// PC平台用來接收ffmpeg回傳資訊
        /// </summary>
        /// <param name="sendProcess"></param>
        /// <param name="output"></param>
        private void Output(object sendProcess, DataReceivedEventArgs output)
        {
            print("output.Data:" + output.Data);
            if (!string.IsNullOrEmpty(output.Data))
            {
                LogManager.WriteLogFile(output.Data);

                if (output.Data.ToLower().Contains("headers"))
                {
                    if (!isCancel)
                    {
                        print("完成");
                        PC_returnCode = "0";
                    }
                }

                if (output.Data.ToLower().Contains("no such file"))
                {
                    print("查無檔案");
                    PC_returnCode = "1";
                }

                if (output.Data.ToLower().Contains("error") || output.Data.ToLower().Contains("fail"))
                {
                    print("發生錯誤");
                    PC_returnCode = "1";
                }

                //解析ffmpeg吐回來的資訊 來取得影片長度
                //ex:Duration: 00:05:00.69, start: 0.000000, bitrate: 1075 kb/s
                string surchStr = "Duration: ";
                //用於過濾掉圖片
                string notSurchStr = "N/A";
                if (output.Data.Contains(surchStr) && !output.Data.Contains(notSurchStr))
                {
                    print("取得總長度");
                    string str = output.Data.Substring(output.Data.IndexOf(surchStr) + surchStr.Length, "00:05:00.69".Length);
                    totalDuration = ConvertToDuration(str);
                    print("總長度是：" + totalDuration);
                    LogManager.WriteLogFile("總長度是：" + totalDuration);
                }

                surchStr = "time=";
                //解析ffmpeg吐回來的資訊 來取得當前轉檔影片長度
                //ex:frame=   91 fps= 46 q=-1.0 Lsize=     333kB time=00:00:03.91 bitrate= 697.6kbits/s dup=18 drop=2 speed=1.99x    
                if (output.Data.Contains(surchStr) && !output.Data.Contains(notSurchStr))
                {
                    print("取得當前轉檔影片長度");
                    string str = output.Data.Substring(output.Data.IndexOf(surchStr) + surchStr.Length, "0:00:03.91".Length);
                    //string str = output.Data.Substring(output.Data.IndexOf(surchStr) + surchStr.Length, "0:00:03.91".Length);
                    nowDuration = ConvertToDuration(str);
                    print("當前轉檔影片長度：" + nowDuration);
                    LogManager.WriteLogFile("當前轉檔影片長度：" + nowDuration);
                }
            }
        }
#endif
        void OnApplicationQuit()
        {
            if (isEncoding)
            {
                CallCancel();
                CompleteCallBack("2");
                LogManager.WriteLogFile("轉檔時，關閉應用程式");
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void CallCancel()
        {
#if UNITY_IOS
        Cancel();
#elif UNITY_ANDROID
        AndroidJavaClass javaClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKit");
        javaClass.CallStatic("cancel");
#elif UNITY_STANDALONE || UNITY_EDITOR
            if (isCancel == false)
            {
                isCancel = true;
                PC_returnCode = "2";
                print("請求取消");
                LogManager.WriteLogFile("請求取消");
                var Bool = process.CloseMainWindow();
                print("關閉視窗是否被接受：" + Bool);
                LogManager.WriteLogFile("關閉視窗是否被接受：" + Bool);
                //如果關閉視窗被
                if (!Bool)
                {
                    process.Kill();
                    CompleteCallBack("2");
                }
            }
#endif
        }

        /// <summary>
        /// 呼叫FFmpeg
        /// </summary>
        /// <param name="path"></param>
        public void callFFmpeg(string cmd, string outputPath, string duration = "")
        {
            //確保一次只執行一個
            if (isEncoding)
                return;
            OutputPath = outputPath;
            isEncoding = true;

#if UNITY_STANDALONE || UNITY_EDITOR
            LogManager.CreateLogFile();
            LogManager.WriteLogFile("輸出路徑：" + OutputPath);
            LogManager.WriteLogFile("FFmpeg執行指令：" + cmd);
            print("輸出路徑：" + OutputPath);
            print("FFmpeg執行指令：" + cmd);
            //參考：https://www.twblogs.net/a/5d668ddfbd9eee541c33567f
            Loom.RunAsync(() =>
            {
                //建立外部調用進程
                process = new Process();
                process.StartInfo.FileName = ffmpegExePath;
                process.StartInfo.Arguments = cmd;
                process.StartInfo.UseShellExecute = false;//不使用操作系統外殼程序啓動線程(一定爲FALSE,詳細的請看MSDN)
                process.StartInfo.RedirectStandardError = true;//把外部程序錯誤輸出寫到StandardError流中(這個一定要注意,FFMPEG的所有輸出信息,都爲錯誤輸出流,用StandardOutput是捕獲不到任何消息的...)
                process.StartInfo.CreateNoWindow = true;//不創建進程窗口
                process.ErrorDataReceived += new DataReceivedEventHandler(Output);//外部程序(這裏是FFMPEG)輸出流時候產生的事件,這裏是把流的處理過程轉移到下面的方法中,詳細請查閱MSDN

                nowDuration = 0;
                isCancel = false;

                process.Start();//啓動線程
                process.BeginErrorReadLine();//開始異步讀取
                process.WaitForExit();//阻塞等待進程結束
                print("執行結束");
                LogManager.WriteLogFile("執行結束");
                process.Close();//關閉進程
                process.Dispose();//釋放資源
                LogManager.WriteLogFile("PC_returnCode:" + PC_returnCode);
                CompleteCallBackActions.Add(() => CompleteCallBack(PC_returnCode));
                print("我有釋放資源");
                LogManager.WriteLogFile("我有釋放資源");
            });
#elif UNITY_ANDROID
            AndroidJavaClass configClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKitConfig");
            AndroidJavaObject paramVal = new AndroidJavaClass("com.arthenica.ffmpegkit.Signal").GetStatic<AndroidJavaObject>("SIGXCPU");
            configClass.CallStatic("ignoreSignal", new object[] { paramVal });
            AndroidJavaClass javaClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKit");

            AndroidJavaObject session = javaClass.CallStatic<AndroidJavaObject>("executeAsync",
                new object[] { cmd, duration, gameObject.name, "CompleteCallBack",
                gameObject.name, "EncodeProgressUpdate" });
#elif UNITY_IOS
            FFmpegKit_Execute(gameObject.name, "CompleteCallBack", "EncodeProgressUpdate", cmd, duration);
#endif
            if (StartEncodeEvent != null)
                StartEncodeEvent();
        }

        /// <summary>
        /// ffmpeg處理完成後的回調
        /// </summary>
        /// <param name="returnCode"></param>
        void CompleteCallBack(string returnCode)
        {
            string str = "";

            print("完成Return：" + (str = returnCode == "2" ? "取消" : returnCode == "1" ? "有錯" : "完成"));
#if UNITY_IOS
            //轉檔成功
            if (returnCode == "0")
            {
                print("成功了!!!!!!!!!!!!!!!!!!!");
                if (SuccessEvent != null)
                    SuccessEvent(OutputPath);
            }
            else if (returnCode == "2")
            {
                if (CancelEvent != null)
                    CancelEvent();

                print("取消了!!!!!!!!!!!!!!!!!!!");
            }
            else if (returnCode == "1")
            {
                if (FailEvent != null)
                    FailEvent();

                print("發生錯誤!!!!!!!!!!!!!!!!!!!");
            }
#elif UNITY_ANDROID
            //轉檔成功
            if (returnCode == "0")
            {
                print("成功了!!!!!!!!!!!!!!!!!!!");
                if (SuccessEvent != null)
                    SuccessEvent(OutputPath);
            }
            else if (returnCode == "255")
            {
                print("取消了!!!!!!!!!!!!!!!!!!!");

                if (CancelEvent != null)
                    CancelEvent();

                if (File.Exists(OutputPath))
                {
                    File.Delete(OutputPath);
                    print("檔案刪除了");
                }
            }
            else
            {
                print("發生錯誤!!!!!!!!!!!!!!!!!!!");

                if (FailEvent != null)
                    FailEvent();

                if (File.Exists(OutputPath))
                {
                    File.Delete(OutputPath);
                    print("檔案刪除了");
                }
            }

#elif UNITY_STANDALONE || UNITY_EDITOR
            LogManager.WriteLogFile("CompleteCallBack");
            if (Directory.Exists(copyFolderPath))
            {
                var copyFiles = Directory.GetFiles(copyFolderPath);
                if (copyFiles.Length > 0)
                {
                    foreach (var file in copyFiles)
                    {
                        File.Delete(file);
                    }
                }
            }

            //轉檔成功
            if (returnCode == "0")
            {
                print("成功了!!!!!!!!!!!!!!!!!!!");
                LogManager.WriteLogFile("轉檔成功");
                if (SuccessEvent != null)
                    SuccessEvent(OutputPath);
            }
            else if (returnCode == "2")
            {
                print("取消了!!!!!!!!!!!!!!!!!!!");
                LogManager.WriteLogFile("取消");
                
                if (CancelEvent != null)
                    CancelEvent();

                if (File.Exists(OutputPath))
                {
                    File.Delete(OutputPath);
                }
            }
            else if (returnCode == "1")
            {
                print("發生錯誤!!!!!!!!!!!!!!!!!!!");
                LogManager.WriteLogFile("發生錯誤");
                
                if (FailEvent != null)
                    FailEvent();

                if (File.Exists(OutputPath))
                {
                    File.Delete(OutputPath);
                }
            }
#endif
            if (CompleteEvent != null)
                CompleteEvent();

            //清空事件
            CompleteEvent = null;
            FailEvent = null;
            SuccessEvent = null;
            CancelEvent = null;
            StartEncodeEvent = null;
            EncodeProgressUpdateEvent = null;

            isEncoding = false;
            print("returnCode:" + returnCode);
        }

        /// <summary>
        /// 更新進度
        /// </summary>
        /// <param name="progress"></param>
        void EncodeProgressUpdate(string progress)
        {
            print("progress:" + progress);

            if (EncodeProgressUpdateEvent != null)
            {
                EncodeProgressUpdateEvent(progress);
            }
        }
    }
}