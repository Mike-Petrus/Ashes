using System.Collections.Generic;

public interface IMenuState
{
    IReadOnlyList<string> MenuOptions { get; }
    int CurrentIndex { get; }
}