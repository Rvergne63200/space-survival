using System.Collections.Generic;

public static class PlayerModeOptions
{
    static private Dictionary<PlayingMode, HashSet<string>> options = new Dictionary<PlayingMode, HashSet<string>>
    {
        {
            PlayingMode.Interface, new HashSet<string>
            {
                PlayerMovement.OPTION_DISABLED,
                PlayerInventory.OPTION_USE_DISABLED,
                InterfaceManager.OPTION_CURSOR_VISIBLE,
                CameraMovement.OPTION_CAMERA_LOCKED
            }
        },
        {
            // Placement, build or destruction (Builder): the clicks are used by the construction, not by the item in hand
            PlayingMode.Build, new HashSet<string>
            {
                PlayerInventory.OPTION_USE_DISABLED
            }
        },
        {
            // Main menu: free cursor, no player control
            PlayingMode.Menu, new HashSet<string>
            {
                PlayerMovement.OPTION_DISABLED,
                PlayerInventory.OPTION_USE_DISABLED,
                InterfaceManager.OPTION_CURSOR_VISIBLE,
                CameraMovement.OPTION_CAMERA_LOCKED
            }
        }
    };

    public static Dictionary<PlayingMode, HashSet<string>> Options { get { return options; } }
}
