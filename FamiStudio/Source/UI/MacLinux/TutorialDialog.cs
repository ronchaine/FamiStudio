using Gtk;
using System;
using System.Reflection;
using System.Resources;

#if FAMISTUDIO_LINUX
    using BaseWindow = Gtk.Dialog;
#else
    using BaseWindow = Gtk.Window;
#endif

namespace FamiStudio
{
    public class TutorialDialog : BaseWindow
    {
        private Label label;
        private Image image;
        private FlatButton buttonLeft;
        private FlatButton buttonRight;
        private CheckButton checkBoxDontShow;
        private int pageIndex = 0;

        private System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.None;

        public TutorialDialog()
#if FAMISTUDIO_MACOS
             : base(WindowType.Toplevel)
#endif
        {
            Init();
            WidthRequest = 756;

#if FAMISTUDIO_LINUX
            TransientFor = FamiStudioForm.Instance;
            SetPosition(WindowPosition.CenterOnParent);
#endif
        }

        private void Init()
        {
            var suffix = GLTheme.DialogScaling >= 2.0f ? "@2x" : "";
            buttonLeft  = new FlatButton(Gdk.Pixbuf.LoadFromResource($"FamiStudio.Resources.ArrowLeft{suffix}.png"));
            buttonRight = new FlatButton(Gdk.Pixbuf.LoadFromResource($"FamiStudio.Resources.ArrowRight{suffix}.png"));

            checkBoxDontShow = new CheckButton();
            checkBoxDontShow.Show();

            var checkLabel = new Label();
            checkLabel.Text = "Do not show again";
            checkLabel.Show();

            buttonLeft.Show();
            buttonLeft.ButtonPressEvent  += ButtonLeft_ButtonPressEvent;
            buttonRight.Show();
            buttonRight.ButtonPressEvent += ButtonRight_ButtonPressEvent;

            var buttonsHbox = new HBox(false, 0);
            buttonsHbox.PackStart(buttonLeft,  false, false,  0);
            buttonsHbox.PackStart(buttonRight, false, false, 0);
            buttonsHbox.Show();

            var buttonsAlign = new Alignment(1.0f, 0.5f, 0.0f, 0.0f);
            buttonsAlign.TopPadding = 5;
            buttonsAlign.Show();
            buttonsAlign.Add(buttonsHbox);

            var checkBoxHBox = new HBox(false, 0);
            checkBoxHBox.PackStart(checkBoxDontShow, false, false, 0);
            checkBoxHBox.PackStart(checkLabel, false, false, 0);
            checkBoxHBox.Show();

            var checkBoxAlign = new Alignment(0.0f, 0.5f, 0.0f, 0.0f);
            checkBoxAlign.TopPadding = 5;
            checkBoxAlign.Show();
            checkBoxAlign.Add(checkBoxHBox);

            var hbox = new HBox(true, 0);
            hbox.PackStart(checkBoxAlign);
            hbox.PackStart(buttonsAlign);
            hbox.Show();

            label = new Label();
            label.WidthRequest = 736;
            label.HeightRequest = 64;
            label.SetAlignment(0.0f, 0.0f);
            label.Wrap = true;
            label.Show();

            image = new Image();
            image.WidthRequest  = 736;
            image.HeightRequest = 414;
            image.Show();

#if FAMISTUDIO_LINUX
            var vbox = VBox;
#else
            var vbox  = new VBox();
            Add(vbox);
#endif

            vbox.PackStart(label, false, false, 0);
            vbox.PackStart(image, false, false, 0);
            vbox.PackStart(hbox, false, false, 0);
            vbox.Show();

            BorderWidth = 10;
            Resizable = false;
            Decorated = false;
            KeepAbove = true;
            Modal = true;
            SkipTaskbarHint = true;

            SetPage(0);
        }

        private void SetPage(int idx)
        {
            pageIndex = Utils.Clamp(idx, 0, TutorialMessages.Messages.Length - 1);
            image.Pixbuf = Gdk.Pixbuf.LoadFromResource($"FamiStudio.Resources.{TutorialMessages.Images[pageIndex]}").ScaleSimple(736, 414, Gdk.InterpType.Bilinear);
            label.Text = TutorialMessages.Messages[pageIndex];
            buttonLeft.Visible = pageIndex != 0;

            var suffix = GLTheme.DialogScaling >= 2.0f ? "@2x" : "";
            buttonRight.Pixbuf = pageIndex == TutorialMessages.Messages.Length - 1 ?
                Gdk.Pixbuf.LoadFromResource($"FamiStudio.Resources.Yes{suffix}.png") :
                Gdk.Pixbuf.LoadFromResource($"FamiStudio.Resources.ArrowRight{suffix}.png");
        }

        private void EndDialog(System.Windows.Forms.DialogResult res)
        {
            result = res;
#if FAMISTUDIO_LINUX
            Respond(0);
#endif
        }

        void ButtonRight_ButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            if (pageIndex == TutorialMessages.Messages.Length - 1)
            {
                EndDialog(checkBoxDontShow.Active ? 
                    System.Windows.Forms.DialogResult.OK : 
                    System.Windows.Forms.DialogResult.Cancel);
            }
            else
            {
                SetPage(pageIndex + 1);
            }
        }

        void ButtonLeft_ButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            SetPage(pageIndex - 1);
        }

        protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
        {
            if (evnt.Key == Gdk.Key.Return ||
                evnt.Key == Gdk.Key.Escape)
            {
                ButtonRight_ButtonPressEvent(null, null);
                return true;
            }
            else
            {
                return base.OnKeyPressEvent(evnt);
            }
        }

        public System.Windows.Forms.DialogResult ShowDialog(FamiStudioForm parent = null)
        {
#if FAMISTUDIO_MACOS
            Show();

            int x = parent.Bounds.Left + (parent.Bounds.Width  - Allocation.Width)  / 2;
            int y = parent.Bounds.Top  + (parent.Bounds.Height - Allocation.Height) / 2;
            Move(x, y);
            MacUtils.SetNSWindowAlwayOnTop(MacUtils.NSWindowFromGdkWindow(GdkWindow.Handle));

            while (result == System.Windows.Forms.DialogResult.None)
                Application.RunIteration();

            Hide();

            MacUtils.RestoreMainNSWindowFocus();
#else
            Run();
            Hide();
#endif

            return result;
        }
    }
}
