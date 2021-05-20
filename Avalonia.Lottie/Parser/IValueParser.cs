namespace Avalonia.Lottie.Parser
{
    internal interface IValueParser<out T>
    {
        T Parse(JsonReader reader);
    }
}