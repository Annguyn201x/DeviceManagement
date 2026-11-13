using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace manegment_device;

/// <summary>
/// Small drawing helpers used to fake "card" style rounded panels,
/// since ReaLTaiizor doesn't ship a plain rounded-card panel.
/// </summary>
public static class UIHelpers
{
    public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var path = new GraphicsPath();
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);

        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

    /// <summary>Clips the control itself into a rounded rectangle. Call after the control has its final Size.</summary>
    public static void ApplyRoundedRegion(Control control, int radius)
    {
        var rect = new Rectangle(0, 0, control.Width, control.Height);
        using var path = RoundedRect(rect, radius);
        control.Region = new Region(path);
    }

    /// <summary>Clips into a perfect circle. Width and Height should be equal.</summary>
    public static void ApplyCircleRegion(Control control)
    {
        var path = new GraphicsPath();
        path.AddEllipse(0, 0, control.Width, control.Height);
        control.Region = new Region(path);
    }

    public static void PaintRoundedCard(Graphics g, Rectangle bounds, Color fill, int radius, Color? border = null)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        using var path = RoundedRect(rect, radius);
        using var brush = new SolidBrush(fill);
        g.FillPath(brush, path);

        if (border.HasValue)
        {
            using var pen = new Pen(border.Value, 1);
            g.DrawPath(pen, path);
        }
    }
}