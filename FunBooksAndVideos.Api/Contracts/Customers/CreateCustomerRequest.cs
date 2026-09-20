using System.ComponentModel.DataAnnotations;

namespace FunBooksAndVideos.Api.Contracts.Customers;

public sealed record CreateCustomerRequest([Required, StringLength(200, MinimumLength = 1)] string Name);
