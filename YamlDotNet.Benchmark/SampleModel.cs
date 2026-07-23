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

namespace YamlDotNet.Benchmark;

// A plausible, heterogeneous configuration-shaped object graph used by the serialization
// benchmarks. It deliberately mixes the scalar kinds and container shapes that exercise the
// real hot paths: strings, ints, decimals, bools, DateTimes, enums, nested objects, string
// lists, string dictionaries and nullable references. Generation is fully deterministic
// (index-derived, no randomness) so the produced YAML is byte-stable across runs.

public enum EmployeeRole
{
    Engineer,
    Manager,
    Director,
    Executive,
}

public sealed class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
    public string Country { get; set; } = "";
}

public sealed class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Salary { get; set; }
    public bool Active { get; set; }
    public DateTime HireDate { get; set; }
    public EmployeeRole Role { get; set; }
    public List<string> Skills { get; set; } = new();
    public Address? Home { get; set; }
}

public sealed class Department
{
    public string Name { get; set; } = "";
    public Address Location { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
}

public sealed class Company
{
    public string Name { get; set; } = "";
    public DateTime Founded { get; set; }
    public long Revenue { get; set; }
    public bool Public { get; set; }
    public List<Department> Departments { get; set; } = new();
}

public static class SampleModel
{
    private static readonly string[] SkillsPool =
    {
        "csharp", "yaml", "docker", "kubernetes", "sql", "azure", "react", "python",
    };

    private static readonly DateTime Epoch = new(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Company CreateCompany(int employeeCount, int departmentCount = 8)
    {
        var company = new Company
        {
            Name = "Contoso Ltd",
            Founded = new DateTime(1998, 3, 14, 0, 0, 0, DateTimeKind.Utc),
            Revenue = 1_250_000_000L,
            Public = true,
            Departments = new List<Department>(departmentCount),
        };

        var perDepartment = Math.Max(1, employeeCount / departmentCount);

        for (var d = 0; d < departmentCount; d++)
        {
            var department = new Department
            {
                Name = "Department " + d,
                Location = new Address
                {
                    Street = (100 + d) + " Main Street",
                    City = "City" + d,
                    State = "State" + (d % 4),
                    Zip = (10000 + d).ToString(),
                    Country = "US",
                },
                Metadata = new Dictionary<string, string>
                {
                    ["costCenter"] = "CC-" + d,
                    ["region"] = "region-" + (d % 3),
                    ["tier"] = (d % 2 == 0) ? "primary" : "secondary",
                },
                Employees = new List<Employee>(perDepartment),
            };

            for (var e = 0; e < perDepartment; e++)
            {
                var i = (d * perDepartment) + e;
                department.Employees.Add(new Employee
                {
                    Id = i,
                    Name = "Employee " + i,
                    Email = "employee" + i + "@contoso.example",
                    Salary = 50_000m + ((i % 500) * 137.5m),
                    Active = (i % 7) != 0,
                    HireDate = Epoch.AddDays(i % 4000),
                    Role = (EmployeeRole)(i % 4),
                    Skills = new List<string>
                    {
                        SkillsPool[i % SkillsPool.Length],
                        SkillsPool[(i + 3) % SkillsPool.Length],
                    },
                    Home = (i % 5) == 0
                        ? null
                        : new Address
                        {
                            Street = i + " Residential Ave",
                            City = "Town" + (i % 50),
                            State = "State" + (i % 4),
                            Zip = (20000 + (i % 9000)).ToString(),
                            Country = "US",
                        },
                });
            }

            company.Departments.Add(department);
        }

        return company;
    }
}
