using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Library.Api.Integration
{
    public class LibraryEndpointsTests : IClassFixture<LibraryWebApplicationFactory>, IAsyncLifetime
    {
        private readonly LibraryWebApplicationFactory _factory;
        private readonly List<string> _createdIsbns = new();
        public LibraryEndpointsTests(LibraryWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
        {
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();

            var result = await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);
            var createdBook = await result.Content.ReadFromJsonAsync<Book>();

            createdBook.Should().BeEquivalentTo(book);
        }

        [Fact]
        public async Task GetBook_ReturnsBook_WhenBookExists()
        {
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();

            await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);

            var result = await httpClient.GetAsync($"/books/{book.Isbn}");
            var existing = await result.Content.ReadFromJsonAsync<Book>();

            existing.Should().BeEquivalentTo(book);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBook_ReturnsNotFound_WhenBookNotExists()
        {
            var httpClient = _factory.CreateClient();
            var isbn = GenerateIsbn();

            var result = await httpClient.GetAsync($"/books/{isbn}");
            
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        
        [Fact]
        public async Task GetAllBooks_ReturnsBooks_WhenBooksExist()
        {
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();

            await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);
            var books = new List<Book> { book };

            var result = await httpClient.GetAsync("/books");
            var existing = await result.Content.ReadFromJsonAsync<List<Book>>();

            existing.Should().BeEquivalentTo(books);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        

        private Book GenerateBook(string title="Snow Crash")
        {
            return new Book
            {
                Isbn = GenerateIsbn(),
                Title = title,
                Author = "Neal",
                PageCount = 420,
                ReleaseDate = DateTime.Now,
                ShortDescription = "The internet crashes your brain"
            };
        }

        private string GenerateIsbn()
        {
            return $"{Random.Shared.Next(100, 999)}-{Random.Shared.Next(1000000000, 2100999999)}";
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var httpClient = _factory.CreateClient();
            foreach (var isbn in _createdIsbns)
            {
                await httpClient.DeleteAsync($"/books/{isbn}");
            }
        }
    }
}
