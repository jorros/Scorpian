using System;
using System.Drawing;
using Scorpian.Asset;
using Scorpian.Graphics;
using Scorpian.SceneManagement;

namespace Scorpian.Components;

public class RenderComponent : Component
{
    public Sprite Sprite { get; set; }
    public Point Position { get; set; }
    public int AnimationFps { get; set; }

    private int _lastFrame;
    
    public RenderComponent(Sprite sprite, Point position)
    {
        Sprite = sprite;
        Position = position;
    }

    public override void OnRender(RenderContext context, float dT)
    {
        if (Sprite is AnimatedSprite animated)
        {
            var framesToUpdate = (int)Math.Floor(dT / (1.0f / AnimationFps));
            
            if (framesToUpdate > 0) {
                _lastFrame += framesToUpdate;
                _lastFrame %= animated.FramesCount;
            }
            
            context.Draw(Sprite, new RectangleF(Position, Sprite.Size), 0, Color.White, 255, _lastFrame);
            return;
        }
        
        context.Draw(Sprite, Position);
    }
}