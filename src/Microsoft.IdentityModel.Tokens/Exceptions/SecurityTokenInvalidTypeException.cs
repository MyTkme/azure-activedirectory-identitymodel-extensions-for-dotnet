//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;

namespace Microsoft.IdentityModel.Tokens
{
#if DESKTOPNET45
        [Serializable]
#endif
    /// <summary>
    /// This exception is thrown when the token type ('typ' header claim) of a JWT token is invalid.
    /// </summary>
    public class SecurityTokenInvalidTypeException : SecurityTokenValidationException
    {
        /// <summary>
        /// Gets or sets the invalid type that created the validation exception.
        /// </summary>
        public string InvalidType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityTokenInvalidTypeException"/> class.
        /// </summary>
        public SecurityTokenInvalidTypeException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityTokenInvalidTypeException"/> class.
        /// </summary>
        /// <param name="message">Additional information to be included in the exception and displayed to user.</param>
        public SecurityTokenInvalidTypeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityTokenInvalidTypeException"/> class.
        /// </summary>
        /// <param name="message">Additional information to be included in the exception and displayed to user.</param>
        /// <param name="innerException">A <see cref="Exception"/> that represents the root cause of the exception.</param>
        public SecurityTokenInvalidTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if DESKTOPNET45
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityTokenInvalidTypeException"/> class.
        /// </summary>
        /// <param name="info">the <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected SecurityTokenInvalidTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

    }
}
