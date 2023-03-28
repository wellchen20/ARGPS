using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainVoice : MonoBehaviour
{
    // Start is called before the first frame update
    //public GameObject LocationObj;
    void Start()
    {
        //LocationObj.GetComponent<ARLocation.PlaceAtLocation.LocationSettingsData>().LocationInput.Location.Latitude = 40.1043505639648;//40.1643905639648
        //LocationObj.GetComponent<ARLocation.PlaceAtLocation.LocationSettingsData>().LocationInput.Location.Longitude = 116.202656533203;//116.232696533203
    }
    private void Awake()
    {
        RegisterCommand();
    }

    //语音注册
    private void RegisterCommand()
    {
        VoiceCommandLogic.Instance.AddInstrucEntityZH("退出程序", "tui chu cheng xu", true, true, true, this.gameObject.name, "ExitApp", "退出程序");
        VoiceCommandLogic.Instance.AddInstrucEntityZH("确定", "que ding", true, true, true, this.gameObject.name, "MakeSure", "确定");
    }
    //语音注销
    private void UnRegisterCommand()
    {
        VoiceCommandLogic.Instance.ClearUserInstruct();
    }

    //退出app
    public void ExitApp()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) //yidao dpad center
        {

        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {

        }
    }

    private void OnDestroy()
    {
        UnRegisterCommand();
    }
}
