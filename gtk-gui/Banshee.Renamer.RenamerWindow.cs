
// This file has been generated by the GUI designer. Do not modify.
namespace Banshee.Renamer
{
	public partial class RenamerWindow
	{
		private global::Gtk.VBox vboxMain;
		private global::Gtk.Frame framePattern;
		private global::Gtk.Alignment GtkAlignment2;
		private global::Gtk.Table tlayoutPattern;
		private global::Gtk.Button btnAdd;
		private global::Gtk.Button btnDelete;
		private global::Gtk.ComboBox cbCompiler;
		private global::Gtk.Entry entryPattern;
		private global::Gtk.ScrolledWindow GtkScrolledWindow1;
		private global::Gtk.NodeView tableStoredTemplates;
		private global::Gtk.Label lblStoredPatterns;
		private global::Gtk.Label lblPattern;
		private global::Gtk.Expander expander2;
		private global::Gtk.ScrolledWindow scrlHelp;
		private global::Gtk.Label lblHelp;
		private global::Gtk.Label expanderHelp;
		private global::Gtk.Expander expanderMessages;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TextView tvMessages;
		private global::Gtk.Label GtkLabel5;
		private global::Gtk.HBox hlayoutButtons;
		private global::Gtk.Button btnGenerate;
		private global::Gtk.Fixed fixed2;
		private global::Gtk.Button btnRename;
		private global::Gtk.Button btnClose;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget Banshee.Renamer.RenamerWindow
			this.Name = "Banshee.Renamer.RenamerWindow";
			this.Title = global::Mono.Unix.Catalog.GetString ("Renamer");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child Banshee.Renamer.RenamerWindow.Gtk.Container+ContainerChild
			this.vboxMain = new global::Gtk.VBox ();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.framePattern = new global::Gtk.Frame ();
			this.framePattern.Name = "framePattern";
			this.framePattern.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child framePattern.Gtk.Container+ContainerChild
			this.GtkAlignment2 = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.GtkAlignment2.Name = "GtkAlignment2";
			// Container child GtkAlignment2.Gtk.Container+ContainerChild
			this.tlayoutPattern = new global::Gtk.Table (((uint)(3)), ((uint)(3)), false);
			this.tlayoutPattern.Name = "tlayoutPattern";
			this.tlayoutPattern.RowSpacing = ((uint)(6));
			this.tlayoutPattern.ColumnSpacing = ((uint)(6));
			// Container child tlayoutPattern.Gtk.Table+TableChild
			this.btnAdd = new global::Gtk.Button ();
			this.btnAdd.CanFocus = true;
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.UseUnderline = true;
			this.btnAdd.Label = global::Mono.Unix.Catalog.GetString ("_Add");
			this.tlayoutPattern.Add (this.btnAdd);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.tlayoutPattern [this.btnAdd]));
			w1.TopAttach = ((uint)(1));
			w1.BottomAttach = ((uint)(2));
			w1.LeftAttach = ((uint)(1));
			w1.RightAttach = ((uint)(2));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tlayoutPattern.Gtk.Table+TableChild
			this.btnDelete = new global::Gtk.Button ();
			this.btnDelete.CanFocus = true;
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.UseUnderline = true;
			this.btnDelete.Label = global::Mono.Unix.Catalog.GetString ("_Delete");
			this.tlayoutPattern.Add (this.btnDelete);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.tlayoutPattern [this.btnDelete]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(2));
			w2.RightAttach = ((uint)(3));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tlayoutPattern.Gtk.Table+TableChild
			this.cbCompiler = global::Gtk.ComboBox.NewText ();
			this.cbCompiler.Name = "cbCompiler";
			this.tlayoutPattern.Add (this.cbCompiler);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.tlayoutPattern [this.cbCompiler]));
			w3.LeftAttach = ((uint)(2));
			w3.RightAttach = ((uint)(3));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tlayoutPattern.Gtk.Table+TableChild
			this.entryPattern = new global::Gtk.Entry ();
			this.entryPattern.CanFocus = true;
			this.entryPattern.Name = "entryPattern";
			this.entryPattern.IsEditable = true;
			this.entryPattern.InvisibleChar = '•';
			this.tlayoutPattern.Add (this.entryPattern);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.tlayoutPattern [this.entryPattern]));
			w4.RightAttach = ((uint)(2));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tlayoutPattern.Gtk.Table+TableChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.tableStoredTemplates = new global::Gtk.NodeView ();
			this.tableStoredTemplates.CanFocus = true;
			this.tableStoredTemplates.Name = "tableStoredTemplates";
			this.GtkScrolledWindow1.Add (this.tableStoredTemplates);
			this.tlayoutPattern.Add (this.GtkScrolledWindow1);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.tlayoutPattern [this.GtkScrolledWindow1]));
			w6.TopAttach = ((uint)(2));
			w6.BottomAttach = ((uint)(3));
			w6.RightAttach = ((uint)(3));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tlayoutPattern.Gtk.Table+TableChild
			this.lblStoredPatterns = new global::Gtk.Label ();
			this.lblStoredPatterns.Name = "lblStoredPatterns";
			this.lblStoredPatterns.Xalign = 0F;
			this.lblStoredPatterns.Yalign = 1F;
			this.lblStoredPatterns.LabelProp = global::Mono.Unix.Catalog.GetString ("<i>Stored filename patterns:</i>");
			this.lblStoredPatterns.UseMarkup = true;
			this.tlayoutPattern.Add (this.lblStoredPatterns);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.tlayoutPattern [this.lblStoredPatterns]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			this.GtkAlignment2.Add (this.tlayoutPattern);
			this.framePattern.Add (this.GtkAlignment2);
			this.lblPattern = new global::Gtk.Label ();
			this.lblPattern.Name = "lblPattern";
			this.lblPattern.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Filename pattern:</b>");
			this.lblPattern.UseMarkup = true;
			this.framePattern.LabelWidget = this.lblPattern;
			this.vboxMain.Add (this.framePattern);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vboxMain [this.framePattern]));
			w10.Position = 0;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.expander2 = new global::Gtk.Expander (null);
			this.expander2.CanFocus = true;
			this.expander2.Name = "expander2";
			// Container child expander2.Gtk.Container+ContainerChild
			this.scrlHelp = new global::Gtk.ScrolledWindow ();
			this.scrlHelp.HeightRequest = 150;
			this.scrlHelp.CanFocus = true;
			this.scrlHelp.Name = "scrlHelp";
			this.scrlHelp.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrlHelp.Gtk.Container+ContainerChild
			global::Gtk.Viewport w11 = new global::Gtk.Viewport ();
			w11.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.lblHelp = new global::Gtk.Label ();
			this.lblHelp.Name = "lblHelp";
			this.lblHelp.Xalign = 0F;
			this.lblHelp.Yalign = 0F;
			this.lblHelp.UseMarkup = true;
			this.lblHelp.Wrap = true;
			this.lblHelp.Selectable = true;
			w11.Add (this.lblHelp);
			this.scrlHelp.Add (w11);
			this.expander2.Add (this.scrlHelp);
			this.expanderHelp = new global::Gtk.Label ();
			this.expanderHelp.Name = "expanderHelp";
			this.expanderHelp.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Help:</b>");
			this.expanderHelp.UseMarkup = true;
			this.expanderHelp.UseUnderline = true;
			this.expander2.LabelWidget = this.expanderHelp;
			this.vboxMain.Add (this.expander2);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vboxMain [this.expander2]));
			w15.Position = 1;
			w15.Expand = false;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.expanderMessages = new global::Gtk.Expander (null);
			this.expanderMessages.CanFocus = true;
			this.expanderMessages.Name = "expanderMessages";
			this.expanderMessages.Expanded = true;
			// Container child expanderMessages.Gtk.Container+ContainerChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.tvMessages = new global::Gtk.TextView ();
			this.tvMessages.CanFocus = true;
			this.tvMessages.Name = "tvMessages";
			this.tvMessages.Editable = false;
			this.GtkScrolledWindow.Add (this.tvMessages);
			this.expanderMessages.Add (this.GtkScrolledWindow);
			this.GtkLabel5 = new global::Gtk.Label ();
			this.GtkLabel5.Name = "GtkLabel5";
			this.GtkLabel5.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Messages:</b>");
			this.GtkLabel5.UseMarkup = true;
			this.GtkLabel5.UseUnderline = true;
			this.expanderMessages.LabelWidget = this.GtkLabel5;
			this.vboxMain.Add (this.expanderMessages);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vboxMain [this.expanderMessages]));
			w18.Position = 2;
			// Container child vboxMain.Gtk.Box+BoxChild
			this.hlayoutButtons = new global::Gtk.HBox ();
			this.hlayoutButtons.Name = "hlayoutButtons";
			this.hlayoutButtons.Spacing = 6;
			// Container child hlayoutButtons.Gtk.Box+BoxChild
			this.btnGenerate = new global::Gtk.Button ();
			this.btnGenerate.CanFocus = true;
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.UseUnderline = true;
			this.btnGenerate.Label = global::Mono.Unix.Catalog.GetString ("_Generate filenames");
			this.hlayoutButtons.Add (this.btnGenerate);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hlayoutButtons [this.btnGenerate]));
			w19.Position = 0;
			w19.Expand = false;
			w19.Fill = false;
			// Container child hlayoutButtons.Gtk.Box+BoxChild
			this.fixed2 = new global::Gtk.Fixed ();
			this.fixed2.Name = "fixed2";
			this.fixed2.HasWindow = false;
			this.hlayoutButtons.Add (this.fixed2);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hlayoutButtons [this.fixed2]));
			w20.Position = 1;
			// Container child hlayoutButtons.Gtk.Box+BoxChild
			this.btnRename = new global::Gtk.Button ();
			this.btnRename.CanFocus = true;
			this.btnRename.Name = "btnRename";
			this.btnRename.UseUnderline = true;
			// Container child btnRename.Gtk.Container+ContainerChild
			global::Gtk.Alignment w21 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w22 = new global::Gtk.HBox ();
			w22.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w23 = new global::Gtk.Image ();
			w23.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-execute", global::Gtk.IconSize.Menu);
			w22.Add (w23);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w25 = new global::Gtk.Label ();
			w25.LabelProp = global::Mono.Unix.Catalog.GetString ("_Rename");
			w25.UseUnderline = true;
			w22.Add (w25);
			w21.Add (w22);
			this.btnRename.Add (w21);
			this.hlayoutButtons.Add (this.btnRename);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hlayoutButtons [this.btnRename]));
			w29.Position = 2;
			w29.Expand = false;
			w29.Fill = false;
			// Container child hlayoutButtons.Gtk.Box+BoxChild
			this.btnClose = new global::Gtk.Button ();
			this.btnClose.CanFocus = true;
			this.btnClose.Name = "btnClose";
			this.btnClose.UseStock = true;
			this.btnClose.UseUnderline = true;
			this.btnClose.Label = "gtk-cancel";
			this.hlayoutButtons.Add (this.btnClose);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hlayoutButtons [this.btnClose]));
			w30.Position = 3;
			w30.Expand = false;
			w30.Fill = false;
			this.vboxMain.Add (this.hlayoutButtons);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vboxMain [this.hlayoutButtons]));
			w31.PackType = ((global::Gtk.PackType)(1));
			w31.Position = 3;
			w31.Expand = false;
			w31.Fill = false;
			this.Add (this.vboxMain);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 503;
			this.DefaultHeight = 576;
			this.Show ();
			this.entryPattern.Changed += new global::System.EventHandler (this.OnPatternChanged);
		}
	}
}
