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
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace YamlDotNet.Configuration
{
	/// <summary>
	/// Loads configuration from various providers.
	/// </summary>
	/// <remarks>
	/// ConfigurationLoader is always initialized with a single provider: <see cref="DotNetConfigurationProvider"/>.
	/// </remarks>
	public class ConfigurationLoader
	{
		private ConfigurationLoader()
		{
		}

		/// <summary>
		/// Gets the only instance of <see cref="ConfigurationLoader" />.
		/// </summary>
		public static readonly ConfigurationLoader Instance = new ConfigurationLoader();

		private readonly HashSet<IConfigurationProvider> providers = new HashSet<IConfigurationProvider>
		{
			new DotNetConfigurationProvider()                                     	
		};

		/// <summary>
		/// Registers a new configuration provider.
		/// </summary>
		/// <param name="provider">The configuration provider.</param>
		public void Register(IConfigurationProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			providers.Add(provider);
		}

		/// <summary>
		/// Gets the configuration section with the specified name.
		/// </summary>
		/// <param name="name">The name of the configuration section.</param>
		/// <returns></returns>
		/// <exception cref="ConfigurationErrorsException">The specified section does not exist.</exception>
		public object GetSection(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			foreach (var provider in providers)
			{
				object section = provider.GetSection(name);
				if (section != null)
				{
					return section;
				}
			}

			throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "Configuration section '{0}' does not exist.", name));
		}
	}
}
