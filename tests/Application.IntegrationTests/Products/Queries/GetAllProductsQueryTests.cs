﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.IntegrationTests;
using CleanArchitecture.Blazor.Application.Features.Products.Queries.Export;
using CleanArchitecture.Blazor.Application.Features.Products.Queries.GetAll;
using CleanArchitecture.Blazor.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace CleanArchitecture.Blazor.Application.IntegrationTests.Products.Queries;
using static Testing;
internal class GetAllProductsQueryTests : TestBase
{
    [SetUp]
    public async Task InitData()
    {
        await AddAsync(new Product() { Name = "Test1" });
        await AddAsync(new Product() { Name = "Test2" });
        await AddAsync(new Product() { Name = "Test3" });
        await AddAsync(new Product() { Name = "Test4" });
    }
    [Test]
    public async Task ShouldQueryAll()
    {
        var query = new GetAllProductsQuery();
        var result = await SendAsync(query);
        Assert.AreEqual(4, result.Count());
    }
    [Test]
    public async Task ShouldQueryById()
    {
        var query = new GetAllProductsQuery();
        var result = await SendAsync(query);
        var id= result.Last().Id;
        var productquery = new GetProductQuery() { Id = id };
        var product = await SendAsync(productquery);
        Assert.AreEqual(id, product.Id);
    }
}
