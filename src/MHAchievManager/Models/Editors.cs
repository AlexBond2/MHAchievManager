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

    public class AssetIdEditor : UITypeEditor
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
                    AssetId currentId = value is AssetId aid ? aid : default;

                    using var form = new AssetIdSearchForm(currentId);
                    if (editorService.ShowDialog(form) == DialogResult.OK)
                    {
                        return (AssetId)form.SelectedId;
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
                if (context.Instance is AchievementInfo info)
                {
                    defaultPath = $"AchievementStrings.{info.Id}.{propName}";
                }
            }

            using var dialog = new LocaleEditorForm(currentId, defaultPath);

            if (editorService.ShowDialog(dialog) == DialogResult.OK)
            {
                return dialog.SelectedLocaleId;
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

            using var form = new DateTimeForm(currentDate);

            if (editorService.ShowDialog(form) == DialogResult.OK)
            {
                return form.GetSelectedTimeSpan();
            }

            return value;
        }
    }
}