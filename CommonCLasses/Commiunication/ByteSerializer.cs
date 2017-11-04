using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace MRL.SSL.CommonClasses.Communication
{
    /// <summary>
    /// Manange Serialization Process in our 2009 serialization
    /// </summary>
    public class ByteCoded : IDisposable
    {
        Stream _basestream;
        BinaryReader _reader;
        BinaryWriter _writer;
        /// <summary>
        /// initial the BinaryReader and BinaryWriter
        /// </summary>
        /// <param name="basestream"></param>
        public ByteCoded(Stream basestream)
        {
            this._basestream = basestream;
            _reader = new BinaryReader(_basestream);
            _writer = new BinaryWriter(_basestream);
        }
        /// <summary>
        /// Serialize a specefic object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void Serialize<T>(T data)
        {
            GCHandle pinnedObject = GCHandle.Alloc(_reader.ReadBytes(Marshal.SizeOf(typeof(T))), GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(data, pinnedObject.AddrOfPinnedObject(), false);

            }
            finally
            {
                pinnedObject.Free();   // Let the Garbage collector do it's magic

            }
        }
        /// <summary>
        /// Deserialize to a specefic object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Deserialize<T>()
        {
            T obj;
            // GHHandle.Alloc creates a Handle of my managed object
            // Pinning an object prevents the garbage collector from 
            // moving it around in memory
            GCHandle pinnedObject = GCHandle.Alloc(_reader.ReadBytes(Marshal.SizeOf(typeof(T))), GCHandleType.Pinned);
            try
            {
                obj = (T)Marshal.PtrToStructure(
                     pinnedObject.AddrOfPinnedObject(), typeof(T));
                // AddrOfPinnedObject
                // is used to get a stable pointer to the object
            }
            finally
            {
                pinnedObject.Free();   // Let the Garbage collector do it's magic
            }
            return obj;
        }
        /// <summary>
        /// Disposing the Binary Reader and Writer
        /// </summary>
        public void Dispose()
        {
            _reader.Close();
            _writer.Close();
            _basestream.Close();
        }

    }
}
