#pragma once

#include <vector>

#ifdef PDB_SHAREDLIB
#ifdef PDB_DLL
#define DLL_EXPORT __declspec(dllexport)
#else
#define DLL_EXPORT __declspec(dllimport)
#endif
#else
#define DLL_EXPORT
#endif

class InternalPDB;

class DLL_EXPORT PDB
{
public:
    struct ObjectFile
    {
        const char* filename;
        std::vector<size_t> symbolIndices;
        std::vector<size_t> sourceFileIndices;
    };

protected:
    const char* mFilename;
    unsigned long mMachineType;

    std::vector<ObjectFile*> mObjects;
    std::vector<const char*> mSymbols;
    std::vector<const char*> mSourceFiles;

public:
    ~PDB();

    static PDB* LoadPDB(const char* filename, char(* errorBuffer)[256] = nullptr);

    const char* getFilename() const { return mFilename; }

    const std::vector<ObjectFile*>& getObjects() const { return mObjects; }
    const std::vector<const char*>& getSymbols() const { return mSymbols; }
    const std::vector<const char*>& getSourceFiles() const { return mSourceFiles; }

private:
    PDB();
    bool PopulateData(InternalPDB* ipdb);
};