//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

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

//  Credits for this class: https://github.com/imgen

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace YamlDotNet.Dynamic.Test
{
	public class DynamicYamlTest
	{
		[Fact]
		public void TestMappingNode()
		{
			dynamic dynamicYaml = new DynamicYaml(MappingYaml);

			var receipt = (string)(dynamicYaml.Receipt);
			var firstPartNo = dynamicYaml.Items[0].part_no;
			Assert.Equal(receipt, "Oz-Ware Purchase Invoice");
		}

		[Fact]
		public void TestSequenceNode()
		{
			dynamic dynamicYaml = new DynamicYaml(SequenceYaml);

			string firstName = dynamicYaml[0].name;

			firstName.Should().Be("Me");
		}

		[Fact]
		public void TestNestedSequenceNode()
		{
			dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);

			string firstNumberAsString = dynamicYaml[0, 0];
			int firstNumberAsInt = dynamicYaml[0, 0];

			firstNumberAsString.Should().Be("1");
			firstNumberAsInt.Should().Be(1);
		}

		[Fact]
		public void TestEnumConvert()
		{
			dynamic dynamicYaml = new DynamicYaml(EnumYaml);

			StringComparison stringComparisonMode = dynamicYaml[0].stringComparisonMode;

			stringComparisonMode.Should().Be(StringComparison.CurrentCultureIgnoreCase);
		}

		[Fact]
		public void TestArrayAccess()
		{
			dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);
			dynamic[] dynamicArray = null;

			Action action = () => { dynamicArray = dynamicYaml; };

			action.ShouldNotThrow();
			dynamicArray.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestArrayConvert() {
			dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);
			dynamic[] dynamicArray = dynamicYaml;
			int[] intArray = null;

			Action action = () => { intArray = dynamicArray[0]; };

			action.ShouldNotThrow();
			intArray.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestEnumArrayConvert()
		{
			dynamic dynamicYaml = new DynamicYaml(EnumSequenceYaml);
			StringComparison[] enumArray = null;

			Action action = () => { enumArray = dynamicYaml; };

			action.ShouldNotThrow();
			enumArray.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestCollectionAccess() {
			dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);
			List<dynamic> dynamicList = null;

			Action action = () => { dynamicList = dynamicYaml; };

			action.ShouldNotThrow();
			dynamicList.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestCollectionConvert()
		{
			dynamic dynamicYaml = new DynamicYaml(NestedSequenceYaml);
			List<dynamic> dynamicList = dynamicYaml;
			List<int> intList = null;

			Action action = () => { intList = dynamicList[0]; };

			action.ShouldNotThrow();
			intList.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestEnumCollectionConvert()
		{
			dynamic dynamicYaml = new DynamicYaml(EnumSequenceYaml);
			List<StringComparison> enumList = null;

			Action action = () => { enumList = dynamicYaml; };

			action.ShouldNotThrow();
			enumList.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestDictionaryAccess()
		{
			dynamic dynamicYaml = new DynamicYaml(MappingYaml);
			Dictionary<string, dynamic> dynamicDictionary = null;

			Action action = () => { dynamicDictionary = dynamicYaml; };

			action.ShouldNotThrow();
			dynamicDictionary.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestDictionaryConvert()
		{
			dynamic dynamicYaml = new DynamicYaml(MappingYaml);
			Dictionary<string, dynamic> dynamicDictionary = dynamicYaml;
			Dictionary<string, string> stringDictonary = null;

			Action action = () => { stringDictonary = dynamicDictionary["customer"]; };

			action.ShouldNotThrow();
			stringDictonary.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestEnumDictonaryConvert()
		{
			dynamic dynamicYaml = new DynamicYaml(EnumMappingYaml);
			IDictionary<StringComparison, string> enumDict = null;

			Action action = () => { enumDict = dynamicYaml; };

			action.ShouldNotThrow();
			enumDict.Should().NotBeNull().And.NotBeEmpty();
		}

		[Fact]
		public void TestNonexistingMember()
		{
			dynamic dynamicYaml = new DynamicYaml(SequenceYaml);

			string title = dynamicYaml[0].Title;
			int? id = dynamicYaml[0].Id;

			title.Should().BeNull();
			id.Should().NotHaveValue();
		}

		[Fact]
		public void TestBool()
		{
			dynamic dynamicYaml = new DynamicYaml(MappingYaml);

			bool[] values = dynamicYaml.valid;

			values[0].Should().BeTrue();
			values[1].Should().BeTrue();
			values[2].Should().BeFalse();
			values[3].Should().BeFalse();
		}

		[Fact]
		public void TestNamingConventions()
		{
			dynamic dynamicYaml = new DynamicYaml(NamingConventionsYaml);

			string snake = dynamicYaml.snake_case;
			snake.Should().Be("should work", "the key was typed in snake case");

			string pascal = dynamicYaml.PascalCase;
			pascal.Should().Be("should work as well", "the key was typed in pascal case");

			string camel = dynamicYaml.camelCase;
			camel.Should().Be("will also work", "the key was typed in camel case");

			string upper = dynamicYaml.UPPER_CASE;
			upper.Should().Be("works as well", "the key was typed in upper case");
		}

		[Fact]
		public void TestNamingConventionsWithFirstLetterCaseInsensitivity()
		{
			dynamic dynamicYaml = new DynamicYaml(NamingConventionsYaml);

			string snake = dynamicYaml.Snake_case;
			snake.Should().Be("should work", "the key was typed in snake case");

			string pascal = dynamicYaml.pascalCase;
			pascal.Should().Be("should work as well", "the key was typed in pascal case");

			string camel = dynamicYaml.CamelCase;
			camel.Should().Be("will also work", "the key was typed in camel case");

			string upper = dynamicYaml.uPPER_CASE;
			upper.Should().Be("works as well", "the key was typed in upper case");
		}

		private const string EnumYaml = @"---
            - stringComparisonMode: CurrentCultureIgnoreCase
            - stringComparisonMode: Ordinal";

		private const string EnumMappingYaml = @"---
            CurrentCultureIgnoreCase: on
            Ordinal: off";

		private const string EnumSequenceYaml = @"---
            - CurrentCultureIgnoreCase
            - Ordinal";

		private const string SequenceYaml = @"---
            - name: Me
            - name: You";

		private const string NestedSequenceYaml = @"---
            - [1, 2, 3]
            - [4, 5, 6]";

		private const string NamingConventionsYaml = @"---
            snake_case: should work
            PascalCase: should work as well
            camelCase: will also work
            UPPER_CASE: works as well";

		private const string MappingYaml = @"---
            receipt:    Oz-Ware Purchase Invoice
            date:        2007-08-06
            valid:        [true, True, False, false]
            customer:
                given:   Dorothy
                family:  Gale

            items:
                - part_no:   A4786
                  descrip:   Water Bucket (Filled)
                  price:     1.47
                  quantity:  4

                - part_no:   E1628
                  descrip:   High Heeled ""Ruby"" Slippers
                  price:     100.27
                  quantity:  1

            bill-to:  &id001
                street: |
                        123 Tornado Alley
                        Suite 16
                city:   East Westville
                state:  KS

            ship-to:  *id001

            specialDelivery:  >
                Follow the Yellow Brick
                Road to the Emerald City.
                Pay no attention to the
                man behind the curtain.

...";
	}
}
