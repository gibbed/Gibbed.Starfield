/* Copyright (c) 2023 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using StarfieldDumping;

namespace DumpReflection.Reflection
{
    internal class ClassType : BaseType<Natives.ClassType>
    {
        public static IntPtr ExpectedVftablePointer;

        private string _Name;
        private readonly List<ClassField> _Fields;
        private readonly List<ClassCast> _Upcasts;
        private readonly List<ClassCast> _Downcasts;

        public ClassType()
        {
            this._Fields = new();
            this._Upcasts = new();
            this._Downcasts = new();
        }

        public override string Name => this._Name;
        public List<ClassField> Fields => this._Fields;
        public List<ClassCast> Upcasts => this._Upcasts;
        public List<ClassCast> Downcasts => this._Downcasts;

        protected override void Read(RuntimeProcess runtime, IntPtr nativePointer, Natives.ClassType native)
        {
            this.Read(nativePointer, native.Base);

            this._Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);

            if (this.VftablePointer != ExpectedVftablePointer)
            {
                throw new InvalidOperationException();
            }

            if (this.TypeId != Natives.TypeId.Class)
            {
                throw new InvalidOperationException();
            }

            var fieldSize = Marshal.SizeOf(typeof(Natives.ClassField));
            List<ClassField> fields = new();
            for (var fieldPointer = native.Fields.Start; fieldPointer != native.Fields.End; fieldPointer += fieldSize)
            {
                fields.Add(ReadField(runtime, fieldPointer));
            }

            var castSize = Marshal.SizeOf(typeof(Natives.ClassCast));
            List<ClassCast> upcasts = new();
            for (var castPointer = native.Upcasts.Start; castPointer != native.Upcasts.End; castPointer += castSize)
            {
                upcasts.Add(ReadCast(runtime, castPointer));
            }
            List<ClassCast> downcasts = new();
            for (var castPointer = native.Downcasts.Start; castPointer != native.Downcasts.End; castPointer += castSize)
            {
                downcasts.Add(ReadCast(runtime, castPointer));
            }

            this._Fields.Clear();
            this._Fields.AddRange(fields);
            this._Upcasts.Clear();
            this._Upcasts.AddRange(upcasts);
            this._Downcasts.Clear();
            this._Downcasts.AddRange(downcasts);
        }

        private static ClassField ReadField(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.ClassField>(nativePointer);

            if (native.Unknown14 != 0 ||
                native.Unknown18 != IntPtr.Zero ||
                native.Unknown20 != IntPtr.Zero ||
                native.Unknown28 != IntPtr.Zero ||
                native.Unknown30 != -1 ||
                native.Unknown34 != -1)
            {

            }

            ClassField instance = new();
            instance.Name = runtime.ReadStringZ(native.Name, Encoding.ASCII);
            instance.TypePointer = native.Type;
            instance.Offset = native.Offset;
            return instance;
        }

        private static ClassCast ReadCast(RuntimeProcess runtime, IntPtr nativePointer)
        {
            var native = runtime.ReadStructure<Natives.ClassCast>(nativePointer);

            ClassCast instance = new();
            instance.TypePointer = native.Type;
            instance.Offset = native.Offset;
            return instance;
        }

        public override void Resolve(Dictionary<IntPtr, IType> typeMap)
        {
            base.Resolve(typeMap);

            foreach (var field in this._Fields)
            {
                if (typeMap.TryGetValue(field.TypePointer, out var fieldType) == false)
                {
                    throw new InvalidOperationException();
                }
                field.Type = fieldType;
            }

            foreach (var upcast in this._Upcasts)
            {
                if (typeMap.TryGetValue(upcast.TypePointer, out var castType) == false)
                {
                    throw new InvalidOperationException();
                }
                upcast.Type = castType;
            }

            foreach (var downcast in this._Downcasts)
            {
                if (typeMap.TryGetValue(downcast.TypePointer, out var castType) == false)
                {
                    throw new InvalidOperationException();
                }
                downcast.Type = castType;
            }
        }
    }
}
