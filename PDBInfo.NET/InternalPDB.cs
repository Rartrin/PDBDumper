using System.Runtime.InteropServices;
using Dia2;

namespace PDBInfoNET
{
	public class InternalPDB
	{
		protected IDiaDataSource? mDataSource;
		protected IDiaSession? mSession;
		protected IDiaSymbol? mGlobalSymbol;

		public IDiaDataSource getDataSource() => mDataSource!;
		public IDiaSession getSession() => mSession!;
		public IDiaSymbol getGlobalSymbol() => mGlobalSymbol!;

		private InternalPDB()
		{
			mDataSource = null;
			mSession = null;
			mGlobalSymbol = null;

			//TODO
			//var _ = CoInitialize(IntPtr.Zero);
		}

		//~InternalPDB()
		//{
		//	// Release DIA objects and CoUninitialize
		//	if(mGlobalSymbol!=null)
		//	{
		//		Marshal.ReleaseComObject(mGlobalSymbol);//mGlobalSymbol.Release();
		//		mGlobalSymbol = null;
		//	}

		//	if(mSession!=null)
		//	{
		//		Marshal.ReleaseComObject(mSession);//mSession.Release();
		//		mSession = null;
		//	}

		//	if(mDataSource!=null)
		//	{
		//		Marshal.ReleaseComObject(mDataSource);//mDataSource.Release();
		//		mDataSource = null;
		//	}

		//	CoUninitialize();
		//}

		public static InternalPDB LoadPDB(string file)
		{
			InternalPDB api = new InternalPDB();

			int hr;//HRESULT

			// Obtain access to the provider
			Type type = Type.GetTypeFromCLSID(new Guid("e6756135-1e65-4d17-8576-610761398c3c"))!;
			api.mDataSource = (IDiaDataSource)Activator.CreateInstance(type)!;
			//hr = CoCreateInstance(__uuidof(DiaDataSource), NULL, CLSCTX_INPROC_SERVER, __uuidof(IDiaDataSource), (void**)&api->mDataSource);
			
			//if (FAILED(hr))
			//{
			//	DBGAPI_ERROR("CoCreateInstance failed - HRESULT = %08X", hr);
			//}

			var wszExt = Path.GetExtension(file);

			if (wszExt == ".pdb")
			{
				// Open and prepare a program database (.pdb) file as a debug data source
				hr = api.mDataSource.loadDataFromPdb(file);

				if(hr!=0)
				{
					throw new Exception("loadDataFromPdb failed - HRESULT = " + hr);
				}
			}
			else
			{
				throw new Exception("Unsupported file format: " + wszExt);
			}

			// Open a session for querying symbols
			hr = api.mDataSource.openSession(out api.mSession);
			if(hr!=0)
			{
				throw new Exception("openSession failed - HRESULT = " + hr);
			}

			// Retrieve a reference to the global scope
			api.mGlobalSymbol = api.mSession.get_globalScope();

			//if (hr != S_OK)
			//{
			//	DBGAPI_ERROR("get_globalScope failed\n");
			//}
			return api;
		}

		//[DllImport("Ole32.dll")]private static extern int CoInitialize(IntPtr pvReserved);
		//[DllImport("Ole32.dll")]private static extern void CoUninitialize();
		//[DllImport("Ole32.dll")]private static extern int CoCreateInstance(REFCLSID  rclsid,
		//  IntPtr pUnkOuter,
		//  int dwClsContext,
		//  REFIID    riid,
		//  out object ppv);
	}
}