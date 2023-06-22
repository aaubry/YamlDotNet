// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;
using YamlDotNet.Samples.Helpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Samples
{
    public class DeserializeGenericObject
    {
        private readonly ITestOutputHelper output;

        public DeserializeGenericObject(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Sample(
            DisplayName = "Deserializing generic objects with basic types",
            Description = "Shows how to convert a YAML document to basic types such as String  and Dictionary"
        )]
        public void Main()
        {
            var input = new StringReader(Document);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var doc = deserializer.Deserialize<List<object>>(input);
            Console.WriteLine("## Document");
            foreach (Dictionary<object, object> item in doc)
            {
                Console.WriteLine("### Item");
                foreach (var kvp in item)
                {
                    Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
                }
            }
        }

        private const string Document = @"---
- name: Install certbot
  tags: setup
  community.general.snap:
    name: certbot
    classic: true
- name: Create symlink for Certbot binary
  tags: setup
  ansible.builtin.file:
    src: /snap/bin/certbot
    dest: /usr/bin/certbot
    state: link
  become: true
- name: Check if the certificate was already generated
  tags: generate_cert
  ansible.builtin.stat:
    path: ""/etc/letsencrypt/live/{{ domain }}/fullchain.pem""
  register: cert_fullchain_file
- name: Generate certificate manually
  tags: generate_cert
  ansible.builtin.command:
    cmd: ""certbot certonly --standalone --preferred-challenges http --agree-tos --email {{ email }} -d {{ domain }}""
    creates: ""/etc/letsencrypt/live/{{ domain }}/fullchain.pem""
  when: not cert_fullchain_file.stat.exists
";
    }
}
