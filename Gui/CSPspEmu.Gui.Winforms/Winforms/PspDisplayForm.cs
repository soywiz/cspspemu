﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Utils;
using CSPspEmu.Resources;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Types;
using CSPspEmu.Hle.Loader;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Hle;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Gui.Winforms.Winforms;
using System.Threading.Tasks;

namespace CSPspEmu.Gui.Winforms
{
	public unsafe partial class PspDisplayForm : Form, IMessageFilter
	{
		/// <summary>
		/// 
		/// </summary>
		internal SceCtrlData SceCtrlData;

		static internal PspDisplayForm Singleton;

		internal IGuiExternalInterface IGuiExternalInterface;
		internal InjectContext InjectContext { get { return IGuiExternalInterface.InjectContext; } }

		internal CpuProcessor CpuProcessor { get { return InjectContext.GetInstance<CpuProcessor>(); } }
		internal GpuProcessor GpuProcessor { get { return InjectContext.GetInstance<GpuProcessor>(); } }
		internal CpuConfig CpuConfig { get { return InjectContext.GetInstance<CpuConfig>(); } }
		internal GpuConfig GpuConfig { get { return InjectContext.GetInstance<GpuConfig>(); } }
		internal HleConfig HleConfig { get { return InjectContext.GetInstance<HleConfig>(); } }
		internal ElfConfig ElfConfig { get { return InjectContext.GetInstance<ElfConfig>(); } }
		internal GuiConfig GuiConfig { get { return InjectContext.GetInstance<GuiConfig>(); } }
		internal DisplayConfig DisplayConfig { get { return InjectContext.GetInstance<DisplayConfig>(); } }
		internal PspStoredConfig StoredConfig { get { return InjectContext.GetInstance<PspStoredConfig>(); } }
		internal PspMemory Memory { get { return InjectContext.GetInstance<PspMemory>(); } }
		internal PspDisplay PspDisplay { get { return InjectContext.GetInstance<PspDisplay>(); } }
		internal PspController PspController { get { return InjectContext.GetInstance<PspController>(); } }

		internal bool EnableRefreshing = true;

		float AnalogX = 0.0f, AnalogY = 0.0f;

		GLControl GLControl;

		public PspDisplayForm(IGuiExternalInterface IGuiExternalInterface)
		{
			Singleton = this;
			Application.AddMessageFilter(this);

			this.IGuiExternalInterface = IGuiExternalInterface;

			InitializeComponent();
			HandleCreated += new EventHandler(PspDisplayForm_HandleCreated);

			this.ShowIcon = ShowMenus;
			this.MainMenuStrip.Visible = ShowMenus;

			/*
			this.MainMenuStrip = null;
			this.PerformLayout();
			*/
			//this.MainMenuStrip.Visible = false;

			//GuiConfig.DefaultDisplayScale
			DisplayScale = StoredConfig.DisplayScale;
			RenderScale = StoredConfig.RenderScale;

			updateResumePause();
			UpdateCheckMenusFromConfig();
			updateDebugGpu();
			ReLoadControllerConfig();
			UpdateRecentList();
		}

		private void PspDisplayForm_Load_1(object sender, EventArgs e)
		{
			Console.WriteLine("PspDisplayForm.Thread: {0}", Thread.CurrentThread.ManagedThreadId);
			this.GLControl = new PspOpenglDisplayControl(OpenglGpuImpl.UsedGraphicsMode);
			//var GLControl = new GLControl(OpenglGpuImpl.UsedGraphicsMode);
			this.Controls.Add(this.GLControl);

			//GLControl.add
			UtilsFrameLimitingMenu.Checked = DisplayConfig.VerticalSynchronization;
			UtilsAstOptimizationsMenu.Checked = StoredConfig.EnableAstOptimizations;
			UtilsUseFastmemMenu.Checked = StoredConfig.UseFastMemory;

			Debug.WriteLine(String.Format("Now: {0}", DateTime.UtcNow));
			Debug.WriteLine(String.Format("LastCheckedTime: {0}", StoredConfig.LastCheckedTime));
			Debug.WriteLine(String.Format("Elapsed: {0}", (DateTime.UtcNow - StoredConfig.LastCheckedTime)));
			if ((DateTime.UtcNow - StoredConfig.LastCheckedTime).TotalDays > 3)
			{
				CheckForUpdates(NotifyIfNotFound: false);
			}

			if (Platform.OperatingSystem == Platform.OS.Windows)
			//if (false)
			{
				GameListComponent = new GameListComponent();

				GameListComponent.SelectedItem += (IsoFile) =>
				{
					OpenFileRealOnNewThreadLock(IsoFile);
				};
				GameListComponent.Dock = DockStyle.Fill;

				//PspConfig.IsosPath = @"e:\isos\pspa";
				if (!AutoLoad)
				{
					RefreshGameList();
					EnablePspDisplay(false);
				}
				else
				{
					EnablePspDisplay(true);
				}

				GameListComponent.Parent = this;
			}

			PspDisplay.DrawEvent += PspDisplayTick;
		}

		private void PspDisplayTick()
		{
			if (this.IsDisposed) return;

			if (this.EnableRefreshing)
			{
				Task.Run(() =>
				{
					try
					{
						this.Invoke((Action)(() =>
						{
							UpdateTitle();
							if (!GLControl.Visible) return;
							SendControllerFrame();
							Refresh();
						}));
					}
					catch (ObjectDisposedException)
					{
					}
					catch (InvalidOperationException)
					{
					}
				});
			}
		}

		public void SendControllerFrame()
		{
			if (IGuiExternalInterface.IsInitialized())
			{
				SceCtrlData.X = 0;
				SceCtrlData.Y = 0;

				bool AnalogXUpdated = false;
				bool AnalogYUpdated = false;
				if (AnalogUp) { AnalogY -= 0.4f; AnalogYUpdated = true; }
				if (AnalogDown) { AnalogY += 0.4f; AnalogYUpdated = true; }
				if (AnalogLeft) { AnalogX -= 0.4f; AnalogXUpdated = true; }
				if (AnalogRight) { AnalogX += 0.4f; AnalogXUpdated = true; }
				if (!AnalogXUpdated) AnalogX /= 2.0f;
				if (!AnalogYUpdated) AnalogY /= 2.0f;

				AnalogX = MathFloat.Clamp(AnalogX, -1.0f, 1.0f);
				AnalogY = MathFloat.Clamp(AnalogY, -1.0f, 1.0f);

				//Console.WriteLine("{0}, {1}", AnalogX, AnalogY);

				SceCtrlData.X = AnalogX;
				SceCtrlData.Y = AnalogY;

				this.PspController.InsertSceCtrlData(SceCtrlData);
			}
		}

		bool ShowMenus { get { return GuiConfig.ShowMenus; } }
		bool AutoLoad { get { return GuiConfig.AutoLoad; } }

		void PspDisplayForm_HandleCreated(object sender, EventArgs e)
		{
			LanguageUpdated();
		}

		private int _DisplayScale;

		public int MainMenuStripHeight
		{
			get 
			{
				if (ShowMenus)
				{
					//Console.WriteLine(this.menuStrip1.Height);
					//Console.ReadKey();
					//return 24;
					return this.menuStrip1.Visible ? this.menuStrip1.Height : 0;
				}
				return 0;
			}
		}

		public int RenderScale
		{
			get
			{
				return GpuProcessor.GpuImpl.ScaleViewport;
			}
			set
			{
				GpuProcessor.GpuImpl.ScaleViewport = value;
				UtilsRenderScale1xMenu.Checked = (value == 1);
				UtilsRenderScale2xMenu.Checked = (value == 2);
				UtilsRenderScale4xMenu.Checked = (value == 4);
				StoredConfig.RenderScale = value;
			}
		}

		public int DisplayScale
		{
			set
			{
				_DisplayScale = value;
				var InnerSize = new Size(480 * _DisplayScale, 272 * _DisplayScale + MainMenuStripHeight);
				MinimumSize = new Size(1, 1);
				MaximumSize = new Size(2048, 2048);
				SetClientSizeCore(InnerSize.Width, InnerSize.Height);
				MinimumSize = MaximumSize = Size;
				CenterToScreen();

				UtilsDisplay1xMenu.Checked = (_DisplayScale == 1);
				UtilsDisplay2xMenu.Checked = (_DisplayScale == 2);
				UtilsDisplay3xMenu.Checked = (_DisplayScale == 3);
				UtilsDisplay4xMenu.Checked = (_DisplayScale == 4);

				StoredConfig.DisplayScale = _DisplayScale;
			}
			get
			{
				return _DisplayScale;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
		}

		String LastText = "";

		private void UpdateTitle()
		{
			try
			{
				if (LastText != ElfConfig.GameTitle)
				{
					LastText = ElfConfig.GameTitle;
					//this.Font = new Font("Lucida Console", 16);
					if (ShowMenus)
					{
						this.Text = "Soywiz's PspEmu - " + PspGlobalConfiguration.CurrentVersion + " :: r" + PspGlobalConfiguration.CurrentVersionNumeric + " :: " + Platform.Architecture + " :: " + LastText;
					}
					else
					{
						this.Text = LastText;
					}
				}
			}
			catch
			{
			}
		}

		protected override void OnPaintBackground(PaintEventArgs PaintEventArgs)
		{
			UpdateTitle();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		protected void PauseResume(Action Action)
		{
			try
			{
				EnableRefreshing = false;
				IGuiExternalInterface.PauseResume(Action);
			}
			finally
			{
				EnableRefreshing = true;
			}
		}

		private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				var SaveFileDialog = new SaveFileDialog();
				SaveFileDialog.Filter = "PNG|*.png|All Files|*.*";
				SaveFileDialog.FileName = String.Format("{0} - screenshot.png", ElfConfig.GameTitle);
				SaveFileDialog.AddExtension = true;
				SaveFileDialog.DefaultExt = "png";
				if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var Buffer2 = GLControl.GrabScreenshot();
					//var Buffer2 = new Bitmap(480, 272);
					//Graphics.FromImage(Buffer2).DrawImage(Buffer, Point.Empty);
					Buffer2.Save(SaveFileDialog.FileName, ImageFormat.Png);
				}
			});
		}

		/*
		public void UpdateFlags(PspCtrlButtons PspCtrlButtons)
		{
			SceCtrlData.Buttons
		}
		*/

		private void frameSkippingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DisplayConfig.VerticalSynchronization = UtilsFrameLimitingMenu.Checked;
		}

		private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//Process.Start(@"http://en.blog.cballesterosvelasco.es/search/label/pspemu/?rf=csp");
			Process.Start(@"http://pspemu.soywiz.com/?rf=csp&version=" + PspGlobalConfiguration.CurrentVersion + "&version2=" + PspGlobalConfiguration.CurrentVersionNumeric);
		}

		private void xToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			DisplayScale = 1;
		}

		private void xToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			DisplayScale = 2;
		}

		private void xToolStripMenuItem3_Click(object sender, EventArgs e)
		{
			DisplayScale = 3;
		}

		private void xToolStripMenuItem4_Click(object sender, EventArgs e)
		{
			DisplayScale = 4;
		}

		private void dumpRamToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				var SaveFileDialog = new SaveFileDialog();
				SaveFileDialog.Filter = "DUMP|*.dump|All Files|*.*";
				SaveFileDialog.FileName = String.Format("memory-{0}.dump", (long)(DateTime.UtcNow - new DateTime(0)).TotalMilliseconds);
				SaveFileDialog.AddExtension = true;
				SaveFileDialog.DefaultExt = "dump";
				if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var Stream = SaveFileDialog.OpenFile();
					Stream.WriteStream(new PspMemoryStream(Memory).SliceWithBounds(PspMemory.MainSegment.Low, PspMemory.MainSegment.High - 1));
					Stream.Flush();
					Stream.Close();
				}
			});
		}

		private PspCtrlButtons GetButtonsFromKeys(Keys Key)
		{
			if (!KeyMap.ContainsKey(Key)) return PspCtrlButtons.None;
			return KeyMap[Key];
		}

		//float AnalogX, AnalogY;
		bool AnalogUp = false;
		bool AnalogDown = false;
		bool AnalogLeft = false;
		bool AnalogRight = false;

		private void TryUpdateAnalog(Keys Key, bool Press)
		{
			switch (AnalogKeyMap.GetOrDefault(Key, PspCtrlAnalog.None))
			{
				case PspCtrlAnalog.Up: AnalogUp = Press; break;
				case PspCtrlAnalog.Down: AnalogDown = Press; break;
				case PspCtrlAnalog.Left: AnalogLeft = Press; break;
				case PspCtrlAnalog.Right: AnalogRight = Press; break;
			}
		}

		bool IsFullScreen;

		private void SetFullScreen(bool SetFullScreen)
		{
			IsFullScreen = SetFullScreen;
			if (SetFullScreen)
			{
				this.TopMost = true;
				this.MainMenuStrip.Visible = false;
				this.FormBorderStyle = FormBorderStyle.None;
				this.MaximumSize = new Size(4096, 4096);
				this.WindowState = FormWindowState.Maximized;
				Cursor.Hide();
			}
			else
			{
				this.MainMenuStrip.Visible = true;
				this.TopMost = false;
				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.MaximumSize = new Size(4096, 4096);
				this.WindowState = FormWindowState.Normal;
				Cursor.Show();
			}
		}

		bool PressingShift = false;
		bool PressingCtrl = false;
		//bool PressingAlt = false;
		bool IMessageFilter.PreFilterMessage(ref Message msg)
		{
			//Console.WriteLine(msg);
			switch (msg.Msg)
			{
				// WM_SYSKEYDOWN
				case 0x104:
				// WM_SYSKEYUP
				case 0x105:
					//Console.WriteLine(msg);
					if (((int)msg.WParam) == 0xd)
					{
						if (msg.Msg == 0x104)
						{
							SetFullScreen(!IsFullScreen);
							return true;
						}
						//PressingAlt = (msg.Msg == 0x104);
					}
					break;
				case 0x100: // WM_KEYDOWN
				case 0x101: // WM_KEYUP
					var Key = (Keys)msg.WParam;
					//Console.WriteLine("{0}", msg);

					if (Key == Keys.ShiftKey)
					{
						PressingShift = (msg.Msg == 0x100);
						break;
					}
					if (Key == Keys.ControlKey)
					{
						PressingCtrl = (msg.Msg == 0x100);
						break;
					}

					if (!PressingShift && !PressingCtrl)
					{
						if (msg.Msg == 256)
						{
							PspDisplayForm.Singleton.DoKeyDown(Key);
						}
						else
						{
							PspDisplayForm.Singleton.DoKeyUp(Key);
						}
						return false;
					}
					break;
			}
			//Console.WriteLine(m);
			return false;
		}

		internal void DoKeyDown(Keys KeyCode)
		{
			//Console.WriteLine("DoKeyDown: {0}", KeyCode);
			switch (KeyCode)
			{
				case Keys.D1: DisplayScale = 1; break;
				case Keys.D2: DisplayScale = 2; break;
				case Keys.D3: DisplayScale = 3; break;
				case Keys.D4: DisplayScale = 4; break;
				//case Keys.F2: IGuiExternalInterface.ShowDebugInformation(); break;
			}

			TryUpdateAnalog(KeyCode, true);

			SceCtrlData.UpdateButtons(GetButtonsFromKeys(KeyCode), true);
		}

		internal void DoKeyUp(Keys KeyCode)
		{
			//Console.WriteLine("DoKeyUp: {0}", KeyCode);

			TryUpdateAnalog(KeyCode, false);

			SceCtrlData.UpdateButtons(GetButtonsFromKeys(KeyCode), false);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			DoKeyDown(e.KeyCode);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			DoKeyUp(e.KeyCode);
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var OpenFileDialog = new OpenFileDialog();
			var Result = default(DialogResult);

			PauseResume(() =>
			{
				OpenFileDialog.Filter = "Compatible Formats (*.elf, *.pbp, *.iso, *.cso, *.dax, *.prx)|*.elf;*.pbp;*.iso;*.cso;*.dax;*.prx|All Files|*.*";
				Result = OpenFileDialog.ShowDialog();
			});

			if (Result == System.Windows.Forms.DialogResult.OK)
			{
				OpenFileRealOnNewThreadLock(OpenFileDialog.FileName);
			}
		}

		private void OpenFileRealOnNewThreadLock(string FilePath)
		{
			var BackgroundThread = new Thread(() =>
			{
				PauseResume(() =>
				{
					OpenFileReal(FilePath);
				});
			});
			BackgroundThread.IsBackground = true;
			BackgroundThread.Start();
		}

		private void OpenFileReal(string FilePath)
		{
			this.Invoke(new Action(() =>
			{
				if (GameListComponent != null)
				{
					EnablePspDisplay(true);
				}
				this.Focus();
			}));

			OpenRecentHook(FilePath);
			IGuiExternalInterface.LoadFile(FilePath);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				new AboutForm(this, IGuiExternalInterface).ShowDialog();
			});
		}

		private void updateResumePause()
		{
			var Paused = IGuiExternalInterface.IsPaused();
			RunPauseMenu.Checked = Paused;
			RunRunResumeMenu.Checked = !Paused;
		}

		private void updateDebugGpu()
		{
			DebugTraceUnimplementedGpuMenu.Checked = GpuConfig.NoticeUnimplementedGpuCommands;
		}

		private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Resume();
			updateResumePause();
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Pause();
			updateResumePause();
		}

		private void indieGamesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://kawagames.com/?rf=csp");
		}

		private void blogcballesterosvelascoesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://en.blog.cballesterosvelasco.es/?rf=csp");
		}

		private void traceSyscallsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			HleConfig.DebugSyscalls = !HleConfig.DebugSyscalls;
			UpdateCheckMenusFromConfig();
		}

		private void traceUnimplementedGpuToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GpuConfig.NoticeUnimplementedGpuCommands = !GpuConfig.NoticeUnimplementedGpuCommands;
			UpdateCheckMenusFromConfig();
		}

		private void showThreadInfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.ShowDebugInformation();
		}

		private void debugToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void traceUnimplementedSyscallsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			HleConfig.DebugNotImplemented = !HleConfig.DebugNotImplemented;
			UpdateCheckMenusFromConfig();
		}

		private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CheckForUpdates(NotifyIfNotFound: true);
		}

		public static string GetComparableVersion(string VersionName)
		{
			string Return = "";
			foreach (var Char in VersionName)
			{
				if (Char == '/') break;
				if (Char >= '0' && Char <= '9')
				{
					Return += Char;
				}
			}
			return Return;
		}

		public void CheckForUpdates(bool NotifyIfNotFound)
		{
			var CurrentVersion = PspGlobalConfiguration.CurrentVersion;

			StoredConfig.LastCheckedTime = DateTime.UtcNow;

			var CheckForUpdatesThread = new Thread(() =>
			{
				try
				{
					var Request = HttpWebRequest.Create(new Uri("https://raw.github.com/soywiz/cspspemu/master/version_last.txt"));
					Request.Proxy = null;
					var Response = Request.GetResponse();
					var Stream = Response.GetResponseStream();
					var LastVersion = Stream.ReadAllContentsAsString(null, false);

					//int CurrentVersionInt = int.Parse(CurrentVersion);
					//int LastVersionInt = int.Parse(LastVersion);

					var ComparableCurrentVersion = GetComparableVersion(LastVersion);
					var ComparableLastVersion = GetComparableVersion(CurrentVersion);

					Console.WriteLine("{0} -> {1}", CurrentVersion, LastVersion);
					Console.WriteLine("{0} -> {1}", ComparableCurrentVersion, ComparableLastVersion);

					if (String.CompareOrdinal(ComparableCurrentVersion, ComparableLastVersion) > 0)
					{
						if (MessageBox.Show(
							String.Format("There is a new version of the emulator.\n") +
							String.Format("\n") +
							String.Format("Current Version: {0}\n", CurrentVersion) +
							String.Format("Last Version: {0}\n", LastVersion) +
							String.Format("\n") +
							String.Format("Download the new version?") +
							"",
							"New Version", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
						{
							Process.Start(@"http://pspemu.soywiz.com/?rf=csp&version=" + CurrentVersion);
						}
					}
					else
					{
						if (NotifyIfNotFound)
						{
							MessageBox.Show("You have the lastest version", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					}
				}
				catch (Exception Exception)
				{
					MessageBox.Show(Exception.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			});
			/*
			foreach (var Embed in Assembly.GetEntryAssembly().GetManifestResourceNames())
			{
				Console.WriteLine(Embed);
			}
			*/
			CheckForUpdatesThread.IsBackground = true;
			CheckForUpdatesThread.Start();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <seealso cref="http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/db6647a3-85ca-4dc4-b661-fbbd36bd561f/"/>
		private void associateWithPBPAndCSOToolStripMenuItem_Click(object sender, EventArgs e)
		{

			var Result = ProcessUtils.RunProgramInBackgroundAsRoot(ApplicationPaths.ExecutablePath, "/associate");
			Console.WriteLine(Result.OutputString);
			Console.WriteLine(Result.ErrorString);

			if (Result.Success)
			{
				MessageBox.Show("Associations done!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				MessageBox.Show("Can't associate\n\n" + Result.ErrorString, "Done", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// Now read the output of the DOS application
			//string Result = ProcessObj.StandardOutput.ReadToEnd();
		}

		private void githubcomsoywizcspspemuToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"https://github.com/soywiz/pspemu/?rf=csp");
		}

		private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"https://github.com/soywiz/cspspemu/issues/new?title=" + HttpUtility.UrlEncode("Problem with " + ElfConfig.GameTitle + "\n") + "&body=" + HttpUtility.UrlEncode("Version: " + PspGlobalConfiguration.CurrentVersion + "\nTitle: " + ElfConfig.GameTitle + "\n") + "&labels[]=" + HttpUtility.UrlEncode("Games"));
		}

		private void PspDisplayForm_Load(object sender, EventArgs e)
		{

		}
		Dictionary<ToolStripMenuItem, CultureInfo> LanguagePairs = new Dictionary<ToolStripMenuItem, CultureInfo>();

		public IEnumerable<Control> GetAll(Control control, Type type)
		{
			var controls = control.Controls.Cast<Control>();

			return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
		}

		private void LanguageUpdated()
		{
			this.UtilsLanguageMenu.DropDownItems.Clear();
			LanguagePairs.Clear();

			foreach (var AvailableLanguage in Translations.AvailableLanguages)
			{
				var CultureInfo = new CultureInfo(Translations.GetString("info", "CultureInfo", AvailableLanguage));
				var ToolStrip = new ToolStripMenuItem()
				{
					Image = Translations.GetLangFlagImage(AvailableLanguage),
					ImageScaling = ToolStripItemImageScaling.None,
					Size = new Size(152, 22),
					Text = Translations.GetString("languages", AvailableLanguage),
					Tag = CultureInfo,
				};
				ToolStrip.Click += LanguageMenuItem_Click;
				this.UtilsLanguageMenu.DropDownItems.Add(ToolStrip);

				LanguagePairs.Add(ToolStrip, CultureInfo);
			}

			foreach (var LanguagePair in LanguagePairs)
			{
				LanguagePair.Key.Tag = LanguagePair.Value;
				LanguagePair.Key.Checked = (LanguagePair.Value.CompareInfo == Thread.CurrentThread.CurrentUICulture.CompareInfo);
			}

			foreach (var Field in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				if (Field.FieldType == typeof(ToolStripMenuItem))
				{
					var ToolStripMenuItem = (ToolStripMenuItem)Field.GetValue(this);
					var Translation = Translations.GetString("menus", ToolStripMenuItem.Name);
					ToolStripMenuItem.Text = ((Translation != null) ? Translation : ToolStripMenuItem.Text);
				}
			}

			UpdateTitle();
		}

		private void LanguageMenuItem_Click(object sender, EventArgs e)
		{
			Thread.CurrentThread.CurrentUICulture = (CultureInfo)((ToolStripMenuItem)sender).Tag;
			LanguageUpdated();
		}

		GameListComponent GameListComponent;

		private void EnablePspDisplay(bool Enable)
		{
			this.GLControl.Visible = Enable;
			this.GameListComponent.Visible = !Enable;
			if (Enable)
			{
				this.GLControl.Focus();
			}
			else
			{
				this.GameListComponent.Focus();
			}
		}

		public void RefreshGameList()
		{
			ThreadPool.QueueUserWorkItem((state) =>
			{
				if (GameListComponent != null) GameListComponent.Init(StoredConfig.IsosPath, ApplicationPaths.MemoryStickRootFolder);
			});
		}

		private void RestartOptions()
		{
			StoredConfig.UseFastMemory = UtilsUseFastmemMenu.Checked;
			StoredConfig.EnableAstOptimizations = UtilsAstOptimizationsMenu.Checked;
			if (MessageBox.Show("This option requires restarting the emulator.\n\nDo you want to restart the emulator?", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Application.Restart();
			}
		}

		private void useFastAndUnsafeMemoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UtilsUseFastmemMenu.Checked = !UtilsUseFastmemMenu.Checked;
			RestartOptions();
		}

		private void UtilsAstOptimizations_Click(object sender, EventArgs e)
		{
			UtilsAstOptimizationsMenu.Checked = !UtilsAstOptimizationsMenu.Checked;
			RestartOptions();
		}

		private void DebugDumpGpuFrameMenu_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.CaptureGpuFrame();
		}

		private void UpdateCheckMenusFromConfig()
		{
			DebugTraceSyscallsMenu.Checked = HleConfig.DebugSyscalls;
			DebugTraceUnimplementedSyscallsMenu.Checked = HleConfig.DebugNotImplemented;
			DebugTraceUnimplementedGpuMenu.Checked = GpuConfig.NoticeUnimplementedGpuCommands;
			//UtilsUseFastmemMenu.Checked = !PspConfig.;
		}

		private void configureControllerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				InjectContext.NewInstance<ButtonMappingForm>().ShowDialog();
				ReLoadControllerConfig();
				StoreConfig();
			});
		}

		private void ReLoadControllerConfig()
		{
			var ControllerConfig = StoredConfig.ControllerConfig;

			AnalogKeyMap = new Dictionary<Keys, PspCtrlAnalog>();
			{
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogLeft)] = PspCtrlAnalog.Left;
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogRight)] = PspCtrlAnalog.Right;
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogUp)] = PspCtrlAnalog.Up;
				AnalogKeyMap[ParseKeyName(ControllerConfig.AnalogDown)] = PspCtrlAnalog.Down;
			}

			KeyMap = new Dictionary<Keys, PspCtrlButtons>();
			{
				KeyMap[ParseKeyName(ControllerConfig.DigitalLeft)] = PspCtrlButtons.Left;
				KeyMap[ParseKeyName(ControllerConfig.DigitalRight)] = PspCtrlButtons.Right;
				KeyMap[ParseKeyName(ControllerConfig.DigitalUp)] = PspCtrlButtons.Up;
				KeyMap[ParseKeyName(ControllerConfig.DigitalDown)] = PspCtrlButtons.Down;

				KeyMap[ParseKeyName(ControllerConfig.TriangleButton)] = PspCtrlButtons.Triangle;
				KeyMap[ParseKeyName(ControllerConfig.CrossButton)] = PspCtrlButtons.Cross;
				KeyMap[ParseKeyName(ControllerConfig.SquareButton)] = PspCtrlButtons.Square;
				KeyMap[ParseKeyName(ControllerConfig.CircleButton)] = PspCtrlButtons.Circle;

				KeyMap[ParseKeyName(ControllerConfig.StartButton)] = PspCtrlButtons.Start;
				KeyMap[ParseKeyName(ControllerConfig.SelectButton)] = PspCtrlButtons.Select;

				KeyMap[ParseKeyName(ControllerConfig.LeftTriggerButton)] = PspCtrlButtons.LeftTrigger;
				KeyMap[ParseKeyName(ControllerConfig.RightTriggerButton)] = PspCtrlButtons.RightTrigger;
			}

			Console.WriteLine("KeyMapping:");

			foreach (var Map in AnalogKeyMap)
			{
				Console.WriteLine("  '{0}' -> PspCtrlAnalog.{1}", Map.Key, Map.Value);
			}

			foreach (var Map in KeyMap)
			{
				Console.WriteLine("  '{0}' -> PspCtrlButtons.{1}", Map.Key, Map.Value);
			}
		}

		[Flags]
		public enum PspCtrlAnalog
		{
			None = 0,
			Left = (1 << 0),
			Right = (1 << 1),
			Up = (1 << 2),
			Down = (1 << 3),
		}

		private Dictionary<Keys, PspCtrlButtons> KeyMap = new Dictionary<Keys, PspCtrlButtons>();
		private Dictionary<Keys, PspCtrlAnalog> AnalogKeyMap = new Dictionary<Keys, PspCtrlAnalog>();

		private void StoreConfig()
		{
			StoredConfig.Save();
		}

		private void OpenRecentHook(string Path)
		{
			//Console.WriteLine("PATH: {0}", Path);
			var RecentFiles = StoredConfig.RecentFiles;
			try { RecentFiles.Remove(Path); } catch { }
			RecentFiles.Insert(0, Path);
			while (RecentFiles.Count > 9) RecentFiles.RemoveAt(RecentFiles.Count - 1);
			StoredConfig.Save();
			UpdateRecentList();
		}

		private void UpdateRecentList()
		{
			this.Invoke(new Action(() =>
			{
				try
				{
					var Items = FileOpenRecentMenu.DropDownItems;
					Items.Clear();
					foreach (var RecentFile in StoredConfig.RecentFiles)
					{
						var Item = new ToolStripMenuItem()
						{
							Text = RecentFile,
							ShowShortcutKeys = true,
							ShortcutKeys = ((Keys)((Keys.Control | (Keys.D1 + Items.Count)))),
						};
						Item.Click += Recent_Click;
						Items.Add(Item);
					}
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
			}));
		}

		void Recent_Click(object sender, EventArgs e)
		{
			var ToolStripMenuItem = (ToolStripMenuItem)sender;
			OpenFileRealOnNewThreadLock(ToolStripMenuItem.Text);
		}

		private static Keys ParseKeyName(string KeyName)
		{
			return (Keys)Enum.Parse(typeof(Keys), KeyName);
		}

		private void openRecentToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void PspDisplayForm_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
			{
				e.Effect = DragDropEffects.All;
			}  
		}

		private void PspDisplayForm_DragDrop(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

			this.TopMost = true;
			this.Focus();
			this.TopMost = false;

			foreach (string file in files)
			{
				OpenFileRealOnNewThreadLock(file);

				break;
			}  
		}

		private void setIsoFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var Dialog = new FolderBrowserDialog();
			Dialog.ShowNewFolderButton = true;
			//Dialog.RootFolder = PspConfig.StoredConfig.IsosPath;

			if (Dialog.ShowDialog() == DialogResult.OK)
			{
				StoredConfig.IsosPath = Dialog.SelectedPath;
				StoredConfig.Save();
			}

			RefreshGameList();
			//new FileDialog();
		}

		private void emureleasescomToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://www.emureleases.com/?rf=csp");
		}

		private void functionViewerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				InjectContext.NewInstance<FunctionViewerForm>().ShowDialog();
			});
		}

		private void UtilsRenderScale1xMenu_Click(object sender, EventArgs e)
		{
			RenderScale = 1;
		}

		private void UtilsRenderScale2xMenu_Click(object sender, EventArgs e)
		{
			RenderScale = 2;
		}

		private void UtilsRenderScale4xMenu_Click(object sender, EventArgs e)
		{
			RenderScale = 4;
		}
	}
}
