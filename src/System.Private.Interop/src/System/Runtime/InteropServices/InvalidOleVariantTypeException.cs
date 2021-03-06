// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//

//

/*=============================================================================
**
** Class: InvalidOleVariantTypeException
**
** Purpose: The type of an OLE variant that was passed into the runtime is
**            invalid.
**
=============================================================================*/

using System;

namespace System.Runtime.InteropServices
{
    public class InvalidOleVariantTypeException : Exception
    {
        public InvalidOleVariantTypeException()
            : base(SR.Arg_InvalidOleVariantTypeException)
        {
            HResult = __HResults.COR_E_INVALIDOLEVARIANTTYPE;
        }

        public InvalidOleVariantTypeException(String message)
            : base(message)
        {
            HResult = __HResults.COR_E_INVALIDOLEVARIANTTYPE;
        }

        public InvalidOleVariantTypeException(String message, Exception inner)
            : base(message, inner)
        {
            HResult = __HResults.COR_E_INVALIDOLEVARIANTTYPE;
        }
    }
}
