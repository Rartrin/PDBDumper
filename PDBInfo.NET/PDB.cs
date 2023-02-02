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

			public string? FileName{get;init;}

			public IReadOnlyList<int> SymbolIndices => symbolIndices;
			public IReadOnlyList<int> SourceFileIndices => sourceFileIndices;
		}

		private uint mMachineType;

		private readonly List<ObjectFile> mObjects = new List<ObjectFile>();
		private readonly List<string> mSymbols = new List<string>();
		private readonly List<string> mSourceFiles = new List<string>();

		
		public string FileName{get;private set;}

		public IReadOnlyList<ObjectFile> Objects => mObjects;
		public IReadOnlyList<string> Symbols => mSymbols;
		public IReadOnlyList<string> SourceFiles => mSourceFiles;

		//public ~PDB();

		public static PDB LoadPDB(string filename)
		{
			InternalPDB ipdb = InternalPDB.LoadPDB(filename);

			PDB pdb = new PDB();
			pdb.FileName = filename;

			// Set Machine type for getting correct register names
			uint dwMachType = ipdb.getGlobalSymbol().get_machineType();
			//if()
			{
				switch (dwMachType)
				{
					case 0x014c://IMAGE_FILE_MACHINE_I386
						pdb.mMachineType = 0x03;//CV_CFL_80386
						break;
					case 0x0200://IMAGE_FILE_MACHINE_IA64
						pdb.mMachineType = 0x80;//CV_CFL_IA64
						break;
					case 0x8664://IMAGE_FILE_MACHINE_AMD64
						pdb.mMachineType = 0xD0;//CV_CFL_AMD64
						break;
				}
			}

			// Populate all the data for this class.
			if (!pdb.PopulateData(ipdb))
			{
				throw new PDBException("Failed to populate data.");
			}

			return pdb;
		}

		private PDB()
		{
			mMachineType = 0x03;//CV_CFL_80386
		}
		private bool PopulateData(InternalPDB ipdb)
		{
			// Retrieve the compilands first
			if(ipdb.getGlobalSymbol().findChildren(SymTagEnum.SymTagCompiland, null, 0/*Namespace search options*/, out var pEnumSymbols) != 0)
			{
				return false;
			}

			foreach(IDiaSymbol pCompiland in pEnumSymbols)
			{
				// Retrieve the name of the module

				string objectfileName = pCompiland.get_name();

				if(objectfileName == null)
				{
					objectfileName = "<no_object_name>";
				}

				ObjectFile? objectfile = null;

				for (int i = 0; i < mObjects.Count(); i++)
				{
					if (mObjects[i] == null)
						continue;

					if (mObjects[i].FileName == objectfileName)
					{
						objectfile = mObjects[i];
						break;
					}
				}

				if (objectfile == null)
				{
					objectfile = new ObjectFile
					{
						FileName = objectfileName
					};

					mObjects.Add(objectfile);
				}

				// Find all the symbols defined in this compiland
				const SymTagEnum symbolType = SymTagEnum.SymTagFunction; // use SymTagNull to get every symbol type
				if (pCompiland.findChildren(symbolType, null, 0/*Namespace search options*/, out var pEnumChildren) == 0)
				{
					foreach(IDiaSymbol pSymbol in pEnumChildren)
					{

						var symbolname = pSymbol.get_undecoratedName()
						#if INCLUDE_DECORATED
						 ?? pSymbol.get_name()
						#endif
						;
						if(symbolname != null)
						{
							#if INCLUDE_DECORATED
							if (symbolname == "obj") // For some reason this symbol always exists on all objects
							{
								continue;
							}
							#endif

							int index = -1;

							// Check if it already exists
							for (int i = 0; i < mSymbols.Count; i++)
							{
								if (mSymbols[i] == null)
									continue;

								if (mSymbols[i] == symbolname)
								{
									index = i;
									break;
								}
							}

		// If not, add it
							if (index == -1)
							{
								index = mSymbols.Count;
								mSymbols.Add(symbolname);
							}

							objectfile.symbolIndices.Add(index);
						}

						//TODO: Need Release?
						//pSymbol.Release();
					}

					//TODO: Need Release?
					//pEnumChildren.Release();
				}


				// Every compiland could contain multiple references to the source files which were used to build it
				// Retrieve all source files by compiland by passing NULL for the name of the source file
				if (ipdb.getSession().findFile(pCompiland, null, 0/*Namespace search options*/, out var pEnumSourceFiles)==0)
				{
					foreach(IDiaSourceFile pSourceFile in pEnumSourceFiles)
					{
						if (pSourceFile.get_fileName() is string filename)
						{
							int index = -1;

							// Check if it already exists
							for (int i = 0; i < mSourceFiles.Count; i++)
							{
								if (mSourceFiles[i] == null)
								{
									continue;
								}

								if (mSourceFiles[i] == filename)
								{
									index = i;
									break;
								}
							}

							// If not, add it
							if (index == -1)
							{
								index = mSourceFiles.Count;
								mSourceFiles.Add(filename);
							}

							objectfile.sourceFileIndices.Add(index);
						}
						//TODO: Need Release?
						//pSourceFile.Release();
					}
					//TODO: Need Release?
					//pEnumSourceFiles.Release();
				}
				//TODO: Need Release?
				//pCompiland.Release();
			}
			//TODO: Need Release?
			//pEnumSymbols.Release();

			return true;
		}
	}
}