using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class InterfaceManager
{
    public static string OPTION_CURSOR_VISIBLE = "interface_manager_cursor_visible";

    private static InterfaceManager instance;
    private List<GameObject> interfaces;

    public static InterfaceManager GetInstance()
    {
        if(instance == null)
        {
            instance = new InterfaceManager();
        }

        return instance;
    }

    public InterfaceManager()
    {
        SetCursorVisible(false);
        interfaces = new List<GameObject>();
        ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);
    }

    public void OnUpdateOptions()
    {
        SetCursorVisible(ModeManager.Instance.IsEnabled(OPTION_CURSOR_VISIBLE));
    }

    private void SetCursorVisible(bool isVisible)
    {
        if (isVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    // Manage interfaces
    public void SetInterface(GameObject component, bool value)
    {
        bool interfaceIsOpenBefore = interfaces.Count > 0;
        bool contains = interfaces.Contains(component);

        if (value)
        {
            if (!contains)
            {
                interfaces.Add(component);
            }
        }
        else if(contains)
        {
            interfaces.Remove(component);
        }

        bool interfaceIsOpenAfter = interfaces.Count > 0;

        if(interfaceIsOpenAfter != interfaceIsOpenBefore)
        {
            if (interfaceIsOpenAfter)
            {
                ModeManager.Instance.EnableMode(PlayingMode.Interface);
            }
            else
            {
                ModeManager.Instance.DisableMode(PlayingMode.Interface);
            }
        }
    }

    public void CloseInterfaces()
    {
        GameObject[] interfacesArray = interfaces.ToArray();

        foreach(GameObject component in interfacesArray)
        {
            component.gameObject.SetActive(false);
            SetInterface(component, false);
        }
    }
}
