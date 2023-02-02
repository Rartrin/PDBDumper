#define INCLUDE_DECORATED

using Dia2;

namespace PDBInfoNET
{
	public class PDBException : Exception
	{
		public PDBException(){}

		public PDBException(string message) : base(message){}

		public PDBException(string message, Exception innerException) : base(message, innerException){}
	}

	public sealed class PDB
	{
		public sealed class ObjectFile
		{
			internal readonly List<int> symbolIndices = new List<int>();
			internal readonly List<int> sourceFileIndices = new List<int>();

			public string FileName{get;}

			public IReadOnlyList<int> SymbolIndices => symbolIndices;
			public IReadOnlyList<int> SourceFileIndices => sourceFileIndices;

			public ObjectFile(string filename)
			{
				FileName = filename;
			}
		}

		private readonly List<ObjectFile> mObjects = new List<ObjectFile>();
		private readonly List<string> mSymbols = new List<string>();
		private readonly List<string> mSourceFiles = new List<string>();

		
		public string FileName{get;}
		public uint MachineType{get;} 

		public IReadOnlyList<ObjectFile> Objects => mObjects;
		public IReadOnlyList<string> Symbols => mSymbols;
		public IReadOnlyList<string> SourceFiles => mSourceFiles;

		//public ~PDB();

		public static PDB LoadPDB(string filename)
		{
			InternalPDB ipdb = InternalPDB.LoadPDB(filename);

			// Set Machine type for getting correct register names
			uint machineType = ipdb.GlobalSymbol.MachineType switch
			{
				0x014c => 0x03,//IMAGE_FILE_MACHINE_I386 -> CV_CFL_80386
				0x0200 => 0x80,//IMAGE_FILE_MACHINE_IA64 -> CV_CFL_IA64
				0x8664 => 0xD0,//IMAGE_FILE_MACHINE_AMD64 -> CV_CFL_AMD64
				_ => 0x03//defaults to CV_CFL_80386
			};

			PDB pdb = new PDB(filename, machineType);

			// Populate all the data for this class.
			if (!pdb.PopulateData(ipdb))
			{
				throw new PDBException("Failed to populate data.");
			}

			return pdb;
		}

		private PDB(string filename, uint machineType)
		{
			FileName = filename;
			MachineType = machineType;
		}

		private bool PopulateData(InternalPDB ipdb)
		{
			// Retrieve the compilands first
			if(ipdb.GlobalSymbol.FindChildren(SymTagEnum.SymTagCompiland, null, 0/*Namespace search options*/, out var pEnumSymbols) != 0)
			{
				return false;
			}

			foreach(IDiaSymbol pCompiland in pEnumSymbols)
			{
				// Retrieve the name of the module

				string objectfileName = pCompiland.Name;

				if(objectfileName == null)
				{
					objectfileName = "<no_object_name>";
				}

				ObjectFile? objectfile = null;

				for (int i = 0; i < mObjects.Count; i++)
				{
					if (mObjects[i] == null)
					{
						continue;
					}

					if (mObjects[i].FileName == objectfileName)
					{
						objectfile = mObjects[i];
						break;
					}
				}

				if (objectfile == null)
				{
					objectfile = new ObjectFile(objectfileName);

					mObjects.Add(objectfile);
				}

				// Find all the symbols defined in this compiland
				const SymTagEnum symbolType = SymTagEnum.SymTagFunction; // use SymTagNull to get every symbol type
				if (pCompiland.FindChildren(symbolType, null, 0/*Namespace search options*/, out var pEnumChildren) == 0)
				{
					foreach(IDiaSymbol pSymbol in pEnumChildren)
					{
						string symbolname = pSymbol.UndecoratedName;
						#if INCLUDE_DECORATED
						symbolname ??= pSymbol.Name;
						#endif
						if(symbolname == null)
						{
							continue;
						}
						#if INCLUDE_DECORATED
						if (symbolname == "obj") // For some reason this symbol always exists on all objects
						{
							continue;
						}
						#endif

						// Check if it already exists
						int index = mSourceFiles.IndexOf(symbolname);

						// If not, add it
						if (index == -1)
						{
							index = mSymbols.Count;
							mSymbols.Add(symbolname);
						}

						objectfile.symbolIndices.Add(index);
					}
				}


				// Every compiland could contain multiple references to the source files which were used to build it
				// Retrieve all source files by compiland by passing NULL for the name of the source file
				if (ipdb.Session.FindFile(pCompiland, null, 0/*Namespace search options*/, out var pEnumSourceFiles)==0)
				{
					foreach(IDiaSourceFile pSourceFile in pEnumSourceFiles)
					{
						if (pSourceFile.FileName is string filename)
						{
							// Check if it already exists
							int index = mSourceFiles.IndexOf(filename);

							// If not, add it
							if (index == -1)
							{
								index = mSourceFiles.Count;
								mSourceFiles.Add(filename);
							}

							objectfile.sourceFileIndices.Add(index);
						}
					}
				}
			}
			return true;
		}
	}
}