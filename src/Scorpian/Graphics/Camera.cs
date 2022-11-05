using System;
using System.Drawing;
using System.Numerics;
using Scorpian.Maths;
using static Scorpian.SDL.SDL;

namespace Scorpian.Graphics;

public class Camera
{
    private readonly SDL_Rect _viewport;
    private float _zoom;
    private float _minimumZoom;
    private float _maximumZoom;

    public RenderContext RenderContext { get; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }

    public float Zoom
    {
        get => _zoom;
        set
        {
            if ((value < MinimumZoom) || (value > MaximumZoom))
                throw new EngineException("Zoom must be between MinimumZoom and MaximumZoom");

            _zoom = value;
        }
    }

    public float MinimumZoom
    {
        get => _minimumZoom;
        set
        {
            if (value < 0)
                throw new EngineException("MinimumZoom must be greater than zero");

            if (Zoom < value)
                Zoom = MinimumZoom;

            _minimumZoom = value;
        }
    }

    public float MaximumZoom
    {
        get => _maximumZoom;
        set
        {
            if (value < 0)
                throw new EngineException("MaximumZoom must be greater than zero");

            if (Zoom > value)
                Zoom = value;

            _maximumZoom = value;
        }
    }

    public RectangleF BoundingRectangle
    {
        get
        {
            var frustum = GetBoundingFrustum();
            var corners = frustum.GetCorners();
            var topLeft = corners[0];
            var bottomRight = corners[2];
            var width = bottomRight.X - topLeft.X;
            var height = bottomRight.Y - topLeft.Y;
            return new RectangleF(topLeft.X, topLeft.Y, width, height);
        }
    }

    public Vector2 Origin { get; set; }
    public Vector2 Center => Position + Origin;

    internal Camera(RenderContext renderContext, SDL_Rect viewport)
    {
        RenderContext = renderContext;
        _viewport = viewport;
        Position = Vector2.Zero;

        MaximumZoom = 2;
        MinimumZoom = 0;

        Zoom = 1;
        Rotation = 0;
        Origin = new Vector2(viewport.w / 2f, viewport.h / 2f);
    }

    public void Move(Vector2 direction)
    {
        Position += Vector2.Transform(direction, Matrix4x4.CreateRotationZ(-Rotation));
    }

    public void Rotate(float deltaRadians)
    {
        Rotation += deltaRadians;
    }

    public void ZoomIn(float deltaZoom)
    {
        ClampZoom(Zoom + deltaZoom);
    }

    public void ZoomOut(float deltaZoom)
    {
        ClampZoom(Zoom - deltaZoom);
    }

    private void ClampZoom(float value)
    {
        if (value < MinimumZoom)
            Zoom = MinimumZoom;
        else
            Zoom = value > MaximumZoom ? MaximumZoom : value;
    }

    public void LookAt(Vector2 position)
    {
        Position = position - new Vector2(_viewport.w / 2f, _viewport.h / 2f);
    }

    public void LookAtAndClamp(Vector2 position, RectangleF clamp)
    {
        var pos = position - new Vector2(_viewport.w / 2f, _viewport.h / 2f);
        
        var screenBounds = BoundingRectangle;
        var size = GetSize(clamp.Size).ToSize();

        var start = clamp.Location.ToVector2();
        var end = new Vector2(size.Width - screenBounds.Width, size.Height - screenBounds.Height);

        var newX = Math.Clamp(pos.X, start.X, end.X);
        var newY = Math.Clamp(pos.Y, start.Y, end.Y);
        
        Position = new Vector2(newX, newY);
    }

    public SizeF GetSize(SizeF size)
    {
        return new SizeF(size.Width * Zoom, size.Height * Zoom);
    }

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        var vector = Vector2.Transform(worldPosition + new Vector2(_viewport.x, _viewport.y), GetViewMatrix());
        return vector;
    }

    public PointF WorldToScreen(PointF worldPosition)
    {
        return WorldToScreen(worldPosition.ToVector2()).ToPointF();
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition - new Vector2(_viewport.x, _viewport.y),
            GetInverseViewMatrix());
    }

    public PointF ScreenToWorld(PointF screenPosition)
    {
        return ScreenToWorld(screenPosition.ToVector2()).ToPointF();
    }

    private Matrix4x4 GetVirtualViewMatrix()
    {
        return
            Matrix4x4.CreateTranslation(new Vector3(-Position, 0.0f)) *
            Matrix4x4.CreateTranslation(new Vector3(-Origin, 0.0f)) *
            Matrix4x4.CreateRotationZ(Rotation) *
            Matrix4x4.CreateScale(Zoom, Zoom, 1) *
            Matrix4x4.CreateTranslation(new Vector3(Origin, 0.0f));
    }

    private Matrix4x4 GetViewMatrix()
    {
        return GetVirtualViewMatrix() * Matrix4x4.Identity;
    }

    public Matrix4x4 GetInverseViewMatrix()
    {
        Matrix4x4.Invert(GetViewMatrix(), out var inverted);

        return inverted;
    }
    
    private Matrix4x4 GetProjectionMatrix(Matrix4x4 viewMatrix)
    {
        var projection = Matrix4x4.CreateOrthographicOffCenter(0, _viewport.w, _viewport.h, 0, -1, 0);
        projection = Matrix4x4.Multiply(viewMatrix, projection);
        return projection;
    }

    private BoundingFrustum GetBoundingFrustum()
    {
        var viewMatrix = GetVirtualViewMatrix();
        var projectionMatrix = GetProjectionMatrix(viewMatrix);
        
        return new BoundingFrustum(projectionMatrix);
    }
}