using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using AR2VR;

public class CircuitManager : MonoBehaviour
{
    string FileName;

    string OutputPath;

    enum Type
    {
        img,
        audio,
        video
    }

    Type selectType;

    public int ar = 32000;

    public int ac = 2;

    public string ab = "96k";

    string Resolution = "1280x720";

    string videoDataRate = "1000K";

    public InputField videoDataRateInputField;

    public string DefaultDataRate = "1000K";

    public InputField resolutionWidthInputField;

    public string DefultResolutionWidth = "1280";

    public InputField resolutionHeightInputField;

    public string DefultResolutionHeight = "720";

    string aspect;

    float time;

    bool canUpdate;

#if UNITY_STANDALONE
    /// <summary>
    /// 複製檔案的路徑
    /// 備註:如果原檔路徑有含空白 就複製到該路徑 確保沒空白
    /// </summary>
    string copyFolderPath = Application.streamingAssetsPath + "/CopyFile/";
#endif

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(updateTime());

        videoDataRateInputField.text = DefaultDataRate;

        resolutionWidthInputField.text = DefultResolutionWidth;

        resolutionHeightInputField.text = DefultResolutionHeight;

        aspect = (float.Parse(DefultResolutionWidth) / float.Parse(DefultResolutionHeight)).ToString();
    }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
    /// <summary>
    /// 『行動裝置』將轉檔後的圖片存進相簿後的回調
    /// </summary>
    /// <param name="success"></param>
    /// <param name="path"></param>
    public void SaveImgCall(bool success, string path)
    {
        if (File.Exists(OutputPath))
        {
            File.Delete(OutputPath);
            print("檔案刪除了");
        }

        print("是否成功：" + success);
        print("路徑：" + path);
    }
#endif
    /// <summary>
    /// 選取照片
    /// </summary>
    public void getImg()
    {
        selectType = Type.img;

#if UNITY_STANDALONE
        getCallBack(FileBrowser.OpenSingleFile("Open single file", Application.streamingAssetsPath + "/", new ExtensionFilter("Image Files", "png", "jpg")));
#else
        NativeGallery.GetImageFromGallery((p)=>getCallBack(p));
#endif
    }

    /// <summary>
    /// 選取影片
    /// </summary>
    public void getVideo()
    {
        selectType = Type.video;

#if UNITY_STANDALONE || UNITY_EDITOR
        getCallBack(FileBrowser.OpenSingleFile("Open single file", Application.streamingAssetsPath + "/", new ExtensionFilter("Video Files", "mp4", "mov")));
#else
        NativeGallery.GetVideoFromGallery((p)=> getCallBack(p));
#endif
    }

    /// <summary>
    /// 選取影片
    /// </summary>
    public void getVideo2()
    {
        selectType = Type.video;

        if (string.IsNullOrEmpty(resolutionWidthInputField.text))
            resolutionWidthInputField.text = DefultResolutionWidth;

        videoDataRate = videoDataRateInputField.text +"K";

        if (string.IsNullOrEmpty(resolutionHeightInputField.text))
            resolutionWidthInputField.text = DefultResolutionHeight;

        if (string.IsNullOrEmpty(videoDataRateInputField.text))
            videoDataRateInputField.text = DefaultDataRate;

        Resolution = resolutionWidthInputField.text + "x" + resolutionHeightInputField.text;

        aspect = (float.Parse(resolutionWidthInputField.text) /
            float.Parse(resolutionHeightInputField.text)).ToString();

#if UNITY_STANDALONE || UNITY_EDITOR
        getCallBack(FileBrowser.OpenSingleFile("Open single file", Application.streamingAssetsPath + "/", new ExtensionFilter("Video Files", "mp4", "mov")));
#else
        NativeGallery.GetVideoFromGallery((p)=> getCallBack(p));
#endif
    }

    /// <summary>
    /// wav轉mp3
    /// </summary>
    public void ToMp3()
    {
        selectType = Type.audio;

#if UNITY_STANDALONE
        getCallBack(FileBrowser.OpenSingleFile("Open single file", Application.streamingAssetsPath + "/", new ExtensionFilter("Audio Files", "wav","mp3")),true);
#else
        NativeGallery.GetAudioFromGallery((p)=>getCallBack(p,true));
#endif
    }

    /// <summary>
    /// wav轉ogg
    /// </summary>
    public void ToOgg()
    {
        selectType = Type.audio;

#if UNITY_STANDALONE
        getCallBack(FileBrowser.OpenSingleFile("Open single file", Application.streamingAssetsPath + "/", new ExtensionFilter("Audio Files", "wav","mp3")), false, true);
#else
        NativeGallery.GetAudioFromGallery((p)=>getCallBack(p,false,true));
#endif
    }

    /// <summary>
    /// 取得檔案後的回調
    /// </summary>
    /// <param name="path"></param>
    public void getCallBack(string path, bool ToMp3 = false, bool ToOgg = false)
    {
        if (string.IsNullOrEmpty(path))
            return;

        print("get Path:" + path);

        //開啟loading
        LoadingController.instance.LoadingSwitch(true, selectType == Type.video);

        string cmd = "";

        string duration = "";

        path = path.Replace("\\", "/");

        string inputFileName = path.Substring(path.LastIndexOf("/") + 1, path.LastIndexOf(".") - (path.LastIndexOf("/") + 1)); ;

#if UNITY_STANDALONE
        LogManager.CreateLogFile();
        LogManager.WriteLogFile("取得檔案路徑原始路徑：" + path);
        //如果路徑中含有空白
        if (path.Contains(" "))
        {
            inputFileName = inputFileName.Replace(" ", "");
            var FilenameExtension = path.Substring(path.LastIndexOf("."), path.Length - path.LastIndexOf("."));
            if (!Directory.Exists(copyFolderPath))
            {
                Directory.CreateDirectory(copyFolderPath);
            }
            File.Copy(path, copyFolderPath + inputFileName + FilenameExtension, true);
            path = copyFolderPath + inputFileName + FilenameExtension;
            LogManager.WriteLogFile("修改後的取得檔案路徑：" + path);
        }
#endif

        //如果是圖片
        if (selectType == Type.img)
        {
            FileName = inputFileName + "-new.jpg";
#if UNITY_STANDALONE
            //設定輸出路徑
            OutputPath = Application.streamingAssetsPath + "/" + FileName;
#else
            OutputPath = Application.persistentDataPath + "/" + FileName;
#endif
            // put your ffmpeg command here
            cmd = "-i " + "\"" + path + "\"" + " -y " + "\"" + OutputPath + "\"";
        }
        //如果是影片
        else if (selectType == Type.video)
        {
            FileName = inputFileName + " " + Resolution + " -new.mp4";
            //設定輸出路徑
#if UNITY_STANDALONE || UNITY_EDITOR
            if(!Directory.Exists(Application.streamingAssetsPath + "/Video/" + inputFileName))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath + "/Video/" + inputFileName);
            }
            OutputPath = Application.streamingAssetsPath + "/Video/" + inputFileName + "/" + FileName;
#else
            OutputPath = Application.persistentDataPath + "/" + FileName;
            //取得影片時間
            var properties = NativeGallery.GetVideoProperties(path);
            duration = properties.duration.ToString();
#endif

#if UNITY_STANDALONE_WIN
            cmd = "-i " + "\"" + path + "\"" + " -aspect " + aspect + " -b:v "+ videoDataRate + " -y -c:v h264 -s " + Resolution + " \"" + OutputPath + "\"";
#else
            cmd = "-i " + "\"" + path + "\"" + " -aspect " + aspect + " -b " + videoDataRate + " -y -c:v h264 -s " + Resolution + " \"" + OutputPath + "\"";
#endif

            FFmpegManager.Instance.EncodeProgressUpdateEvent = ((p) =>
              {
                  LoadingController.instance.UpdateProgress(p);
              });
        }
        else if (selectType == Type.audio)
        {
            var originalFileType = path.Substring(path.LastIndexOf(".") + 1, path.Length - (path.LastIndexOf(".") + 1));

            if (ToMp3 == true)
            {
                FileName = inputFileName + "_ac:" + ac + "_ar:" + ar + "_ab:" + ab + "_[" +
                    originalFileType + " => mp3" + "]-new.mp3";
            }
            else if (ToOgg == true)
            {
                FileName = inputFileName + "_ac:" + ac + "_ar:" + ar + "_ab:" + ab + "_[" +
                    originalFileType + " => ogg" + "]-new.ogg";
            }
            //設定輸出路徑
#if UNITY_STANDALONE
            OutputPath = Application.streamingAssetsPath + FileName;
#else
            OutputPath = Application.persistentDataPath + FileName;
            //取得影片時間
            var properties = NativeGallery.GetVideoProperties(path);
            duration = properties.duration.ToString();
#endif

#if UNITY_STANDALONE_WIN
            cmd = "-i " + "\""+ path + "\""+ " -ar " + ar + " -ac "+ac + " -ab:v "+ab+" -y "+ "\"" + OutputPath+ "\"";
#else
            cmd = "-i " + "\"" + path + "\"" + " -ar " + ar + " -ac " + ac + " -ab " + ab + " -y " + "\"" + OutputPath + "\"";
#endif

            FFmpegManager.Instance.EncodeProgressUpdateEvent = (p) =>
            {
                LoadingController.instance.UpdateProgress(p);
            };
        }


        print("OutputPath" + OutputPath);
        print("cmd:" + cmd);

        FFmpegManager.Instance.CompleteEvent = () =>
          {
              print("完成");
              //開啟loading
              LoadingController.instance.LoadingSwitch(false, selectType == Type.video);
              canUpdate = false;
              if (ToMp3)
                  print("ToMp3轉檔時間：" + time);
              else if (ToOgg)
                  print("ToOgg轉檔時間：" + time);
              else
                  print("轉檔時間：" + time);
          };

#if UNITY_STANDALONE
        FFmpegManager.Instance.SuccessEvent = (p) =>
        {
            if (selectType == Type.video)
                System.Diagnostics.Process.Start(@Application.streamingAssetsPath + "/Video/" + inputFileName);
        };

        FFmpegManager.Instance.FailEvent = () =>
        {
            System.Diagnostics.Process.Start(@LogManager.GetNowLogFile());
        };
#endif

        FFmpegManager.Instance.StartEncodeEvent = () =>
          {
              time = 0;
              canUpdate = true;
          };

        FFmpegManager.Instance.callFFmpeg(cmd, OutputPath, duration);
    }

    public IEnumerator updateTime()
    {
        yield return new WaitForSeconds(0.1f);

        if (canUpdate)
            time += 0.1f;

        StartCoroutine(updateTime());
    }
}
