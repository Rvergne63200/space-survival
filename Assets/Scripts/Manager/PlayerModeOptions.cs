using System.Collections.Generic;

public static class PlayerModeOptions
{
    static private Dictionary<PlayingMode, HashSet<string>> options = new Dictionary<PlayingMode, HashSet<string>>
    {
        {
            PlayingMode.Interface, new HashSet<string>
            {
                PlayerMovement.OPTION_DISABLED,
                InterfaceManager.OPTION_CURSOR_VISIBLE,
                CameraMovement.OPTION_CAMERA_LOCKED
            }
        }
    };

    public static Dictionary<PlayingMode, HashSet<string>> Options { get { return options; } }
}
