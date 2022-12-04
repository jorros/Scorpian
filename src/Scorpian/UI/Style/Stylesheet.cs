#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using Scorpian.Asset;
using Scorpian.Asset.Font;

namespace Scorpian.UI.Style;

public class Stylesheet
{
    private readonly Dictionary<string, Font> _fonts = new();
    private readonly Dictionary<string, ButtonStyle> _buttons = new();
    private readonly Dictionary<string, LabelStyle> _labels = new();
    private readonly Dictionary<string, WindowStyle> _windows = new();
    private readonly Dictionary<string, TextInputStyle> _inputs = new();
    private readonly Dictionary<string, RadioButtonStyle> _radios = new();
    private readonly Dictionary<string, HorizontalDividerStyle> _horizontalDividers = new();
    private readonly Dictionary<string, ProgressBarStyle> _progressBars = new();

    private Font? _defaultFont;
    private ButtonStyle? _defaultButton;
    private LabelStyle? _defaultLabel;
    private WindowStyle? _defaultWindow;
    private TextInputStyle? _defaultInput;
    private RadioButtonStyle? _defaultRadio;
    private HorizontalDividerStyle? _defaultHorizontalDivider;
    private ProgressBarStyle? _defaultProgressBar;
    private readonly AssetManager _assetManager;

    public Stylesheet(AssetManager assetManager)
    {
        _assetManager = assetManager;
        ScaleModifier = assetManager.HighRes ? 1.0 : 0.5;
    }
    
    public double ScaleModifier { get; }

    public int Scale(int val)
    {
        return (int)Math.Floor(val * ScaleModifier);
    }
    
    public float Scale(float val)
    {
        return (float)(val * ScaleModifier);
    }

    public Point Scale(Point val)
    {
        return new Point(Scale(val.X), Scale(val.Y));
    }
    
    public PointF Scale(PointF val)
    {
        return new PointF(Scale(val.X), Scale(val.Y));
    }
    
    #region ProgressBar
    public ProgressBarStyle CreateProgressBarStyle(string? name, string fillSprite, string? backgroundSprite = null, string? foregroundSprite = null)
    {
        var style = new ProgressBarStyle
        {
            Fill = _assetManager.Get<Sprite>(fillSprite)
        };

        if (backgroundSprite is not null)
        {
            style.Background = _assetManager.Get<Sprite>(backgroundSprite);
        }

        if (foregroundSprite is not null)
        {
            style.Foreground = _assetManager.Get<Sprite>(foregroundSprite);
        }

        if (name is null)
        {
            _defaultProgressBar = style;
            return style;
        }

        _progressBars[name] = style;
        return style;
    }
    
    public ProgressBarStyle GetProgressBar(string? name = null)
    {
        if (name is not null && _progressBars.ContainsKey(name))
        {
            return _progressBars[name];
        }
        
        if (_defaultProgressBar is null)
        {
            throw new EngineException("No default progress bar style set in stylesheet.");
        }

        return _defaultProgressBar;
    }
    #endregion
    
    #region HorizontalDivider
    public HorizontalDividerStyle CreateHorizontalDividerStyle(string? name, string backgroundSprite)
    {
        var style = new HorizontalDividerStyle
        {
            Background = _assetManager.Get<Sprite>(backgroundSprite)
        };

        if (name is null)
        {
            _defaultHorizontalDivider = style;
            return style;
        }

        _horizontalDividers[name] = style;
        return style;
    }
    
    public HorizontalDividerStyle GetHorizontalDivider(string? name = null)
    {
        if (name is not null && _labels.ContainsKey(name))
        {
            return _horizontalDividers[name];
        }
        
        if (_defaultHorizontalDivider is null)
        {
            throw new EngineException("No default horizontal divider style set in stylesheet.");
        }

        return _defaultHorizontalDivider;
    }
    #endregion

    #region Label
    public LabelStyle CreateLabelStyle(string? name, string fontAsset)
    {
        var style = new LabelStyle
        {
            Font = _assetManager.Get<Font>(fontAsset)
        };

        if (name is null)
        {
            _defaultLabel = style;
            return style;
        }

        _labels[name] = style;
        return style;
    }
    
    public LabelStyle GetLabel(string? name = null)
    {
        if (name is not null && _labels.ContainsKey(name))
        {
            return _labels[name];
        }
        
        if (_defaultLabel is null)
        {
            throw new EngineException("No default label style set in stylesheet.");
        }

        return _defaultLabel;
    }
    #endregion
    
    #region TextInput
    public TextInputStyle CreateTextInputStyle(string? name, string spriteAsset, string? labelStyle)
    {
        var style = new TextInputStyle
        {
            Background = _assetManager.Get<Sprite>(spriteAsset),
            Text = GetLabel(labelStyle)
        };

        if (name is null)
        {
            _defaultInput = style;
            return style;
        }

        _inputs[name] = style;
        return style;
    }
    
    public TextInputStyle GetTextInput(string? name = null)
    {
        if (name is not null && _inputs.ContainsKey(name))
        {
            return _inputs[name];
        }

        if (_defaultInput is null)
        {
            throw new EngineException("No default input style set in stylesheet.");
        }

        return _defaultInput;
    }
    #endregion
    
    #region RadioButton
    public RadioButtonStyle CreateRadioButtonStyle(string? name, string? checkedSpriteAsset, string? uncheckedSpriteAsset)
    {
        var style = new RadioButtonStyle
        {
            CheckedButton = checkedSpriteAsset is null ? null : _assetManager.Get<Sprite>(checkedSpriteAsset),
            UncheckedButton = uncheckedSpriteAsset is null ? null : _assetManager.Get<Sprite>(uncheckedSpriteAsset)
        };

        if (name is null)
        {
            _defaultRadio = style;
            return style;
        }

        _radios[name] = style;
        return style;
    }
    
    public RadioButtonStyle GetRadioButton(string? name = null)
    {
        if (name is not null && _radios.ContainsKey(name))
        {
            return _radios[name];
        }

        if (_defaultRadio is null)
        {
            throw new EngineException("No default radio button style set in stylesheet.");
        }

        return _defaultRadio;
    }
    #endregion

    #region Button
    public ButtonStyle CreateButtonStyle(string? name, string? spriteAsset)
    {
        var style = new ButtonStyle
        {
            Button = spriteAsset is not null ? _assetManager.Get<Sprite>(spriteAsset) : null,
        };

        if (name is null)
        {
            _defaultButton = style;
            return style;
        }
        
        _buttons[name] = style;
        return style;
    }

    public ButtonStyle CopyButtonStyle(ButtonStyle toCopy, string name, string spriteAsset)
    {
        var style = toCopy with {Button = _assetManager.Get<Sprite>(spriteAsset)};

        _buttons[name] = style;
        return style;
    }
    
    public ButtonStyle GetButton(string? name = null)
    {
        if (name is not null && _buttons.ContainsKey(name))
        {
            return _buttons[name];
        }

        if (_defaultButton is null)
        {
            throw new EngineException("No default button style set in stylesheet.");
        }

        return _defaultButton;
    }
    #endregion
    
    #region Window
    public WindowStyle CreateWindowStyle(string? name, string backgroundAsset)
    {
        var style = new WindowStyle
        {
            Background = _assetManager.Get<Sprite>(backgroundAsset)
        };

        if (name is null)
        {
            _defaultWindow = style;
            return style;
        }
        
        _windows[name] = style;
        return style;
    }
    
    public WindowStyle GetWindow(string? name = null)
    {
        if (name is not null && _windows.ContainsKey(name))
        {
            return _windows[name];
        }

        if (_defaultWindow is null)
        {
            throw new EngineException("No default window style set in stylesheet.");
        }

        return _defaultWindow;
    }
    #endregion

    #region Font
    public void SetFont(string? name, string assetName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _defaultFont = _assetManager.Get<Font>(assetName);
            return;
        }

        _fonts[name] = _assetManager.Get<Font>(assetName);
    }

    public Font GetFont(string? name = null)
    {
        if (name is not null && _fonts.ContainsKey(name))
        {
            return _fonts[name];
        }

        if (_defaultFont is null)
        {
            throw new EngineException("No default font set in stylesheet.");
        }

        return _defaultFont;
    }
    #endregion
}