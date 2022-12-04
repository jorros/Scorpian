namespace Scorpian.UI;

public class Content
{
    public UIElement Value { get; }
    
    private Content(UIElement element)
    {
        Value = element;
    }
    
    public static implicit operator Content(string text)
    {
        return new Content(new Label
        {
            Text = text
        });
    }
    
    public static implicit operator Content(Image image)
    {
        return new Content(image);
    }
    
    public static implicit operator Content(Label label)
    {
        return new Content(label);
    }
    
    public static implicit operator Content(HorizontalGridLayout label)
    {
        return new Content(label);
    }
}