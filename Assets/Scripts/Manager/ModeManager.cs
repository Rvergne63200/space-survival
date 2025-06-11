using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class ModeManager
{
    private static ModeManager _instance;
    public static ModeManager Instance { 
        get
        {
            if (_instance == null)
            {
                _instance = new ModeManager();
            }

            return _instance;
        }
    }

    public UnityEvent ev_OnUpdateOptions;


    private PlayingMode mode = PlayingMode.Default;
    public PlayingMode Mode => mode;


    public ModeManager()
    {
        mode = PlayingMode.Default;
        ev_OnUpdateOptions = new UnityEvent();
    }

    public bool IsEnabled(string option)
    {
        foreach (var pair in PlayerModeOptions.Options)
        {
            PlayingMode mode = pair.Key;
            HashSet<string> options = pair.Value;

            if ((mode & this.mode) != 0)
            {
                if (options.Contains(option))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void EnableMode(PlayingMode modeToAdd)
    {
        if ((mode & modeToAdd) == modeToAdd) return;
        mode |= modeToAdd;

        ev_OnUpdateOptions.Invoke();
    }

    public void DisableMode(PlayingMode modeToRemove)
    {
        if ((mode & modeToRemove) == 0) return;
        mode &= ~modeToRemove;

        ev_OnUpdateOptions.Invoke();
    }
}
