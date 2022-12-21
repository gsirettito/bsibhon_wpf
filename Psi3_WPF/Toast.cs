using System.Windows;
using System.Windows.Controls;

namespace Psi3_WPF {
    [TemplatePart(Name = "PART_ToastIcon", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ToastTitle", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ToastText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ToastButtonClose", Type = typeof(TextBlock))]
    internal class Toast : Control {
        private TextBlock icon;
        private TextBlock title;
        private TextBlock message;
        private Button btnClose;
        private FontAwesomeSelector fonts;

        public Toast() {
            fonts = FontAwesomeSelector.Parse(Psi3_WPF.Properties.Resources.css);
        }

        public string Title { get; set; }
        public string Message { get; set; }
        /// <summary>
        /// Icon is a FontAwesome glyph
        /// </summary>
        public char Icon { get; set; }//= (char)0xf05a;
        public string IconClass { get; set; }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            icon = GetTemplateChild("PART_ToastIcon") as TextBlock;
            title = GetTemplateChild("PART_ToastTitle") as TextBlock;
            message = GetTemplateChild("PART_ToastText") as TextBlock;
            btnClose = GetTemplateChild("PART_ToastButtonClose") as Button;

            if (icon != null && Icon != 0)
                icon.Text = Icon.ToString();
            if (title != null)
                title.Text = Title;
            if (message != null)
                message.Text = Message;
            if (icon != null)
                btnClose.Click += BtnClose_Click;
            if (fonts.Classes.ContainsKey(icon.Text))
                icon.Text = ((char)fonts.Classes[icon.Text]).ToString();
            if (IconClass != null && fonts.Classes.ContainsKey(IconClass))
                icon.Text = ((char)fonts.Classes[IconClass]).ToString();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            if (this.Parent != null && this.Parent is Grid)
                (this.Parent as Grid).Children.Remove(this);
            else if (this.Parent != null && this.Parent is ContentControl)
                (this.Parent as ContentControl).Content = null;
        }
    }
}