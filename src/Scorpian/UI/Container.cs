namespace Scorpian.UI;

public interface Container
{
    void Attach(UIElement element);

    void Clear();

    void Remove(UIElement element);
}