using System;
using System.Drawing;
using System.Windows.Forms;

namespace MHAchievManager.Forms
{
    public class DateTimeForm : Form
    {
        private readonly DateTimePicker _picker;

        public DateTimeForm(DateTime initialDate)
        {
            Text = "Select Datetime";
            Size = new Size(290, 140);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;

            _picker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
                Value = initialDate,
                Location = new Point(15, 15),
                Width = 245
            };

            var btnNow = new Button
            {
                Text = "Set to Now",
                Location = new Point(15, 55),
                Width = 100
            };
            btnNow.Click += (s, e) => _picker.Value = DateTime.UtcNow;

            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(160, 55),
                Width = 100
            };

            Controls.Add(_picker);
            Controls.Add(btnNow);
            Controls.Add(btnOk);

            AcceptButton = btnOk;

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        public TimeSpan GetSelectedTimeSpan()
        {
            long unixSeconds = ((DateTimeOffset)_picker.Value).ToUnixTimeSeconds();
            return new TimeSpan(unixSeconds * TimeSpan.TicksPerSecond);
        }
    }
}