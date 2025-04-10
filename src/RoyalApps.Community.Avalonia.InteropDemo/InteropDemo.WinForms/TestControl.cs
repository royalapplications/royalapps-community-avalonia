using System;
using System.Drawing;
using System.Windows.Forms;

namespace InteropDemo.WinForms;

public class TestControl : UserControl
{
    public TestControl()
    {
        var rnd = new Random();
        var label = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Text = $"Instance {rnd.Next(999)}",
        };

        var textBox = new TextBox
        {
            Dock = DockStyle.Top,
        };

        label.Parent = this;
        textBox.Parent = this;
        BackColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
    }
}