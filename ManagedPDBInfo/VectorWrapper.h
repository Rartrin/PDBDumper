#include <vector>
#include <pdb.h>
using namespace System;
using namespace System::Collections::Generic;

namespace PDBInfo
{
	generic<typename T>
	public ref class VectorWrapper : IReadOnlyList<T>
	{
		public:
			virtual property int Count
			{
				virtual int get() = 0;
			}
			
			virtual property T Item[int]
			{
				virtual T get(int index) = 0;//= IReadOnlyList<T>::default::get
			}

			virtual IEnumerator<T>^ GetEnumerator()
			{
				return gcnew Enumerator(this);
			}

		private:
			virtual System::Collections::IEnumerator^ GetEnumeratorNonGeneric() = System::Collections::IEnumerable::GetEnumerator
			{
				return GetEnumerator();
			}

			ref class Enumerator : IEnumerator<T>
			{
				private:
					VectorWrapper^ wrapper;
					int index;

					virtual property Object^ CurrentObject
					{
						virtual Object^ get() = System::Collections::IEnumerator::Current::get
						{
							return Current;
						}
					}

				public:
					Enumerator(VectorWrapper^ wrapper)
					{
						this->wrapper = wrapper;
						index = -1;
					}
					~Enumerator()
					{
						wrapper = nullptr;
					}
					virtual bool MoveNext()
					{
						index++;
						return index < wrapper->Count;
					}
					virtual void Reset()
					{
						index = -1;
					}
										
					virtual property T Current
					{
						virtual T get()
						{
							return wrapper->Item[index];
						}
					}
			};
	};

	public ref class IntVectorWrapper : VectorWrapper<Int32>
	{
		private:
			std::vector<size_t>* data;

		public:
			virtual property int Count
			{
				int get() override
				{
					return data->size();
				}
			}
			
			virtual property Int32 Item[int]
			{
				Int32 get(int index) override
				{
					return data->operator[](index);
				}
			}
			
			IntVectorWrapper(std::vector<size_t>* data)
			{
				this->data = data;
			}

			~IntVectorWrapper()
			{
				this->data = nullptr;
			}
	};

	/*public ref class StringVectorWrapper : VectorWrapper<String^>
	{
		private:
			const std::vector<const char*>* data;

		public:
			virtual property int Count
			{
				int get() override
				{
					return data->size();
				}
			}
			
			virtual property String^ Item[int]
			{
				String^ get(int index) override
				{
					auto str = data->operator[](index);
					return gcnew String(str);
				}
			}
			
			StringVectorWrapper(const std::vector<const char*>* data)
			{
				this->data = data;
			}

			~StringVectorWrapper()
			{
				this->data = nullptr;
			}
	};*/
	/*template<typename ManagedType,typename UnmanagedType>
	public ref class ManageVectorWrapper : VectorWrapper<ManagedType^>
	{
		private:
			const std::vector<UnmanagedType>* data;

		public:
			virtual property int Count
			{
				int get() override
				{
					return data->size();
				}
			}
			
			virtual property ManagedType^ Item[int]
			{
				ManagedType^ get(int index) override
				{
					auto str = data->operator[](index);
					return gcnew ManagedType(str);
				}
			}
			
			ManageVectorWrapper(const std::vector<UnmanagedType>* data)
			{
				this->data = data;
			}

			~ManageVectorWrapper()
			{
				this->data = nullptr;
			}
	};*/

	/*public ref class StringVectorWrapper : public ManageVectorWrapper<String,const char*>
	{
		public:
			StringVectorWrapper(const std::vector<const char*>* data):ManageVectorWrapper(data){}
	};*/
}