using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace GNU.Gettext.Examples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
			SetTexts();
        }

		private void SetTexts()
		{
            GettextResourceManager catalog = new GettextResourceManager("Examples.HelloForms.Messages");
			FormLocalizer.Localize(this, catalog);

			// Manually formatted strings
			label2.Text = catalog.GetStringFmt("This program is running as process number \"{0}\".",
			                                   System.Diagnostics.Process.GetCurrentProcess().Id);
            label3.Text = String.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 1),
				1);
            label4.Text = String.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 2),
				2);
            label5.Text = String.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 5),
				5);
		}

		private void OnLocaleChanged(object sender, EventArgs e)
		{
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo((sender as RadioButton).Text);
			FormLocalizer.Revert(this);
			SetTexts();
		}

        #region Windows Form Designer code
        private System.ComponentModel.IContainer components = null;

		private RadioButton rbEnUs;
		private RadioButton rbFrFr;
		private RadioButton rbRuRu;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label label5;
		private TextBox textBox1;

		protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "Hello, world!";
			this.Width = 400;

			rbEnUs = new RadioButton();
			rbEnUs.Text = "en-US";
			rbEnUs.Location = new Point(10, 10);
			rbEnUs.AutoSize = true;
			rbEnUs.Click += OnLocaleChanged;
			Controls.Add(rbEnUs);

			rbFrFr = new RadioButton();
			rbFrFr.Text = "fr-FR";
			rbFrFr.Location = new Point(10, 30);
			rbFrFr.AutoSize = true;
			rbFrFr.Click += OnLocaleChanged;
			Controls.Add(rbFrFr);

			rbRuRu = new RadioButton();
			rbRuRu.Text = "ru-RU";
			rbRuRu.Location = new Point(10, 50);
			rbRuRu.AutoSize = true;
			rbRuRu.Click += OnLocaleChanged;
			Controls.Add(rbRuRu);

			label1 = new Label();
			label1.Name = "label1";
			label1.Location = new Point(10, 80);
			label1.Text = "Hello, world!";
			label1.AutoSize = true;
			Controls.Add(label1);

			label2 = new Label();
			label2.Name = "label2";
			label2.Location = new Point(10, 100);
			label2.AutoSize = true;
			Controls.Add(label2);

			label3 = new Label();
			label3.Name = "label3";
			label3.Location = new Point(10, 120);
			label3.AutoSize = true;
			Controls.Add(label3);

			label4 = new Label();
			label4.Name = "label4";
			label4.Location = new Point(10, 140);
			label4.AutoSize = true;
			Controls.Add(label4);

			label5 = new Label();
			label5.Name = "label5";
			label5.Location = new Point(10, 160);
			label5.AutoSize = true;
			Controls.Add(label5);

			textBox1 = new TextBox();
			textBox1.Name = "textBox1";
			textBox1.Location = new Point(10, 180);
			textBox1.Multiline = true;
			textBox1.AutoSize = false;
			textBox1.ReadOnly = true;
			textBox1.Width = 380;
			textBox1.Height = 50;
			textBox1.Text = "Here is an example of how one might continue a very long string\nfor the common case the string represents multi-line output.\n";
			Controls.Add(textBox1);
		}
		#endregion
    }
}
