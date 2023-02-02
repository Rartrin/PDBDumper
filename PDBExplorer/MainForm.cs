using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using PDBInfoNET;
//using static System.Net.WebRequestMethods;

namespace PDBExplorer
{
	public partial class MainForm : Form
	{
		private PDB _currentPDB;

		private static bool UseShortNames;

		private struct ObjectItem
		{
			public PDB.ObjectFile ObjectFile { get; }

			public ObjectItem(PDB.ObjectFile obj)
			{
				ObjectFile = obj;
			}

			public override string ToString()
			{
				string filename = ObjectFile.FileName;

				if (UseShortNames)
					filename = Path.GetFileName(filename);

				return filename;
			}
		}

		public MainForm()
		{
			InitializeComponent();
		}

		private void UpdateDisplay()
		{
			lsbMain.Items.Clear();

			UseShortNames = mniShortNames.CheckState == CheckState.Checked;

			string filter = txtFilter.Text.Trim();

			foreach (var obj in _currentPDB.Objects)
			{
				if (string.IsNullOrWhiteSpace(filter) || obj.FileName.Contains(filter))
					lsbMain.Items.Add(new ObjectItem(obj));
			}
		}

		public void Open(string file = null)
		{
			if (!string.IsNullOrWhiteSpace(file))
			{
				DoOpen(file);
				return;
			}

			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Program Database Files|*.pdb|PDB Explorer Database File|*.pdbx|All Files|*.*";
			if (ofd.ShowDialog(this) != DialogResult.Cancel)
			{
				if (string.IsNullOrWhiteSpace(ofd.FileName))
					return;

				DoOpen(ofd.FileName);
			}
		}

		private void DoOpen(string file)
		{
			try
			{
				_currentPDB = PDB.LoadPDB(file);
			} catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				_currentPDB = null;
			}

			UpdateDisplay();
		}

		private void Export()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "*.pdbx|PDB Explorer Database File|*.*|All Files";
			if (sfd.ShowDialog(this) != DialogResult.Cancel)
			{
				if (string.IsNullOrWhiteSpace(sfd.FileName))
					return;

				DoExport(sfd.FileName);
			}
		}

		private void DoExport(string filePath)
		{
			//var json = new Dictionary<string, Dictionary<string, string[]>>();
			//foreach (var obj in _currentPDB.Objects)
			//{
			//	var symbolIndices = obj.SymbolIndices;
			//	var symbols = new string[symbolIndices.Count];
			//	for(int i=0; i<symbolIndices.Count; i++)
			//	{
			//		symbols[i] = _currentPDB.Symbols[symbolIndices[i]];
			//	}

			//	var sourceFileIndices = obj.SourceFileIndices;
			//	var sourceFiles = new string[sourceFileIndices.Count];
			//	for(int i=0; i<sourceFileIndices.Count; i++)
			//	{
			//		sourceFiles[i] = _currentPDB.SourceFiles[sourceFileIndices[i]];
			//	}

			//	json.Add(obj.FileName, new Dictionary<string, string[]>
			//	{
			//		["symbols"] = symbols,
			//		["sourceFiles"] = sourceFiles
			//	});
			//}
			//using var file = File.Create(filePath);
			//JsonSerializer.Serialize(file, json);

			var ret = new StringBuilder();
			ret.Append("{\n");
			var objects = _currentPDB.Objects;
			var symbols = _currentPDB.Symbols;
			for(int i=0; i<objects.Count; i++)
			{
				if((i%100)==0)
				{
					Console.WriteLine(i);
				}
				var obj = objects[i];
				ret.Append("\t\"");
				ret.Append(obj.FileName);
				ret.Append("\":\n\t{\n");

				var symbolIndices = obj.SymbolIndices;
				for(int s=0; s<symbolIndices.Count; s++)
				{
					ret.Append("\t\t\"");
					ret.Append(symbols[symbolIndices[s]]);
					ret.Append("\",\n");
				}
				ret.Append("\t},\n");
			}
			ret.Append("}\n");

			File.WriteAllText(filePath, ret.ToString());
		}

		private void OnSelect(object obj)
		{
			if (_currentPDB == null)
				return;

			if (obj is ObjectItem objItem)
				obvMain.PDB = new ObjectViewer.PDBObject(_currentPDB, objItem.ObjectFile);
		}

		private void UpdateFilter() => UpdateDisplay();

		private void Exit()
		{
			Application.Exit();
		}

		private void About()
		{
			MessageBox.Show(this, "This program was created by Human Gamer.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#region Event Handlers
		private void mniOpen_Click(object sender, EventArgs e)
		{
			Open();
		}

		private void mniExport_Click(object sender, EventArgs e)
		{
			Export();
		}

		private void mniExit_Click(object sender, EventArgs e)
		{
			Exit();
		}

		private void mniAbout_Click(object sender, EventArgs e)
		{
			About();
		}

		private void mniShortNames_Click(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void tsbOpen_Click(object sender, EventArgs e)
		{
			Open();
		}

		private void tsbExport_Click(object sender, EventArgs e)
		{
			Export();
		}

		private void lsbMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			OnSelect(lsbMain.Items[lsbMain.SelectedIndex]);
		}

		private void txtFilter_TextChanged(object sender, EventArgs e)
		{
			UpdateFilter();
		}
		#endregion
	}
}
