using System;

namespace GameLogic.Input.Interfaces
{
    public interface IInputSource
    {
        event Action<IInputCommand> OnCommand;
        
        
    }
}