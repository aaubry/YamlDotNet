//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011 Antoine Aubry
    
//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:
    
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
    
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

﻿using System;
using YamlDotNet.Core;
using System.IO;
using YamlDotNet.Core.Events;

namespace Dumper
{
	class Program
	{
		static void Main(string[] args)
		{
			using (TextReader input = File.OpenText(args[0]))
			{
				int indent = 0;
				Parser parser = new Parser(input);
				while(parser.MoveNext())
				{
					if (parser.Current is StreamEnd || parser.Current is DocumentEnd || parser.Current is SequenceEnd || parser.Current is SequenceEnd || parser.Current is MappingEnd)
					{
						--indent;
					}
					for(int i = 0; i < indent; ++i)
					{
						Console.Write("  ");
					}

					Console.WriteLine(parser.Current.ToString());

					if (parser.Current is StreamStart || parser.Current is DocumentStart || parser.Current is SequenceStart || parser.Current is MappingStart)
					{
						++indent;
					}
				}
			}
		}
	}
}