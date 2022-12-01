using System.Drawing;
using Scorpian.Asset;
using Scorpian.Graphics;
using Scorpian.SceneManagement;
using Scorpian.UI;
using Scorpian.UI.Style;

namespace Scorpian.Sample;

public class SampleScene : Scene
{
    private BasicLayout _layout = null!;
    private Stylesheet _stylesheet = null!;
    public override Color BackgroundColor => Color.Bisque;

    private int _sizeIncrement = 1;
    private Label _label;

    protected override void OnLoad(AssetManager assetManager)
    {
        assetManager.Load("Sample");
        _stylesheet = new Stylesheet(assetManager);
        
        var defaultLabel = _stylesheet.CreateLabelStyle(null, "Sample:Gidole-Regular");
        defaultLabel.Color = Color.Black;
        defaultLabel.Size = 40;
        
        _layout = new BasicLayout();

        _label = new Label
        {
            Anchor = UIAnchor.Center,
            Text = "This is a sample",
            Size = 40
        };
        _layout.Attach(_label);
    }

    protected override void OnUpdate()
    {
        _label.Size += _sizeIncrement;

        if (_label.Size is >= 80 or <= 30)
        {
            _sizeIncrement *= -1;
        }
    }

    protected override void OnRender(RenderContext context)
    {
        _layout.Render(context, _stylesheet, false);
    }
}