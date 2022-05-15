
namespace ScriptDelivery.Maps.Requires
{
    internal enum RequireMode
    {
        None = 0,       //  条件無し。全ての状態に一致
        And = 1,        //  全ての条件に一致
        Or = 2,         //  いずれかの条件に一致
    }
}
