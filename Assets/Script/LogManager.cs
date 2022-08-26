using System.IO;
using System;
using UnityEngine;

public class LogManager
{
    /// <summary>
    /// Log檔存放的資料夾路徑
    /// </summary>
    static string logFolderPath = Application.streamingAssetsPath + "/Log/";

    /// <summary>
    /// Log檔案上限
    /// </summary>
    static int logFileMax = 4;

    /// <summary>
    /// 當前log檔案路徑
    /// </summary>
    static string logFilePath;

    // Start is called before the first frame update
    public static void CreateLogFile()
    {
        if (!Directory.Exists(logFolderPath))
        {
            Directory.CreateDirectory(logFolderPath);
        }
        var files = Directory.GetFiles(logFolderPath);
        if (files.Length >= logFileMax)
        {
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        logFilePath = logFolderPath + "Log_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
        File.Create(logFilePath);

    }


    /// <summary>
    /// 寫入Log檔
    /// </summary>
    /// <param name="content"></param>
    public static void WriteLogFile(string content)
    {
        try
        {
            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw = new StreamWriter(logFilePath, true);
            //Write a line of text
            sw.WriteLine(content);
            //Close the file
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Executing finally block.");
        }
    }
}
