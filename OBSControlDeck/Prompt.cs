using System.Windows.Forms;

namespace OBSControlDeck
{
    // Minimal prompt dialog for simple text input
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultText = "", IWin32Window? owner = null)
        {
            using var form = new Form();
            using var label = new Label();
            using var textBox = new TextBox();
            using var buttonOk = new Button();
            using var buttonCancel = new Button();

            form.Text = caption;
            label.Text = text;
            textBox.Text = defaultText;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Annulla";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            form.ShowInTaskbar = false;
            // ensure the prompt appears above the main window even if the main form is TopMost
            form.TopMost = true;

            try
            {
                return form.ShowDialog(owner) == DialogResult.OK ? textBox.Text.Trim() : string.Empty;
            }
            finally
            {
                // reset TopMost just before disposal to avoid side-effects
                form.TopMost = false;
            }
        }
    }
}
