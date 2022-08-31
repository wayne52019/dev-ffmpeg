package com.arthenica.ffmpegkit;

import android.app.Activity;
import android.util.Log;
import android.widget.Toast;

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;

/**
 * Created by Jing on 2018-1-18.
 */
public class Android2Unity {

    /**
     * unity项目启动时的的上下文
     */
    private Activity _unityActivity;
    /**
     * 获取unity项目的上下文
     * @return
     */
    Activity getActivity(){
        if(null == _unityActivity) {
            try {
                Class<?> classtype = Class.forName("com.unity3d.player.UnityPlayer");
                Activity activity = (Activity) classtype.getDeclaredField("currentActivity").get(classtype);
                _unityActivity = activity;
            } catch (ClassNotFoundException e) {

            } catch (IllegalAccessException e) {

            } catch (NoSuchFieldException e) {

            }
        }
        return _unityActivity;
    }

    /**
     * 调用Unity的方法
     * @param gameObjectName    為gameObjectName
     * @param functionName    為functionName
     * @param Message    為回傳Unity的訊息
     */
    public static void callUnity(String gameObjectName,String functionName,String Message){
        try {
            Class<?> classtype = Class.forName("com.unity3d.player.UnityPlayer");
            Method method = classtype.getMethod("UnitySendMessage", String.class, String.class, String.class);
            method.invoke(classtype, gameObjectName, functionName, Message);
            Log.d("Android2Unity", "呼叫Unity成功");
        } catch (ClassNotFoundException e) {
            Log.d("Android2Unity", "呼叫Unity失敗1");
        } catch (NoSuchMethodException e) {
            Log.d("Android2Unity", "呼叫Unity失敗2");
        } catch (IllegalAccessException e) {
            Log.d("Android2Unity", "呼叫Unity失敗3");
        } catch (InvocationTargetException e) {
            Log.d("Android2Unity", "呼叫Unity失敗4");
        }
    }
}
