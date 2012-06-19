// 
// PatternCompilationException.cs
// 
// Author:
//   Matej Urbas <matej.urbas@gmail.com>
// 
// Copyright 2012 Matej Urbas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Mono.Unix;

namespace Banshee.Renamer
{
    /// <summary>
    /// Pattern compilation exceptions are thrown when a compilation of a string pattern
    /// fails for any reason (typically because of a malformed pattern).
    /// This exception provides some explanation to the user.
    /// </summary>
    public class PatternCompilationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Banshee.Renamer.PatternCompilationException"/> class.
        /// </summary>
        /// <param name='message'>
        /// A human-readable and informative message as to why the pattern compilation failed.
        /// </param>
        /// <param name='cause'>
        /// [Optional] The inner exception that may have caused this exception.
        /// </param>
        /// <exception cref='Exception'>
        /// Thrown if the message is <c>null</c> or empty.
        /// </exception>
        public PatternCompilationException (string message, Exception cause = null)
            : base(message, cause)
        {
            if (string.IsNullOrEmpty (message)) {
                throw new Exception (Catalog.GetString ("A non-empty and informative pattern compilation error message must be provided."));
            }
        }
    }
}

