using Rive;
using Rive.Components;
using UnityEngine;

public class AnimatedTriangleDrawing : ProceduralDrawing
{
    private Path m_rectanglePath;
    private Path m_trianglePath;
    private Paint m_rectanglePaint;
    private Paint m_trianglePaint;
    private Path m_clipPath;

    // Animation properties
    private float m_currentScale = 1.0f;
    private float m_minScale = 0.5f;
    private float m_maxScale = 1.0f;
    private float m_scaleSpeed = 0.5f; // Complete cycle per second
    private bool m_scaleIncreasing = false;

    private const float TRIANGLE_SIZE_PERCENT = 0.6f;
    private const float EQUAL_SIDED_TRIANGLE_RATIO = 0.866f;
    private const float TOP_AND_BOTTOM_PADDING_COUNT = 3f;

    // Cache for frame dimensions
    private float m_lastWidth;
    private float m_lastHeight;

    // Cache for calculated dimensions
    private float m_baseTriangleHeight;
    private float m_baseTriangleWidth;
    private float m_centerX;
    private float m_centerY;
    private float m_trianglePadding;

    public override void Draw(IRenderer renderer, AABB frame, RenderContext renderContext)
    {
        float width = frame.maxX - frame.minX;
        float height = frame.maxY - frame.minY;

        // Check if frame dimensions have changed
        bool frameChanged = width != m_lastWidth || height != m_lastHeight;

        if (frameChanged)
        {
            m_lastWidth = width;
            m_lastHeight = height;
            m_trianglePadding = Mathf.Min(width, height) * 0.1f;
        }

        // This would be true if, for example, the drawing is being displayed within a shared render texture, so this drawing might overlap with other drawings if not clipped.
        if (renderContext.ClippingMode == RenderContext.ClippingModeSetting.CheckClipping)
        {
            SetupClipping(renderer, frame);
        }

        if (m_rectanglePath == null || frameChanged)
        {
            InitializeOrUpdatePaths(frame.minX, frame.minY, width, height);
        }
        else
        {
            UpdateTrianglePath();
        }

        renderer.Draw(m_rectanglePath, m_rectanglePaint);
        renderer.Draw(m_trianglePath, m_trianglePaint);
    }

    private void InitializeOrUpdatePaths(float minX, float minY, float width, float height)
    {
        // Initialize paints if they don't exist
        if (m_rectanglePaint == null)
        {
            m_rectanglePaint = new Paint
            {
                Color = new Rive.Color(0xFF111111), // Dark grey
                Style = PaintingStyle.Fill,
                Join = StrokeJoin.Round,
                Thickness = 2.0f
            };

            m_trianglePaint = new Paint
            {
                Color = new Rive.Color(0xFFFF0000), // Red
                Style = PaintingStyle.Fill,
                Join = StrokeJoin.Round,
                Thickness = 2.0f
            };
        }

        // Create or reset rectangle path
        if (m_rectanglePath == null)
        {
            m_rectanglePath = new Path();
            m_trianglePath = new Path();
        }
        else
        {
            m_rectanglePath.Reset();
        }

        // Draw rectangle to fill entire frame
        m_rectanglePath.MoveTo(minX, minY);
        m_rectanglePath.LineTo(minX + width, minY);
        m_rectanglePath.LineTo(minX + width, minY + height);
        m_rectanglePath.LineTo(minX, minY + height);
        m_rectanglePath.Close();

        // Update cached center points and triangle dimensions
        m_centerX = minX + width / 2;
        m_centerY = minY + height / 2;

        // Calculate triangle size based on available space minus padding
        float heightAfterPadding = height - (m_trianglePadding * TOP_AND_BOTTOM_PADDING_COUNT);
        m_baseTriangleHeight = heightAfterPadding * TRIANGLE_SIZE_PERCENT;
        m_baseTriangleWidth = m_baseTriangleHeight * EQUAL_SIDED_TRIANGLE_RATIO;

        // Update triangle
        UpdateTrianglePath();
    }

    private void UpdateTrianglePath()
    {
        m_trianglePath.Reset();

        float scaledHeight = m_baseTriangleHeight * m_currentScale;
        float scaledWidth = m_baseTriangleWidth * m_currentScale;

        // Draw triangle centered in the middle of the drawing area
        m_trianglePath.MoveTo(m_centerX, m_centerY - scaledHeight / 2);
        m_trianglePath.LineTo(m_centerX + scaledWidth / 2, m_centerY + scaledHeight / 2);
        m_trianglePath.LineTo(m_centerX - scaledWidth / 2, m_centerY + scaledHeight / 2);
        m_trianglePath.Close();
    }

    private void SetupClipping(IRenderer renderer, AABB frame)
    {
        if (m_clipPath == null)
        {
            m_clipPath = new Path();
        }
        else
        {
            m_clipPath.Reset();
        }

        m_clipPath.MoveTo(frame.minX, frame.minY);
        m_clipPath.LineTo(frame.maxX, frame.minY);
        m_clipPath.LineTo(frame.maxX, frame.maxY);
        m_clipPath.LineTo(frame.minX, frame.maxY);
        m_clipPath.Close();
        renderer.Clip(m_clipPath);
    }

    public override bool Advance(float deltaTime)
    {
        float scaleChange = m_scaleSpeed * deltaTime;

        if (m_scaleIncreasing)
        {
            m_currentScale += scaleChange;
            if (m_currentScale >= m_maxScale)
            {
                m_currentScale = m_maxScale;
                m_scaleIncreasing = false;
            }
        }
        else
        {
            m_currentScale -= scaleChange;
            if (m_currentScale <= m_minScale)
            {
                m_currentScale = m_minScale;
                m_scaleIncreasing = true;
            }
        }

        // Return true to indicate we need to redraw. If this were a static drawing, we would return false.
        return true;
    }
}