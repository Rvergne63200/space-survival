using System;

[Flags]
public enum PlayingMode
{
    None = 0,
    Default = 1 << 0,
    Interface = 1 << 1,
    Build = 1 << 2
}
