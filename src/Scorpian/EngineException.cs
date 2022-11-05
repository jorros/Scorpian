using System;

namespace Scorpian;

public class EngineException : Exception
{
    public EngineException(string message) : base(message)
    {
    }
}