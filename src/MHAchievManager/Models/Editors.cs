using MHAchievManager.Forms;
using MHAchievManager.Services;
using OpenCalligraphy.Core.GameData;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MHAchievManager.Models
{
    public class LocaleListItem
    {
        public LocaleStringId Id { get; set; }
        public string DisplayId => ((ulong)Id).ToString();
        public string Text { get; set; }
        public int Used { get; set; }

        public override string ToString() => $"[{Id}] {Text}";
    }

    public class GuidPrototypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    long currentGuid = value is long l ? l : 0;

                    using var form = new GuidPrototypeSearchForm(currentGuid);
                    if (editorService.ShowDialog(form) == DialogResult.OK)
                    {
                        return (long)form.SelectedId;
                    }
                }
            }
            return value;
        }
    }

    public sealed class LocaleStringEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService editorService)
                return value;

            var currentId = (LocaleStringId)value;
            // Extract current LocaleStringId directly from 'value' without reflection!
            string defaultPath = "AchievementStrings.Unknown.Property";

            if (context != null)
            {
                string propName = context.PropertyDescriptor.Name;
                if (context.Instance is AchievementInfoViewModel vm)
                {
                    defaultPath = $"AchievementStrings.{vm.Id}.{propName}";
                }
            }

            using var dialog = new LocaleEditorForm(currentId, defaultPath);

            if (editorService.ShowDialog(dialog) == DialogResult.OK)
            {
                var newId = dialog.SelectedLocaleId;

                if (newId != currentId)
                {
                    context?.PropertyDescriptor?.SetValue(context.Instance, newId);
                    AchievementRepository.Instance.RebuildIndexes();
                    AchievementRepository.Instance.IsDirty = true;

                    return newId;
                }
            }

            return value;
        }
    }

    public class UnixTimeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService editorService)
                return value;

            DateTime currentDate = DateTime.UtcNow;
            if (value is TimeSpan ts && ts.Ticks > 0)
            {
                long sec = ts.Ticks / TimeSpan.TicksPerSecond;
                if (sec > 0) currentDate = DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime;
            }

            using var form = new Form
            {
                Text = "Select Datetime",
                Size = new System.Drawing.Size(290, 140),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false
            };

            var picker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
                Value = currentDate,
                Location = new System.Drawing.Point(15, 15),
                Width = 245
            };

            var btnNow = new Button { Text = "Set to Now", Location = new System.Drawing.Point(15, 55), Width = 100 };
            btnNow.Click += (s, e) => picker.Value = DateTime.UtcNow;

            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(160, 55), Width = 100 };

            form.Controls.Add(picker);
            form.Controls.Add(btnNow);
            form.Controls.Add(btnOk);
            form.AcceptButton = btnOk;

            if (editorService.ShowDialog(form) == DialogResult.OK)
            {
                long unixSeconds = ((DateTimeOffset)picker.Value).ToUnixTimeSeconds();
                return new TimeSpan(unixSeconds * TimeSpan.TicksPerSecond);
            }

            return value;
        }
    }
}