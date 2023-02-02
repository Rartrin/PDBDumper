#include "pdb.h"
#include "VectorWrapper.h"

namespace PDBInfo
{
    public ref struct PDBException : public System::Exception
    {
        public:
            PDBException() : System::Exception()
            {

            }

            PDBException(System::String^ message) : System::Exception(message)
            {
                
            }

            PDBException(System::String^ message, System::Exception^ innerException) : System::Exception(message, innerException)
            {

            }

            PDBException(System::Runtime::Serialization::SerializationInfo^ info, System::Runtime::Serialization::StreamingContext context) : System::Exception(info, context)
            {

            }
    };

    /*public ref class ObjectFileVectorWrapper : VectorWrapper<PDBInfo::PDB::ObjectFile^>
	{
		private:
			const std::vector<::PDB::ObjectFile*>* data;

		public:
			virtual property int Count
			{
				int get() override
				{
					return data->size();
				}
			}
			
			virtual property PDBInfo::PDB::ObjectFile^ Item[int]
			{
				PDBInfo::PDB::ObjectFile^ get(int index) override
				{
					auto obj = data->operator[](index);
					return gcnew PDBInfo::PDB::ObjectFile(obj);
				}
			}
			
			ObjectFileVectorWrapper(const std::vector<::PDB::ObjectFile*>* data)
			{
				this->data = data;
			}

			~ObjectFileVectorWrapper()
			{
				this->data = nullptr;
			}
	};*/

    /*public ref class ObjectFileVectorWrapper : public ManageVectorWrapper<PDB::ObjectFile, ::PDB::ObjectFile*>
	{
		public:
			ObjectFileVectorWrapper(const std::vector<::PDB::ObjectFile*>* data):ManageVectorWrapper(data){}
	};*/

    public ref class PDB
    {
    private:
        ::PDB* mPDB;

        PDB(::PDB* pdb)
        {
            mPDB = pdb;
        }
    public:
        ref class ObjectFile
        {
        private:
            ::PDB::ObjectFile* mObjectfile;
        public:
            property System::String^ FileName
            {
                System::String^ get()
                {
                    return gcnew System::String(mObjectfile->filename);
                }
            }

            property System::Collections::Generic::IReadOnlyList<System::Int32>^ SymbolIndices
            {
                System::Collections::Generic::IReadOnlyList<System::Int32>^ get()
                {
                    return gcnew IntVectorWrapper(&mObjectfile->symbolIndices);
                    /*auto ret = gcnew System::Collections::Generic::List<System::Int32>((System::Int32)mObjectfile->symbolIndices.size());
                    for (auto index : mObjectfile->symbolIndices)
                        ret->Add((System::Int32)index);

                    return ret;*/
                }
            }

            property System::Collections::Generic::IReadOnlyList<System::Int32>^ SourceFileIndices
            {
                System::Collections::Generic::IReadOnlyList<System::Int32>^ get()
                {
                    return gcnew IntVectorWrapper(&mObjectfile->sourceFileIndices);
                    /*auto ret = gcnew System::Collections::Generic::List<System::Int32>((System::Int32)mObjectfile->sourceFileIndices.size());
                    for (auto index : mObjectfile->sourceFileIndices)
                        ret->Add((System::Int32)index);

                    return ret;*/
                }
            }
        internal:
            ObjectFile(::PDB::ObjectFile* objectfile)
            {
                mObjectfile = objectfile;
            }
        };

        private:
            List<ObjectFile^>^ objectFiles;
            List<String^>^ symbols;
            List<String^>^ sourceFiles;

        public:
        ~PDB()
        {
            delete mPDB;
        }

        property System::String^ FileName
        {
            System::String^ get()
            {
                return gcnew System::String(mPDB->getFilename());
            }
        }

        static PDB^ LoadPDB(System::String^ filename)
        {
            auto buffer = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(filename);

            char errorBuffer[256];
            errorBuffer[0] = '\0';
            ::PDB* pdb = ::PDB::LoadPDB((char*)buffer.ToPointer(), &errorBuffer);
            System::Runtime::InteropServices::Marshal::FreeHGlobal(buffer);

            if (pdb == nullptr)
            {
                if (*errorBuffer)
                    throw gcnew PDBException(gcnew System::String(errorBuffer));
                else
                    throw gcnew PDBException(gcnew System::String("Unknown Error"));
            }

            return gcnew PDB(pdb);
        }

        property System::Collections::Generic::IReadOnlyList<ObjectFile^>^ Objects
        {
            System::Collections::Generic::IReadOnlyList<ObjectFile^>^ get()
            {
                if(objectFiles == nullptr)
                {
                    auto nativeObjects = mPDB->getObjects();
                    objectFiles = gcnew List<ObjectFile^>(nativeObjects.size());
                    for(int i=0; i<nativeObjects.size(); i++)
                    {
                        objectFiles->Add(gcnew ObjectFile(nativeObjects[i]));
                    }
                }
                return objectFiles;
            }
        }

        property System::Collections::Generic::IReadOnlyList<System::String^>^ Symbols
        {
            System::Collections::Generic::IReadOnlyList<System::String^>^ get()
            {
                if(symbols == nullptr)
                {
                    auto nativeSymbols = mPDB->getSymbols();
                    symbols = gcnew List<String^>(nativeSymbols.size());
                    for(int i=0; i<nativeSymbols.size(); i++)
                    {
                        symbols->Add(gcnew String(nativeSymbols[i]));
                    }
                }
                return symbols;
            }
        }

        property System::Collections::Generic::IReadOnlyList<System::String^>^ SourceFiles
        {
            System::Collections::Generic::IReadOnlyList<System::String^>^ get()
            {
                if(symbols == nullptr)
                {
                    auto nativeSourceFiles = mPDB->getSourceFiles();
                    sourceFiles = gcnew List<String^>(nativeSourceFiles.size());
                    for(int i=0; i<nativeSourceFiles.size(); i++)
                    {
                        sourceFiles->Add(gcnew String(nativeSourceFiles[i]));
                    }
                }
                return sourceFiles;
            }
        }
    };
}