using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public static LoadingController instance;

    public GameObject LoadingObj;

    public Text ProgressText;

    public Image LoadingImg;

    public Button CancelBtn;

    fileAmountMode nowMode = fileAmountMode.dee;

    public enum fileAmountMode
    {
        add = 0,
        dee = 1
    }

    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(LoadingObj);
            Destroy(this);
        }
    }

    public IEnumerator TurnRun()
    {
        LoadingImg.transform.localEulerAngles = new Vector3(LoadingImg.transform.localEulerAngles.x, LoadingImg.transform.localEulerAngles.y, LoadingImg.transform.localEulerAngles.z + 2);
        
        if(nowMode == 0)
        {
            LoadingImg.fillAmount += 0.005f;

            if (LoadingImg.fillAmount >= 1)
            {
                nowMode = fileAmountMode.dee;
                LoadingImg.fillClockwise = false;
            }
        }
        else
        {
            LoadingImg.fillAmount -= 0.005f;

            if (LoadingImg.fillAmount <= 0)
            {
                nowMode = fileAmountMode.add;
                LoadingImg.fillClockwise = true;
            }
        }

        yield return new WaitForSeconds(0.01f);

        if (LoadingObj.activeSelf == true)
            StartCoroutine(TurnRun());
    }

    // Update is called once per frame
    public void LoadingSwitch(bool open,bool isVideo = false)
    {
        if (open && LoadingObj.activeSelf == false)
        {
            if(isVideo)
                ProgressText.text = "0 %";
            else
                ProgressText.text = "";

            StartCoroutine(TurnRun());
        }

        LoadingObj.SetActive(open);
    }

    public void UpdateProgress(string progress)
    {
        ProgressText.text = progress+" %";
    }
}
